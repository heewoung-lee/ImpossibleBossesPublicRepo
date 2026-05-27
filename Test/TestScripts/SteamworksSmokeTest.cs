using Test;
using UnityEngine;
using Util;

#if UNITY_EDITOR
using Steamworks;

namespace Test.TestScripts
{
    public class SteamworksSmokeTest : MonoBehaviour, IExcludeFromPlayerBuild
    {
        private bool _isInitialized;

        private void Start()
        {
            TryInitializeSteamworks();
        }

        private void OnDestroy()
        {
            if (_isInitialized == false)
            {
                return;
            }

            SteamAPI.Shutdown();
            _isInitialized = false;
            UtilDebug.Log("[SteamworksSmokeTest] SteamAPI shutdown.");
        }

        private void TryInitializeSteamworks()
        {
            try
            {
                if (SteamAPI.Init() == false)
                {
                    UtilDebug.LogError("[SteamworksSmokeTest] SteamAPI.Init failed. Check Steam client login, app ownership, and steam_appid.txt.");
                    return;
                }

                _isInitialized = true;

                UtilDebug.Log($"[SteamworksSmokeTest] SteamAPI.Init succeeded. AppId: {SteamUtils.GetAppID()}");
                UtilDebug.Log($"[SteamworksSmokeTest] SteamId: {SteamUser.GetSteamID()}");
                UtilDebug.Log($"[SteamworksSmokeTest] PersonaName: {SteamFriends.GetPersonaName()}");
                UtilDebug.Log($"[SteamworksSmokeTest] LoggedOn: {SteamUser.BLoggedOn()}");
            }
            catch (System.Exception ex)
            {
                UtilDebug.LogError($"[SteamworksSmokeTest] Steamworks initialization exception: {ex}");
            }
        }
    }
}
#endif
