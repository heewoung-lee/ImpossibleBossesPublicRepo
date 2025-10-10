using System.Threading.Tasks;
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

        public override Define.Scene CurrentScene => Define.Scene.BattleScene;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        protected override void StartInit()
        {
            base.StartInit();
            // _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            // _gamePlaySceneLoadingProgress = _uiLoadingScene.AddComponent<GamePlaySceneLoadingProgress>();
            // // if (isTest == true)
            // // {
            // //     _battleSceneController = new MoveSceneController(new MockUnitBattleScene(Define.PlayerClass.Fighter, _uiLoadingScene, isSoloTest));
            // //     gameObject.AddComponent<MockUnitUIGamePlaySceneModule>();
            // //     _battleSceneController.InitGamePlayScene();
            // // }
            // // else
            // // {
            // //     _battleSceneController = new MoveSceneController(new UnitBattleScene());
            // //     gameObject.AddComponent<MockUnitUIGamePlaySceneModule>();
            // //     _battleSceneController.InitGamePlayScene();
            // //     _gamePlaySceneLoadingProgress.OnLoadingComplete += _battleSceneController.SpawnObj;
            // // }
            // //
            //
            // _sceneSpawnBehaviour.Init();
            // _sceneSpawnBehaviour.SpawnObj();
            //TODO: 0617 인르톨러가 없으니깐 반드시 인스톨러 넣을 것
            
            base.StartInit();
            _ = StartInitAsync();
        }
        private async Task StartInitAsync()
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
        
        public override void Clear()
        {
        }
        protected override void AwakeInit()
        {
        }

    }
}
