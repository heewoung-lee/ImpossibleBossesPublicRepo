
using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace ScenesScripts.GamePlayScene
{
    public class PlayScene : BaseScene, ISkillInit, IHasSceneMover
    {
        private ISceneStarter _gameplaySceneStarter;
        private ISceneMover _sceneMover;


        [Inject]
        public void Construct(
            ISceneStarter gameplaySceneStarter,
            ISceneMover sceneMover)
        {
            _gameplaySceneStarter = gameplaySceneStarter;
            _sceneMover = sceneMover;
            
        }

        public ISceneMover SceneMover => _sceneMover;


        public override Define.SceneName CurrentSceneName => Define.SceneName.GamePlayScene;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        protected override void StartInit()
        {
            base.StartInit();
            _gameplaySceneStarter.SceneStart();
        }

        
    }
}