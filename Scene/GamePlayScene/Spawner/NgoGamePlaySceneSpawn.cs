using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Pool;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using NetWork.BaseNGO;
using NetWork.NGO;
using NetWork.NGO.Scene_NGO;
using NPC.Dummy;
using Stats.BaseStats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Scene.GamePlayScene.Spawner
{
    public interface ISceneSpawn
    {
        public void SpawnObject(IResourcesServices resources, RelayManager relayManager);
    }


    /// <summary>
    ///  1.28일 수정
    ///  스폰으로 테스트해볼일이 생겨, 바인드를 함.
    ///  기존에는 노말로 바인드가 되고,
    ///  테스트가 필요할때마다 Rebind해서 스폰요구를 수정
    /// </summary>
    public class GamePlaySceneNormalSpawn : ISceneSpawn
    {
        public void SpawnObject(IResourcesServices resources, RelayManager relayManager)
        {
            if (relayManager.NetworkManagerEx.IsHost == false)
                return;

            BaseStats dummy1 = resources.InstantiateByKey("Prefabs/NPC/DamageTestDummy").GetComponent<BaseStats>();
            relayManager.SpawnNetworkObj(dummy1.gameObject, relayManager.NgoRoot.transform,
                position: new Vector3(12f, 0, -4f));


            NgoBossRoomEntrance ngoBossRoomEntrance = resources
                .InstantiateByKey("Prefabs/NGO/Scene_NGO/NGOBossRoomEntrance").GetComponent<NgoBossRoomEntrance>();
            relayManager.SpawnNetworkObj(ngoBossRoomEntrance.gameObject, relayManager.NgoRoot.transform);

            NgoStageTimerController ngoStageTimerController = resources
                .InstantiateByKey("Prefabs/NGO/Scene_NGO/NgoStageTimerController")
                .GetComponent<NgoStageTimerController>();
            relayManager.SpawnNetworkObj(ngoStageTimerController.gameObject, relayManager.NgoRoot.transform);
        }
    }


    public class NgoGamePlaySceneSpawn : NetworkBehaviourBase
    {
        public class NgoGamePlaySceneSpawnFactory : NgoZenjectFactory<NgoGamePlaySceneSpawn>
        {
            public NgoGamePlaySceneSpawnFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoGamePlaySceneSpawn");
            }
        }

        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        private IResourcesServices _resourcesServices;
        private ISceneSpawn _sceneSpawn;

        [Inject]
        public void Construct(RelayManager relayManager, NgoPoolManager poolManager,
            IResourcesServices resourcesServices, ISceneSpawn sceneSpawn)
        {
            _relayManager = relayManager;
            _poolManager = poolManager;
            _resourcesServices = resourcesServices;
            _sceneSpawn = sceneSpawn;
        }

        protected override void AwakeInit()
        {
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _sceneSpawn.SpawnObject(_resourcesServices, _relayManager);
            _poolManager.Create_NGO_Pooling_Object(); //네트워크 오브젝트 풀링 생성
        }

        protected override void StartInit()
        {
        }
    }
}