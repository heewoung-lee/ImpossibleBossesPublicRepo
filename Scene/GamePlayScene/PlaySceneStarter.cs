using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using GameManagers.ResourcesEx;
using Module.UI_Module;
using Scene.CommonInstaller;
using Scene.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace Scene.GamePlayScene
{
    public class PlaySceneStarter : ISceneStarter
    {
        private readonly ISceneSpawnBehaviour _sceneSpawnBehaviour;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourcesServices;

        [Inject]
        public PlaySceneStarter(ISceneSpawnBehaviour sceneSpawnBehaviour, IUIManagerServices uiManagerServices, IResourcesServices resourcesServices)
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
            _sceneSpawnBehaviour.SpawnObj();
            _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            _gamePlaySceneLoadingProgress =_resourcesServices.GetOrAddComponent<GamePlaySceneLoadingProgress>(_uiLoadingScene.gameObject);
        }
    }
}
