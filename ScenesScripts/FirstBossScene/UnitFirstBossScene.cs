using GameManagers.RelayManagement;
using GameManagers.UIManagement;
using ScenesScripts.GamePlayScene;
using ScenesScripts.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace ScenesScripts.FirstBossScene
{
    public class UnitFirstBossScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        [Inject]
        public UnitFirstBossScene(IUIManagerServices uiManagerServices,RelayManager relayManager)
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
