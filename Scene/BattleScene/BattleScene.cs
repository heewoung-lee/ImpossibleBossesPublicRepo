using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.UIManager;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene;
using UI.Scene.SceneUI;
using Unity.VisualScripting;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.BattleScene
{
    public class BattleScene : BaseScene, ISkillInit, IHasSceneMover
    {
        private ISceneStarter _sceneStarter;
        private ISceneMover _sceneMover;

        [Inject]
        public void Construct(
            ISceneStarter gameplaySceneStarter,
            ISceneMover sceneMover)
        {
            _sceneStarter = gameplaySceneStarter;
            _sceneMover = sceneMover;
        }

        public ISceneMover SceneMover => _sceneMover;

        public override Define.Scene CurrentScene => Define.Scene.BattleScene;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        protected override void StartInit()
        {
            base.StartInit();
            _sceneStarter.SceneStart();
        }

    }
}