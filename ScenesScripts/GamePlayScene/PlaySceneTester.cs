using Cysharp.Threading.Tasks;

using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using Util;
using Zenject;

#if UNITY_EDITOR 
namespace ScenesScripts.GamePlayScene
{
    public class PlaySceneTester : BaseScene, ISkillInit, IHasSceneMover
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


        public override Define.SceneName CurrentSceneName => Define.SceneName.GamePlayScene;
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
                UtilDebug.LogError($"[RoomPlayScene] 초기화 중 예외: {e}");
            }
        }

        
    }
}
#endif