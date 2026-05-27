
using ScenesScripts.CommonInstaller.Interfaces;
using Util;
using Zenject;

namespace ScenesScripts.RoomScene
{
    public class RoomPlayScene : BaseScene
    {
        private ISceneStarter _roomSceneStarter;

        [Inject]
        public void Construct(ISceneStarter roomSceneStarter)
        {
            _roomSceneStarter = roomSceneStarter;
        }
        public override Define.SceneName CurrentSceneName => Define.SceneName.RoomScene;

        protected override void AwakeInit()
        {
        }
        protected override void StartInit()
        {
            base.StartInit();
            _roomSceneStarter.SceneStart();
        }
    }
}