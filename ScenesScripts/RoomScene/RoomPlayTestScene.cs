using Cysharp.Threading.Tasks;
using GameManagers.RelayManagement;
using GameManagers.UIManagement;

using ScenesScripts.CommonInstaller.Interfaces;
using UnityEngine;
using Util;
using Zenject;

#if UNITY_EDITOR

namespace ScenesScripts.RoomScene
{
    public class RoomPlayTestScene : BaseScene, ISceneMultiMode
    {
        private IUIManagerServices _uiManagerServices;
        private ISceneConnectOnline _sceneConnectOnline;
        private ISceneStarter _roomSceneStarter;
        private RelayManager _relayManager;

        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, ISceneConnectOnline sceneConnectOnline,
            ISceneStarter roomSceneStarter, RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _sceneConnectOnline = sceneConnectOnline;
            _roomSceneStarter = roomSceneStarter;
            _relayManager = relayManager;
        }

        [SerializeField] private MultiMode multiMode;
        public override Define.SceneName CurrentSceneName => Define.SceneName.RoomScene;


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
                UtilDebug.LogError($"[RoomPlayScene] 초기화 중 예외: {e}");
            }
        }
        public MultiMode GetMultiTestMode() => multiMode;
    }
}
#endif
