using GameManagers.RelayManagement;
using GameManagers.UIManagement;
using ScenesScripts.GamePlayScene;
using ScenesScripts.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Zenject;

namespace ScenesScripts.SecondBossScene
{
    public class UnitSecondBossScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        [Inject]
        public UnitSecondBossScene(IUIManagerServices uiManagerServices,RelayManager relayManager)
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
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                //_relayManager.Load_NGO_Prefab<NgoBattleSceneSpawn>();
                _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoSecondBossSceneSpawn",_relayManager.NgoRoot.transform);
            }
        }
    }
}
