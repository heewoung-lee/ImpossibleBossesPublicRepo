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
    public class RoomPlayScene : BaseScene
    {
        private ISceneStarter _roomSceneStarter;

        [Inject]
        public void Construct(ISceneStarter roomSceneStarter)
        {
            _roomSceneStarter = roomSceneStarter;
        }
        public override Define.Scene CurrentScene => Define.Scene.RoomScene;

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