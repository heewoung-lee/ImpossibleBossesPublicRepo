using GameManagers.GameManagerExManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene.Spawner;
using Stats;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace ScenesScripts.GamePlayScene
{
    public class PlaySceneStarter : ISceneStarter
    {
        private readonly ISceneSpawnBehaviour _sceneSpawnBehaviour;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourcesServices;
        private readonly IPlayerSpawnManager _playerSpawnManager;

        [Inject]
        public PlaySceneStarter(
            ISceneSpawnBehaviour sceneSpawnBehaviour,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourcesServices,
            IPlayerSpawnManager playerSpawnManager)
        {
            _sceneSpawnBehaviour = sceneSpawnBehaviour;
            _uiManagerServices = uiManagerServices;
            _resourcesServices = resourcesServices;
            _playerSpawnManager = playerSpawnManager;
        }

        
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;

        public void SceneStart()
        {
            NotifySceneOpeningStart();
            Debug.Log($"{_sceneSpawnBehaviour.GetType()}");
            
            _sceneSpawnBehaviour.Init();
            _sceneSpawnBehaviour.SpawnObj();
            _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            
            _gamePlaySceneLoadingProgress =_resourcesServices.GetOrAddComponent<GamePlaySceneLoadingProgress>(_uiLoadingScene.gameObject);

            if (_uiLoadingScene.gameObject.activeSelf == true)
            {
                _gamePlaySceneLoadingProgress.OnLoadingComplete += NotifySceneOpeningEnd;
                return;
            }

            NotifySceneOpeningEnd();
        }

        private void NotifySceneOpeningStart()
        {
            NotifyPlayerSceneOpening(target => target.OnSceneOpeningStart());
        }

        private void NotifySceneOpeningEnd()
        {
            NotifyPlayerSceneOpening(target => target.OnSceneOpeningEnd());
        }

        private void NotifyPlayerSceneOpening(System.Action<IPlayerSceneOpeningTarget> notifyAction)
        {
            GameObject player = _playerSpawnManager.GetPlayer();
            IPlayerSceneOpeningTarget target = player != null ? player.GetComponent<IPlayerSceneOpeningTarget>() : null;
            if (target != null)
            {
                notifyAction(target);
                return;
            }

            void OnPlayerSpawned(PlayerStats playerStats)
            {
                _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
                IPlayerSceneOpeningTarget spawnedTarget =
                    playerStats != null ? playerStats.GetComponent<IPlayerSceneOpeningTarget>() : null;
                if (spawnedTarget != null)
                {
                    notifyAction(spawnedTarget);
                }
            }

            _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
            _playerSpawnManager.OnPlayerSpawnEvent += OnPlayerSpawned;
        }
    }
}
