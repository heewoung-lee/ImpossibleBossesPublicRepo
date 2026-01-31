using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.GamePlayScene
{
    public class PlayScene : BaseScene, ISkillInit, IHasSceneMover
    {
        private ISceneConnectOnline _sceneConnectOnline;
        private ISceneStarter _gameplaySceneStarter;
        private ISceneMover _sceneMover;


        [Inject]
        public void Construct(
            ISceneConnectOnline sceneConnectOnline,
            ISceneStarter gameplaySceneStarter,
            ISceneMover sceneMover)
        {
            _sceneConnectOnline = sceneConnectOnline;
            _gameplaySceneStarter = gameplaySceneStarter;
            _sceneMover = sceneMover;
            
        }

        public ISceneMover SceneMover => _sceneMover;


        public override Define.Scene CurrentScene => Define.Scene.GamePlayScene;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        protected override void StartInit()
        {
            base.StartInit();
            StartInitAsync().Forget();
        }

        private async UniTaskVoid StartInitAsync()
        {
            await _sceneConnectOnline.SceneConnectOnlineStart();
            try
            {
                _gameplaySceneStarter.SceneStart();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomPlayScene] 초기화 중 예외: {e}");
            }
        }

        
    }
}