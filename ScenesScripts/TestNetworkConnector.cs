using Cysharp.Threading.Tasks;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene.Installer;
using Test;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
namespace ScenesScripts
{
    public class TestNetworkConnector : MonoBehaviour, ITestNetworkConnect, IExcludeFromPlayerBuild
    {
    
        private ISceneConnectOnline _connectOnline;

        [Inject]
        public void Construct(ISceneConnectOnline connectOnline)
        {
            _connectOnline = connectOnline;
        }
        public ISceneConnectOnline ConnectOnline  => _connectOnline;

        private void Start()
        {
            StartInitAsync().Forget();
        }

        private async UniTaskVoid StartInitAsync()
        {
            await _connectOnline.SceneConnectOnlineStart();
        }
    
    
    }
}
#endif
