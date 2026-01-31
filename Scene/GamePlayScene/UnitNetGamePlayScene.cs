using GameManagers;
using GameManagers.Interface.UIManager;
using GameManagers.Pool;
using GameManagers.RelayManager;
using NetWork.NGO.UI;
using Scene.BattleScene;
using Scene.GamePlayScene.Spawner;
using Scene.GamePlayScene.Spwaner;
using UI.Scene.SceneUI;
using Zenject;

namespace Scene.GamePlayScene
{
    public class UnitNetGamePlayScene : ISceneSpawnBehaviour
    {
        private readonly IUIManagerServices _uiManagerServices;
        private readonly ISceneMover _sceneMover;
        private readonly RelayManager _relayManager;
        private readonly NgoPoolManager _poolManager;
        
        [Inject]
        public UnitNetGamePlayScene(
            IUIManagerServices uiManagerServices,
            ISceneMover sceneMover,
            RelayManager relayManager,
            NgoPoolManager poolManager)
        {
            _uiManagerServices = uiManagerServices;
            _sceneMover = sceneMover;
            _relayManager = relayManager;
            _poolManager = poolManager;
        } 

        private UIStageTimer _uiStageTimer;
        private UILoading _uiLoadingScene;
        private GamePlaySceneLoadingProgress _gamePlaySceneLoadingProgress;


        public void Init()
        {
            _uiLoadingScene = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            _uiStageTimer = _uiManagerServices.GetOrCreateSceneUI<UIStageTimer>();
        }
    
        public void SpawnObj()
        {
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoGamePlaySceneSpawn",_relayManager.NgoRoot.transform);
            }
            _poolManager.Create_NGO_Pooling_Object();
        }
   
    }
}
