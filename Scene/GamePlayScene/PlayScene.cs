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


        public override Define.Scene CurrentScene => Define.Scene.GamePlayScene;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        protected override void StartInit()
        {
            base.StartInit();
            _gameplaySceneStarter.SceneStart();
        }

        
    }
}