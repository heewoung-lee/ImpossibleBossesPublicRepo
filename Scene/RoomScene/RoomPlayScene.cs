using System.Threading.Tasks;
using GameManagers;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.RoomScene
{
    public class RoomPlayScene : BaseScene,ISceneTestMode,ISceneMultiMode
    {
        [Inject]private IUIManagerServices _uiManagerServices;
        [Inject]private ISceneConnectOnline _sceneConnectOnline;
        [Inject]private ISceneStarter _roomSceneStarter;
        [Inject]private RelayManager _relayManager;

        [SerializeField] private TestMode testMode;
        [SerializeField] private MultiMode multiMode;
        public override Define.Scene CurrentScene => Define.Scene.RoomScene;

        public override void Clear()
        {
        }

        protected override void AwakeInit()
        {
        }
        protected override void StartInit()
        {
            base.StartInit();
            _ = StartInitAsync();
        }
        
        private async Task StartInitAsync()
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
