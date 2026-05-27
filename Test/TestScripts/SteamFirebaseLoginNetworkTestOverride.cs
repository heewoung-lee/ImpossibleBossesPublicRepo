using GameManagers.LoginManagement;
using UnityEngine;
using Util;

namespace Test.TestScripts
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Test/Steam Firebase Login Network Test Override")]
    public class SteamFirebaseLoginNetworkTestOverride : MonoBehaviour, IExcludeFromPlayerBuild
    {
        private const string DefaultTimeoutTestUrl = "https://10.255.255.1/getSteamProfile";

        [SerializeField] private bool _enableOverride;
        [SerializeField] private bool _overrideGetSteamProfileUrl = true;
        [SerializeField] private bool _overrideSaveSteamProfileUrl;
        [SerializeField] private string _getSteamProfileUrl = DefaultTimeoutTestUrl;
        [SerializeField] private string _saveSteamProfileUrl = DefaultTimeoutTestUrl;

        private void OnEnable()
        {
            ApplyOverride();
        }

        private void Start()
        {
            ApplyOverride();
        }

        private void OnDisable()
        {
            ClearOverride();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyOverride();
            }
        }
#endif

        private void ApplyOverride()
        {
#if UNITY_EDITOR
            if (_enableOverride == false)
            {
                ClearOverride();
                return;
            }

            string getUrl = _overrideGetSteamProfileUrl ? _getSteamProfileUrl : string.Empty;
            string saveUrl = _overrideSaveSteamProfileUrl ? _saveSteamProfileUrl : string.Empty;

            SteamFirebaseLoginService.SetEditorRequestUrlOverride(getUrl, saveUrl);
            UtilDebug.Log("[SteamFirebaseLoginNetworkTestOverride] Firebase login URL override enabled.");
#endif
        }

        private void ClearOverride()
        {
#if UNITY_EDITOR
            SteamFirebaseLoginService.ClearEditorRequestUrlOverride();
#endif
        }
    }
}
