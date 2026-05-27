using GameManagers.RelayManagement;
using GameManagers.UIManagement;
using ScenesScripts.GamePlayScene;
using ScenesScripts.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace ScenesScripts.SecondBossScene
{
    public class MockSecondBossSceneSpawner : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        [Inject]
        public MockSecondBossSceneSpawner(IUIManagerServices uiManagerServices,RelayManager relayManager)
        {
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
        }
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;
        private UILoading _uiLoadingScene;

        public void Init()
        {
            _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
        }

        public void SpawnObj()
        {   
            UtilDebug.Log("ISceneSpawnBehaviour 스폰됨");
        }
    }
}
