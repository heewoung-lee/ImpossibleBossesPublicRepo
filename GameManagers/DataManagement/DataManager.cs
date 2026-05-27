using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GameManagers.GoogleAuthLogin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Zenject;

namespace GameManagers.DataManagement
{
    interface ILoader<TKey, TValue>
    {
        Dictionary<TKey, TValue> MakeDict();
    }

    public interface IDataManagerInitializer
    {
        // 2026.05.19 - DataManager가 시작해둔 초기화 작업이 끝날 때까지 대기하는 진입점.
        UniTask WaitUntilInitializedAsync(CancellationToken cancellationToken);
    }

    public class DataManager : IGoogleDataBaseStruct, IDataManagerInitializer
    {
        private readonly IGoogleAuthLoginLoader _googleAuthLogin;
        private readonly IGameDataSpreadSheet _gameDataSpreadSheet;
        private readonly IAllData _allData;

        private IList<Type> _requestDataTypes;
        private Dictionary<string, Type> _loadDataTypetoDict;
        private GoogleDataBaseStruct _databaseStruct;
        private bool _isInitialized;
        private readonly UniTaskCompletionSource _initializeCompletion = new UniTaskCompletionSource();

        [Inject]
        public DataManager(
            IGoogleAuthLoginLoader googleAuthLogin,
            IGameDataSpreadSheet gameDataSpreadSheet,
            IAllData allData)
        {
            _googleAuthLogin = googleAuthLogin;
            _gameDataSpreadSheet = gameDataSpreadSheet;
            _allData = allData;
            
            if (IsPreLoadingSceneActive())
            {
                InitializeOnStartupAsync().Forget();
                return;
            }

            Initialize();
        }

        public GoogleDataBaseStruct DataBaseStruct
        {
            get
            {
                if (_databaseStruct.Equals(default(GoogleDataBaseStruct)))
                {
                    TextAsset[] jsonTexts = _googleAuthLogin.LoadGoogleAuthJsonFiles();
                    GoogleLoginWrapper googleLoginData = _googleAuthLogin.ParseJsontoGoogleAuth(jsonTexts);
                    _databaseStruct = new GoogleDataBaseStruct(
                        googleLoginData.installed.client_id,
                        googleLoginData.installed.client_secret,
                        Define.Applicationname,
                        _gameDataSpreadSheet.GameDataSpreadsheetID);
                }

                return _databaseStruct;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _requestDataTypes = LoadTypesFromAssembly();
            _loadDataTypetoDict = new Dictionary<string, Type>();
            foreach (Type typeData in _requestDataTypes)
            {
                _loadDataTypetoDict.Add(typeData.Name, typeData);
            }

            LoadDataFromPublishedCsv(_requestDataTypes);
            _isInitialized = true;
        }

        public async UniTask WaitUntilInitializedAsync(CancellationToken cancellationToken)
        {
            if (_isInitialized)
            {
                return;
            }

            // 2026.05.19 - PreLoadingScene은 초기화를 새로 시작하지 않고, 이미 진행 중인 완료 신호만 기다린다.
            await _initializeCompletion.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTaskVoid InitializeOnStartupAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await UniTask.SwitchToThreadPool();
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread();
                _initializeCompletion.TrySetException(e);
                return;
            }

            await UniTask.SwitchToMainThread();
            _initializeCompletion.TrySetResult();
        }
        
        
        // 2026.05.20 - DataManager는 ProjectContext NonLazy에서 생성되고, BaseScene은 각 SceneContext에 바인드된다.
        // 그래서 생성자 시점에는 BaseScene 주입/탐색에 의존하지 않고 현재 활성 씬만 확인한다.
        // PreLoadingScene은 화면 갱신이 멈추지 않도록 비동기 로딩하고, 테스트 씬 직행은 기존처럼 동기 로딩한다.
        // 장기적으로는 씬 이름 문자열 비교가 씬 파일명과 enum 이름 동기화에 의존하므로 부트 정책으로 분리하는 편이 더 안전하다.
        private bool IsPreLoadingSceneActive()
        {
            return SceneManager.GetActiveScene().name == Define.SceneName.PreLoadingScene.ToString();
        }

        private IList<Type> LoadTypesFromAssembly()
        {
            List<Type> targetTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name != "Assembly-CSharp")
                {
                    continue;
                }

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsInterface || type.IsAbstract)
                    {
                        continue;
                    }

                    if (typeof(IGoogleSheetData).IsAssignableFrom(type) == false)
                    {
                        continue;
                    }

                    if (_gameDataSpreadSheet.TryGetPublishedCsvUrl(type, out _) == false)
                    {
                        continue;
                    }

                    targetTypes.Add(type);
                }
            }

            return targetTypes;
        }

        public Spreadsheet GetGoogleSpreadsheet(
            GoogleDataBaseStruct databaseStruct,
            out SheetsService service,
            out string spreadsheetId,
            bool isWrite = false)
        {
            string[] readAndWriteOption;
            string tokenID;
            if (isWrite)
            {
                readAndWriteOption = new[] { SheetsService.Scope.Spreadsheets };
                tokenID = "WriteUser";
            }
            else
            {
                readAndWriteOption = new[] { SheetsService.Scope.SpreadsheetsReadonly };
                tokenID = "ReadUser";
            }

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = databaseStruct.GoogleClientID,
                    ClientSecret = databaseStruct.GoogleSecret
                },
                readAndWriteOption,
                tokenID,
                CancellationToken.None).Result;

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = databaseStruct.ApplicationName
            });

            spreadsheetId = databaseStruct.SpreedSheetID;
            return service.Spreadsheets.Get(spreadsheetId).Execute();
        }

        private void AddAllDataDictFromJsonData(string jsonFileName, string jsonString)
        {
            string typeName = GetTypeNameFromFileName(jsonFileName);
            if (string.IsNullOrEmpty(typeName))
            {
                return;
            }

            if (_loadDataTypetoDict.TryGetValue(typeName, out Type statType) == false)
            {
                UtilDebug.LogError($"Type '{typeName}' not found.");
                return;
            }

            Type keyType = FindGenericKeyType(statType);
            Type loaderType = typeof(DataToDictionary<,>).MakeGenericType(keyType, statType);

            MethodInfo method = typeof(JsonConvert)
                .GetMethods()
                .First(m => m.Name == "DeserializeObject" && m.IsGenericMethod);
            MethodInfo genericMethod = method.MakeGenericMethod(loaderType);
            object statData = genericMethod.Invoke(null, new object[] { jsonString });

            MethodInfo makeDicMethod = loaderType.GetMethod("MakeDict");
            object dict = makeDicMethod.Invoke(statData, null);

            _allData.OverWriteData(statType, dict);
        }

        private void LoadDataFromPublishedCsv(IList<Type> requestDataTypes)
        {
            foreach (Type requestType in requestDataTypes)
            {
                if (_gameDataSpreadSheet.TryGetPublishedCsvUrl(requestType, out string csvUrl) == false)
                {
                    throw new InvalidOperationException(
                        $"Published CSV URL not configured for \"{requestType.Name}\".");
                }

                string csvText = FetchPublishedCsv(csvUrl);
                if (string.IsNullOrWhiteSpace(csvText))
                {
                    throw new InvalidOperationException(
                        $"Published CSV for \"{requestType.Name}\" is empty.");
                }

                string jsonString = ParseCsvData(csvText);
                AddAllDataDictFromJsonData(requestType.Name, jsonString);
            }
        }

        private string FetchPublishedCsv(string csvUrl)
        {
            WebRequest request = WebRequest.Create(csvUrl);
            using WebResponse response = request.GetResponse();
            using Stream responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                throw new InvalidOperationException($"Failed to open CSV stream: {csvUrl}");
            }

            using StreamReader reader = new StreamReader(responseStream);
            return reader.ReadToEnd();
        }

        private string GetTypeNameFromFileName(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                UtilDebug.LogError("File name is null or empty.");
                return null;
            }

            string typeName = filepath.Replace("_", "").Replace("Data", "");
            if (typeName.Length == 0)
            {
                return null;
            }

            return typeName;
        }

        private Type FindGenericKeyType(Type typeinfo)
        {
            Type[] typeinterfaces = typeinfo.GetInterfaces();

            foreach (Type typeInterface in typeinterfaces)
            {
                if (typeInterface.IsGenericType &&
                    typeInterface.GetGenericTypeDefinition() == typeof(IKey<>))
                {
                    return typeInterface.GetGenericArguments()[0];
                }
            }

            return null;
        }

        private string ParseCsvData(string csvText)
        {
            List<string> csvRows = SplitCsvRows(csvText);
            if (csvRows.Count == 0)
            {
                return "{}";
            }

            List<string> headers = ParseCsvRow(csvRows[0]);
            if (headers.Count == 0)
            {
                return "{}";
            }

            headers[0] = headers[0].TrimStart('\uFEFF');
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            for (int i = 1; i < csvRows.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(csvRows[i]))
                {
                    continue;
                }

                List<string> row = ParseCsvRow(csvRows[i]);
                Dictionary<string, object> dict = new Dictionary<string, object>();

                for (int j = 0; j < headers.Count; j++)
                {
                    string key = headers[j].Trim();
                    object value = j < row.Count ? row[j] : string.Empty;
                    dict.Add(key, value);
                }

                list.Add(dict);
            }

            var wrapper = new { stats = list };
            return JsonConvert.SerializeObject(wrapper, Formatting.Indented);
        }

        private List<string> SplitCsvRows(string csvText)
        {
            List<string> rows = new List<string>();
            StringBuilder builder = new StringBuilder();
            bool isInQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char current = csvText[i];
                if (current == '"')
                {
                    bool isEscapedQuote = isInQuotes &&
                                          i + 1 < csvText.Length &&
                                          csvText[i + 1] == '"';
                    if (isEscapedQuote)
                    {
                        builder.Append(current);
                        builder.Append(csvText[i + 1]);
                        i++;
                        continue;
                    }

                    isInQuotes = !isInQuotes;
                }

                if (current == '\n' && isInQuotes == false)
                {
                    rows.Add(builder.ToString());
                    builder.Clear();
                    continue;
                }

                if (current != '\r')
                {
                    builder.Append(current);
                }
            }

            if (builder.Length > 0)
            {
                rows.Add(builder.ToString());
            }

            return rows;
        }

        private List<string> ParseCsvRow(string rowText)
        {
            List<string> cells = new List<string>();
            StringBuilder builder = new StringBuilder();
            bool isInQuotes = false;

            for (int i = 0; i < rowText.Length; i++)
            {
                char current = rowText[i];
                if (current == '"')
                {
                    bool isEscapedQuote = isInQuotes &&
                                          i + 1 < rowText.Length &&
                                          rowText[i + 1] == '"';
                    if (isEscapedQuote)
                    {
                        builder.Append('"');
                        i++;
                        continue;
                    }

                    isInQuotes = !isInQuotes;
                    continue;
                }

                if (current == ',' && isInQuotes == false)
                {
                    cells.Add(builder.ToString().Trim());
                    builder.Clear();
                    continue;
                }

                builder.Append(current);
            }

            cells.Add(builder.ToString().Trim());
            return cells;
        }
    }

    public interface IGoogleSheetData
    {
    }
}
