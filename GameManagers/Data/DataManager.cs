using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Data;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.GoogleAuthLogin;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.Data
{
    interface ILoader<TKey, TValue>
    {
        Dictionary<TKey, TValue> MakeDict();
    }

    public class DataManager : IGoogleDataBaseStruct
    {
        private IResourcesServices _resourcesServices;
        private IGoogleAuthLoginLoader _googleAuthLogin;
        private IGameDataSpreadSheet _gameDataSpreadSheet;
        private IAllData _allData;
       
        [Inject]
        public DataManager
            (IResourcesServices resourcesServices,
                IGoogleAuthLoginLoader googleAuthLogin,
                IGameDataSpreadSheet gameDataSpreadSheet,
                IAllData allData)
        {
            _resourcesServices = resourcesServices;
            _googleAuthLogin = googleAuthLogin;
            _gameDataSpreadSheet = gameDataSpreadSheet;
            _allData = allData;
            Initialize();
        } 
        
        
        private IList<Type> _requestDataTypes;
        private Dictionary<string, Type> _loadDataTypetoDict;//필수조건은 아니기 때문에 인터페이스를 안만듦
        private GoogleDataBaseStruct _databaseStruct;
        
        
        public GoogleDataBaseStruct DataBaseStruct
        {
            get
            {
                if (_databaseStruct.Equals(default(GoogleDataBaseStruct)))
                {
                    TextAsset[] jsonTexts = _googleAuthLogin.LoadGoogleAuthJsonFiles();
                    GoogleLoginWrapper googleLoginData = _googleAuthLogin.ParseJsontoGoogleAuth(jsonTexts);
                    _databaseStruct = new GoogleDataBaseStruct(googleLoginData.installed.client_id, googleLoginData.installed.client_secret, Define.Applicationname, _gameDataSpreadSheet.GameDataSpreadsheetID);
                }
                return _databaseStruct;
            }
        }

        public void Initialize()
        {
            _requestDataTypes = LoadTypesFromAssembly();
            _loadDataTypetoDict = new Dictionary<string, Type>();
            foreach (Type typeData in _requestDataTypes)
            {
                _loadDataTypetoDict.Add(typeData.Name, typeData);
            }
            //데이터 로드
            LoadDataFromGoogleSheets(_requestDataTypes);
        }
        private IList<Type> LoadTypesFromAssembly()
        {
            List<Type> targetTypes = new List<Type>();

            // 현재 도메인의 어셈블리 중
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // 내 스크립트 검사
                if (assembly.GetName().Name != "Assembly-CSharp") 
                    continue;

                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    // 인터페이스 아니고 추상 클래스(abstract)가 아니고 IGoogleSheetData 인터페이스를 구현했는지 확인
                    if (type.IsInterface == false && type.IsAbstract == false 
                                                  && typeof(IGoogleSheetData).IsAssignableFrom(type))
                    {
                        targetTypes.Add(type);
                    }
                }
            }

            return targetTypes;
        }
        public Spreadsheet GetGoogleSpreadsheet(GoogleDataBaseStruct databaseStruct,out SheetsService service,out string spreadsheetId,bool isWrite = false)
        {
            // 구글 스프레드시트에서 데이터 로드하는 로직
            // 반환값: Dictionary<string, string> (sheetName, jsonString)

            // 구글 인증 및 서비스 생성
            try
            {
                string[] readAndWriteOption;
                string tokenID;
                if (isWrite == true)
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

                // 스프레드시트 요청
                Spreadsheet spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
                return spreadsheet;
            }
            catch
            {
                throw;
            }
        }

        private bool LoadAllDataFromLocal(string typeName)
        {
            TextAsset[] jsonFiles = _resourcesServices.LoadAll<TextAsset>("Data");

            foreach (TextAsset jsonFile in jsonFiles)
            {
                if (typeName != GetTypeNameFromFileName(jsonFile.name))
                {
                    continue;
                }
                else
                {
                    AddAllDataDictFromJsonData(jsonFile.name, jsonFile.text);
                    return true;
                }
            }
            return false;
        }

        private void AddAllDataDictFromJsonData(string jsonFileName, string jsonString)
        {
            string typeName = GetTypeNameFromFileName(jsonFileName);
            if (string.IsNullOrEmpty(typeName))
                return;
            
            //Type statType = Type.GetType($"{typeName}, Assembly-CSharp");
            //6.11일 수정 타입 클래스들의 네임스페이스를 씌우니 GetType에서 정확한 네임스페이스를 요구함.
            //따라서 Init부분에 필요한 로드 타입을 미리 캐싱 해놓고 현재 메소드에서 로드한 json의 타입이름 들만 가져와
            //캐싱되어있는 딕셔너리를 가져옴
            Type statType = null;

            if (_loadDataTypetoDict.TryGetValue(typeName, out Type loadType) == true)
            {
                statType = _loadDataTypetoDict[typeName];
            }
            if (statType == null)
            {
                Debug.LogError($"Type '{typeName}' not found.");
                return;
            }

            Type keyType = FindGenericKeyType(statType);
            Type loaderType = typeof(DataToDictionary<,>).MakeGenericType(keyType, statType);

            MethodInfo method = typeof(JsonConvert).GetMethods().First(m => m.Name == "DeserializeObject" && m.IsGenericMethod);
            MethodInfo genericMethod = method.MakeGenericMethod(loaderType);
            object statData = genericMethod.Invoke(null, new object[] { jsonString });

            MethodInfo makeDicMethod = loaderType.GetMethod("MakeDict");
            object dict = makeDicMethod.Invoke(statData, null);

            _allData.OverWriteData(statType, dict);
        }

        private void SaveDataToFile(string fileName, string jsonString)
        {
            string directoryPath = Path.Combine(Application.dataPath, "Resources/Data");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, $"{fileName}.json");

            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                if (existingJson == jsonString)
                {
                    Debug.Log($"{fileName} 데이터에 변경 사항이 없습니다.");
                    return;
                }
            }
            File.WriteAllText(filePath, jsonString);
            Debug.Log($"{fileName} 데이터를 로컬에 저장했습니다.");
        }
        
        private void LoadDataFromGoogleSheets(IList<Type> requestDataTypes)
        {
            // 구글 스프레드시트에서 데이터 로드하는 로직
            // 반환값: Dictionary<string, string> (sheetName, jsonString)

            // 구글 인증 및 서비스 생성
            try
            {
                // 스프레드시트 요청
                Spreadsheet spreadsheet = GetGoogleSpreadsheet(DataBaseStruct, out SheetsService service,out string spreadsheetId);
                foreach (Type requestType in requestDataTypes)
                {
                    Sheet sheet = null;
                    for (int i = 0; i < spreadsheet.Sheets.Count; i++)
                    {
                        if (GetTypeNameFromFileName(spreadsheet.Sheets[i].Properties.Title) != requestType.Name)
                        {
                            continue;
                        }
                        else
                        {
                            sheet = spreadsheet.Sheets[i];
                            break;
                        }
                    }
                    if (sheet != null) //필요한 데이터 타입의 시트가 있다면 DB에 있는걸 쓴다.
                    {
                        string sheetName = sheet.Properties.Title;

                        string range = $"{sheetName}!A1:Z"; // 필요한 범위 지정 전부 다 읽겠다.
                        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                        ValueRange response = request.Execute();

                        string jsonString = ParseSheetData(response.Values);

                        if (_resourcesServices.TryGetLoad($"Data/{sheetName}", out TextAsset originJsonFile))//있다면.
                        {
                            if (BinaryCheck(jsonString, originJsonFile.ToString()) == false)
                            {
                                SaveDataToFile(sheetName, jsonString);//있는데 두개가 다르다면 최신을 저장
                            }
                        }
                        else
                        {
                            SaveDataToFile(sheetName, jsonString);//없다면 저장
                        }
                        AddAllDataDictFromJsonData(sheetName, jsonString);
                    }
                    else
                    {
                        if (LoadAllDataFromLocal(requestType.Name) == false)
                        {
                            Debug.LogError($"Not Found RequestType  \"{requestType.Name}\"");
                        }
                    }
                }
            }
            catch (Exception error)//구글 스프레드시트에 연결이 안될때 에러처리
            {
                Debug.Log(error);
                Debug.Log("Load from LocalJson");
                foreach (Type requestType in requestDataTypes)
                {
                    if (LoadAllDataFromLocal(requestType.Name) == false)
                    {
                        Debug.LogError($"Not Found RequestType  \"{requestType.Name}\"");
                    }
                }
            }
        }

        private string GetTypeNameFromFileName(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                Debug.LogError("File name is null or empty.");
                return null;
            }

            string typeName = filepath.Replace("_", "").Replace("Data", "");
            if (typeName.Length == 0)
                return null;

            return typeName;
        }

        private Type FindGenericKeyType(Type typeinfo)
        {
            Type[] typeinterfaces = typeinfo.GetInterfaces();

            foreach (Type typeInterface in typeinterfaces)
            {
                if (typeInterface.IsGenericType && typeInterface.GetGenericTypeDefinition() == typeof(IKey<>))
                {
                    //제네릭타입의 첫번째 매개변수를 던진다. = 키가 되는 매개변수 
                    return typeInterface.GetGenericArguments()[0];
                }
            }
            return null;
        }

        private string ParseSheetData(IList<IList<object>> values)
        {
            // 데이터가 없으면 빈 JSON 리턴
            if (values == null || values.Count == 0) return "{}";

            var headers = values[0]; // 첫 번째 줄은 헤더(키값들)
            var list = new List<Dictionary<string, object>>();

            // 두 번째 줄부터 데이터
            for (int i = 1; i < values.Count; i++)
            {
                var row = values[i];
                var dict = new Dictionary<string, object>();

                for (int j = 0; j < headers.Count; j++)
                {
                    string key = headers[j].ToString();
            
                    //12.22수정: 데이터가 헤더보다 짧을 경우(빈 셀) 안전하게 처리
                    object val = (j < row.Count) ? row[j] : ""; 

                    dict.Add(key, val);
                }
                list.Add(dict);
            }

            // 최상단에 "stats" 키로 감싸기
            var wrapper = new { stats = list };

            return JsonConvert.SerializeObject(wrapper, Formatting.Indented);
        }

        private bool BinaryCheck(string src, string target)
        {
            if (string.Equals(src, target))
            {
                return true;
            }
    
            Debug.Log("Data has changed");
            return false;
        }

    }

    public interface IGoogleSheetData
    {
        //마킹용도의 인터페이스라 비워져 있음;
    }
}