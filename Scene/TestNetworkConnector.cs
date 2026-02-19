using System;
using Cysharp.Threading.Tasks;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene.Installer;
using UnityEngine;
using Util;
using Zenject;

#if UNITY_EDITOR
namespace Scene
{
    public class TestNetworkConnector : MonoBehaviour, ITestNetworkConnect
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