using GameManagers;
using GameManagers.Interface.ResourcesManager;
using Scene.CommonInstaller;
using Scene.GamePlayScene;
using Scene.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Zenject;

namespace Scene.BattleScene
{
    public class BattleSceneStarter : ISceneStarter
    {
        private readonly ISceneSpawnBehaviour _sceneSpawnBehaviour;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourcesServices;

        [Inject]
        public BattleSceneStarter(ISceneSpawnBehaviour sceneSpawnBehaviour, IUIManagerServices uiManagerServices, IResourcesServices resourcesServices)
        {
            _sceneSpawnBehaviour = sceneSpawnBehaviour;
            _uiManagerServices = uiManagerServices;
            _resourcesServices = resourcesServices;
        }
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        public void SceneStart()
        {
            _sceneSpawnBehaviour.Init();
            _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            _gamePlaySceneLoadingProgress =
                _resourcesServices.GetOrAddComponent<GamePlaySceneLoadingProgress>(_uiLoadingScene.gameObject);

            if (_uiLoadingScene.gameObject.activeSelf == true)
            {
                _gamePlaySceneLoadingProgress.OnLoadingComplete += () => _sceneSpawnBehaviour.SpawnObj();
            }
            else
            {
                _sceneSpawnBehaviour.SpawnObj();
            }
            
            //TODO: 여기 하드코딩 했음. 테스트일떄는 UILoading바가 닫혀서 바로 스폰되고, 노멀 부트일때는 로딩이끝나면 스폰됨.
            //이후로 수정할땐 테스트모드일때와 노멀모드 일떄 나눠서 실행할것 분기없이
        }
    }
}