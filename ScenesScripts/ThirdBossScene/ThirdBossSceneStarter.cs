using GameManagers.GameManagerExManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using ScenesScripts.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Zenject;

namespace ScenesScripts.ThirdBossScene
{
    public class ThirdBossSceneStarter : ISceneStarter
    {
        private readonly ISceneSpawnBehaviour _sceneSpawnBehaviour;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourcesServices;

        [Inject]
        public ThirdBossSceneStarter(
            ISceneSpawnBehaviour sceneSpawnBehaviour,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourcesServices)
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
                _gamePlaySceneLoadingProgress.OnLoadingComplete += _sceneSpawnBehaviour.SpawnObj;
            }
            else
            {
                _sceneSpawnBehaviour.SpawnObj();
            }
        }
    }
}
