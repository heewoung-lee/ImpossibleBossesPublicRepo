using GameManagers;
using GameManagers.RelayManager;
using Scene.GamePlayScene;
using Scene.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Zenject;

namespace Scene.BattleScene
{
    public class UnitBattleScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        [Inject]
        public UnitBattleScene(IUIManagerServices uiManagerServices,RelayManager relayManager)
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
                _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoBattleSceneSpawn",_relayManager.NgoRoot.transform);
            }
        }
    }
}
