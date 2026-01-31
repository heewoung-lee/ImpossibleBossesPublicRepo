using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.RelayManager;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.RoomScene
{
    public class RoomPlayScene : BaseScene,ISceneTestMode,ISceneMultiMode
    {
        private IUIManagerServices _uiManagerServices;
        private ISceneConnectOnline _sceneConnectOnline;
        private ISceneStarter _roomSceneStarter;
        private RelayManager _relayManager;
        
        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, ISceneConnectOnline sceneConnectOnline,ISceneStarter roomSceneStarter,RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _sceneConnectOnline = sceneConnectOnline;
            _roomSceneStarter = roomSceneStarter;
            _relayManager = relayManager;
        }
        


        [SerializeField] private TestMode testMode;
        [SerializeField] private MultiMode multiMode;
        public override Define.Scene CurrentScene => Define.Scene.RoomScene;


        protected override void AwakeInit()
        {
        }
        protected override void StartInit()
        {
            base.StartInit();
            StartInitAsync().Forget();
        }
        
        private async UniTaskVoid StartInitAsync()
        {
            try
            {
                await _sceneConnectOnline.SceneConnectOnlineStart();
                _roomSceneStarter.SceneStart();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomPlayScene] 초기화 중 예외: {e}");
            }
        }

        public TestMode GetTestMode()=>testMode;

        public MultiMode GetMultiTestMode() => multiMode;
    }
}
