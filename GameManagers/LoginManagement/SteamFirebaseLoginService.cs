using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using Util;
using Zenject;

namespace GameManagers.LoginManagement
{
    public class SteamFirebaseLoginService : ILoginService, IPlayerLogininfo, IPlayerIngameLogininfo, ITickable, IDisposable
    {
        private const string SteamAuthIdentity = "ImpossibleBossesFirebase";
        private const string GetSteamProfileUrl = "https://us-central1-impossiblebosses-43b63.cloudfunctions.net/getSteamProfile";
        private const string SaveSteamProfileUrl = "https://us-central1-impossiblebosses-43b63.cloudfunctions.net/saveSteamProfile";
        private const int LoginRequestTimeoutSeconds = 20;

#if UNITY_EDITOR
        private static string _editorGetSteamProfileUrlOverride = string.Empty;
        private static string _editorSaveSteamProfileUrlOverride = string.Empty;
#endif

        private bool _isInitialized;
        private bool _isWaitingForTicket;
        private HAuthTicket _authTicket = HAuthTicket.Invalid;
        private Callback<GetTicketForWebApiResponse_t> _ticketCallback;
        private UniTaskCompletionSource<string> _ticketCompletionSource;
        private PlayerIngameLoginInfo _playerIngameLoginInfo;
        private string _steamId64 = string.Empty;
        private string _playerNickName = string.Empty;

        public string PlayerNickName => _playerNickName;

        public void Tick()
        {
            if (_isInitialized == false)
            {
                return;
            }

            SteamAPI.RunCallbacks();
        }

        public async UniTask<LoginResult> LoginAsync()
        {
            UtilDebug.Log("[SteamFirebaseLoginService] LoginAsync started.");
            string ticket = await RequestSteamTicketAsync();

            if (string.IsNullOrEmpty(ticket))
            {
                UtilDebug.LogWarning(
                    $"[SteamFirebaseLoginService] Login failed at Steam ticket stage. ErrorCode: {LoginErrorCode.SteamUnavailable}");
                return LoginResult.Fail(LoginErrorCode.SteamUnavailable);
            }

            SteamProfileResponse response;

            // Steam 티켓은 Firebase 서버 검증이 끝나기 전까지 유효해야 하므로, 요청 완료 후 정리합니다.
            try
            {
                response = await PostJsonAsync(
                    GetCurrentGetSteamProfileUrl(),
                    JsonUtility.ToJson(new SteamTicketRequest(ticket)));
            }
            finally
            {
                CancelSteamAuthTicket();
            }

            if (response == null)
            {
                UtilDebug.LogError(
                    $"[SteamFirebaseLoginService] Login failed at Firebase profile stage. Response is null. ErrorCode: {LoginErrorCode.ServerError}");
                return LoginResult.Fail(LoginErrorCode.ServerError);
            }

            if (response.success == false)
            {
                UtilDebug.LogWarning(
                    $"[SteamFirebaseLoginService] Login failed at Firebase profile stage. ErrorCode: {response.code}");
                return LoginResult.Fail(response.code);
            }

            _steamId64 = response.SteamID64;
            _playerNickName = response.NickName ?? string.Empty;
            UtilDebug.Log("[SteamFirebaseLoginService] LoginAsync succeeded.");

            return LoginResult.Succeed(_steamId64, _playerNickName);
        }

#if UNITY_EDITOR
        public void SetEditorTestProfile(string playerKey, string nickName)
        {
            _steamId64 = playerKey;
            _playerNickName = nickName;
        }

        public static void SetEditorRequestUrlOverride(string getSteamProfileUrl, string saveSteamProfileUrl)
        {
            _editorGetSteamProfileUrlOverride = getSteamProfileUrl ?? string.Empty;
            _editorSaveSteamProfileUrlOverride = saveSteamProfileUrl ?? string.Empty;
        }

        public static void ClearEditorRequestUrlOverride()
        {
            _editorGetSteamProfileUrlOverride = string.Empty;
            _editorSaveSteamProfileUrlOverride = string.Empty;
        }
#endif

        public async UniTask<LoginResult> SaveNickNameAsync(string nickName)
        {
            if (string.IsNullOrEmpty(nickName))
            {
                return LoginResult.Fail(LoginErrorCode.MissingNickname);
            }

            string ticket = await RequestSteamTicketAsync();

            if (string.IsNullOrEmpty(ticket))
            {
                return LoginResult.Fail(LoginErrorCode.SteamUnavailable);
            }

            SteamProfileResponse response;

            // 닉네임 저장도 같은 Steam 티켓 검증 흐름을 사용하므로, 서버 응답을 받은 뒤 티켓을 취소합니다.
            try
            {
                response = await PostJsonAsync(
                    GetCurrentSaveSteamProfileUrl(),
                    JsonUtility.ToJson(new SteamNicknameRequest(ticket, nickName)));
            }
            finally
            {
                CancelSteamAuthTicket();
            }

            if (response == null)
            {
                return LoginResult.Fail(LoginErrorCode.ServerError);
            }

            if (response.success == false)
            {
                return LoginResult.Fail(response.code);
            }

            _steamId64 = response.SteamID64;
            _playerNickName = response.NickName ?? string.Empty;

            return LoginResult.Succeed(_steamId64, _playerNickName);
        }

        public PlayerIngameLoginInfo GetPlayerIngameLoginInfo()
        {
            return _playerIngameLoginInfo;
        }

        public void SetPlayerIngameLoginInfo(PlayerIngameLoginInfo playerIngameLoginInfo)
        {
            _playerIngameLoginInfo = playerIngameLoginInfo;
        }

        public void Dispose()
        {
            CancelSteamAuthTicket();

            if (_ticketCallback != null)
            {
                _ticketCallback.Dispose();
                _ticketCallback = null;
            }

            if (_isInitialized == false)
            {
                return;
            }

            SteamAPI.Shutdown();
            _isInitialized = false;
        }

        private async UniTask<string> RequestSteamTicketAsync()
        {
            UtilDebug.Log("[SteamFirebaseLoginService] Steam ticket request started.");
            if (TryInitializeSteamworks() == false)
            {
                UtilDebug.LogWarning("[SteamFirebaseLoginService] Steam ticket request stopped because Steamworks initialization failed.");
                return string.Empty;
            }

            if (_isWaitingForTicket)
            {
                UtilDebug.LogWarning("[SteamFirebaseLoginService] Steam ticket request skipped because another ticket request is pending.");
                return string.Empty;
            }

            _ticketCompletionSource = new UniTaskCompletionSource<string>();
            _isWaitingForTicket = true;
            _authTicket = SteamUser.GetAuthTicketForWebApi(SteamAuthIdentity);
            UtilDebug.Log("[SteamFirebaseLoginService] Steam Web API ticket requested.");

            if (_authTicket == HAuthTicket.Invalid)
            {
                _isWaitingForTicket = false;
                _ticketCompletionSource.TrySetResult(string.Empty);
                UtilDebug.LogError("[SteamFirebaseLoginService] GetAuthTicketForWebApi returned invalid ticket handle.");
            }

            (bool isTimeout, string ticket) = await _ticketCompletionSource.Task
                .TimeoutWithoutException(TimeSpan.FromSeconds(LoginRequestTimeoutSeconds));

            if (isTimeout)
            {
                _isWaitingForTicket = false;
                CancelSteamAuthTicket();
                _ticketCompletionSource.TrySetResult(string.Empty);
                UtilDebug.LogWarning("[SteamFirebaseLoginService] Steam ticket request timed out.");
                return string.Empty;
            }

            UtilDebug.Log("[SteamFirebaseLoginService] Steam ticket request completed.");
            return ticket;
        }

        private string GetCurrentGetSteamProfileUrl()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_editorGetSteamProfileUrlOverride) == false)
            {
                return _editorGetSteamProfileUrlOverride;
            }
#endif
            return GetSteamProfileUrl;
        }

        private string GetCurrentSaveSteamProfileUrl()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_editorSaveSteamProfileUrlOverride) == false)
            {
                return _editorSaveSteamProfileUrlOverride;
            }
#endif
            return SaveSteamProfileUrl;
        }

        private bool TryInitializeSteamworks()
        {
            if (_isInitialized)
            {
                return true;
            }

            try
            {
                if (SteamAPI.Init() == false)
                {
                    UtilDebug.LogWarning("[SteamFirebaseLoginService] SteamAPI.Init failed. Steam client is not running or not logged in.");
                    return false;
                }

                _isInitialized = true;
                _ticketCallback = Callback<GetTicketForWebApiResponse_t>.Create(OnGetTicketForWebApiResponse);
                return true;
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"[SteamFirebaseLoginService] SteamAPI.Init exception: {ex}");
                return false;
            }
        }

        private void OnGetTicketForWebApiResponse(GetTicketForWebApiResponse_t response)
        {
            if (_isWaitingForTicket == false)
            {
                return;
            }

            if (response.m_hAuthTicket != _authTicket)
            {
                return;
            }

            _isWaitingForTicket = false;

            if (response.m_eResult != EResult.k_EResultOK)
            {
                UtilDebug.LogError($"[SteamFirebaseLoginService] Steam ticket callback failed. Result: {response.m_eResult}");
                CancelSteamAuthTicket();
                _ticketCompletionSource.TrySetResult(string.Empty);
                return;
            }

            string ticket = ConvertTicketToHex(response.m_rgubTicket, response.m_cubTicket);
            UtilDebug.Log($"[SteamFirebaseLoginService] Steam ticket callback succeeded. Bytes: {response.m_cubTicket}");
            // 여기서 티켓을 취소하면 Firebase 검증 전에 무효화될 수 있으므로 호출자에서 검증 후 정리합니다.
            _ticketCompletionSource.TrySetResult(ticket);
        }

        private async UniTask<SteamProfileResponse> PostJsonAsync(string url, string json)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.timeout = LoginRequestTimeoutSeconds;
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                UtilDebug.Log($"[SteamFirebaseLoginService] Firebase profile request started. Url: {url}");
                await request.SendWebRequest().ToUniTask();
            }
            catch (UnityWebRequestException ex)
            {
                if (request.downloadHandler == null || string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    UtilDebug.LogError(
                        $"[SteamFirebaseLoginService] Firebase request failed without response body. Result: {request.result}, ResponseCode: {request.responseCode}, Error: {request.error}, Exception: {ex}");
                    request.Dispose();
                    return SteamProfileResponse.Fail(LoginErrorCode.NetworkError);
                }
            }

            string responseText = request.downloadHandler.text;

            if (string.IsNullOrEmpty(responseText))
            {
                UtilDebug.LogError(
                    $"[SteamFirebaseLoginService] Firebase response is empty. Result: {request.result}, ResponseCode: {request.responseCode}, Error: {request.error}");
                request.Dispose();
                return SteamProfileResponse.Fail(LoginErrorCode.NetworkError);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                UtilDebug.LogWarning(
                    $"[SteamFirebaseLoginService] Firebase request returned non-success transport state. Result: {request.result}, ResponseCode: {request.responseCode}, Error: {request.error}, Body: {responseText}");
            }

            try
            {
                SteamProfileResponse response = JsonUtility.FromJson<SteamProfileResponse>(responseText);
                if (response != null && response.success == false)
                {
                    UtilDebug.LogWarning(
                        $"[SteamFirebaseLoginService] Firebase profile response failed. Result: {request.result}, ResponseCode: {request.responseCode}, ErrorCode: {response.code}");
                }

                request.Dispose();
                return response;
            }
            catch (Exception ex)
            {
                UtilDebug.LogError(
                    $"[SteamFirebaseLoginService] Firebase response parse failed. Result: {request.result}, ResponseCode: {request.responseCode}, Error: {ex}, Body: {responseText}");
                request.Dispose();
                return SteamProfileResponse.Fail(LoginErrorCode.ServerError);
            }
        }

        private string ConvertTicketToHex(byte[] ticketBytes, int ticketLength)
        {
            StringBuilder builder = new StringBuilder(ticketLength * 2);

            for (int i = 0; i < ticketLength; i++)
            {
                builder.Append(ticketBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private void CancelSteamAuthTicket()
        {
            if (_authTicket == HAuthTicket.Invalid)
            {
                return;
            }

            SteamUser.CancelAuthTicket(_authTicket);
            _authTicket = HAuthTicket.Invalid;
        }

        [Serializable]
        private class SteamTicketRequest
        {
            public string Ticket;

            public SteamTicketRequest(string ticket)
            {
                Ticket = ticket;
            }
        }

        [Serializable]
        private class SteamNicknameRequest
        {
            public string Ticket;
            public string NickName;

            public SteamNicknameRequest(string ticket, string nickName)
            {
                Ticket = ticket;
                NickName = nickName;
            }
        }

        [Serializable]
        private class SteamProfileResponse
        {
            public bool success;
            public string code;
            public string SteamID64;
            public string NickName;

            public static SteamProfileResponse Fail(string errorCode)
            {
                return new SteamProfileResponse
                {
                    success = false,
                    code = errorCode,
                };
            }
        }
    }
}
