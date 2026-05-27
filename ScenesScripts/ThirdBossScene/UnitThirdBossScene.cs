using GameManagers.RelayManagement;
using GameManagers.UIManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using ScenesScripts.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Zenject;

namespace ScenesScripts.ThirdBossScene
{
    public class UnitThirdBossScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        [Inject]
        public UnitThirdBossScene(IUIManagerServices uiManagerServices,RelayManager relayManager)
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
            if (_relayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoThirdBossSceneSpawn", _relayManager.NgoRoot.transform);
        }
    }
}
