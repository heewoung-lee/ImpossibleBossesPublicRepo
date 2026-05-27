using System;
using System.Collections;
using System.Text;
using Test;
using UnityEngine;
using UnityEngine.Networking;
using Util;

#if UNITY_EDITOR
using Steamworks;

namespace Test.TestScripts
{
    public class SteamFirebaseAuthTicketTest : MonoBehaviour, IExcludeFromPlayerBuild
    {
        private const string SteamAuthIdentity = "ImpossibleBossesFirebase";
        private const string GetSteamProfileUrl = "https://us-central1-impossiblebosses-43b63.cloudfunctions.net/getSteamProfile";

        private bool _isInitialized;
        private bool _isWaitingForTicket;
        private HAuthTicket _authTicket = HAuthTicket.Invalid;
        private Callback<GetTicketForWebApiResponse_t> _ticketCallback;

        private void Start()
        {
            TryStartSteamTicketTest();
        }

        private void Update()
        {
            if (_isInitialized == false)
            {
                return;
            }

            SteamAPI.RunCallbacks();
        }

        private void OnDestroy()
        {
            CleanupSteamworks();
        }

        private void TryStartSteamTicketTest()
        {
            try
            {
                if (SteamAPI.Init() == false)
                {
                    UtilDebug.LogError("[SteamFirebaseAuthTicketTest] SteamAPI.Init failed.");
                    return;
                }

                _isInitialized = true;
                _ticketCallback = Callback<GetTicketForWebApiResponse_t>.Create(OnGetTicketForWebApiResponse);

                _isWaitingForTicket = true;
                _authTicket = SteamUser.GetAuthTicketForWebApi(SteamAuthIdentity);

                if (_authTicket == HAuthTicket.Invalid)
                {
                    _isWaitingForTicket = false;
                    UtilDebug.LogError("[SteamFirebaseAuthTicketTest] GetAuthTicketForWebApi returned invalid ticket handle.");
                    return;
                }

                UtilDebug.Log("[SteamFirebaseAuthTicketTest] Steam Web API ticket requested.");
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"[SteamFirebaseAuthTicketTest] Steam ticket test exception: {ex}");
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
                UtilDebug.LogError($"[SteamFirebaseAuthTicketTest] Steam ticket callback failed. Result: {response.m_eResult}");
                CancelSteamAuthTicket();
                return;
            }

            string ticketHex = ConvertTicketToHex(response.m_rgubTicket, response.m_cubTicket);
            UtilDebug.Log($"[SteamFirebaseAuthTicketTest] Steam ticket received. Bytes: {response.m_cubTicket}");

            StartCoroutine(VerifyTicketWithFirebase(ticketHex));
        }

        private IEnumerator VerifyTicketWithFirebase(string ticketHex)
        {
            string json = $"{{\"Ticket\":\"{ticketHex}\"}}";
            byte[] body = Encoding.UTF8.GetBytes(json);

            UnityWebRequest request = new UnityWebRequest(GetSteamProfileUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                UtilDebug.LogError($"[SteamFirebaseAuthTicketTest] Firebase request failed. Result: {request.result}, Error: {request.error}, Body: {request.downloadHandler.text}");
                CancelSteamAuthTicket();
                request.Dispose();
                yield break;
            }

            UtilDebug.Log($"[SteamFirebaseAuthTicketTest] Firebase response: {request.downloadHandler.text}");
            CancelSteamAuthTicket();
            request.Dispose();
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

        private void CleanupSteamworks()
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
            UtilDebug.Log("[SteamFirebaseAuthTicketTest] SteamAPI shutdown.");
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
    }
}
#endif
