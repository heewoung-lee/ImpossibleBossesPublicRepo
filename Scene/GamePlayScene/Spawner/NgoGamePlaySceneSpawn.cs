using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using NetWork.NGO.Scene_NGO;
using NPC.Dummy;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Scene.GamePlayScene.Spawner
{
    public class NgoGamePlaySceneSpawn : NetworkBehaviourBase
    {
        public class NgoGamePlaySceneSpawnFactory : NgoZenjectFactory<NgoGamePlaySceneSpawn>
        {
            public NgoGamePlaySceneSpawnFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoGamePlaySceneSpawn");
            }
        }

        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        private IResourcesServices _resourcesServices;

        [Inject]
        public void Construct(RelayManager relayManager, NgoPoolManager poolManager,IResourcesServices resourcesServices)
        {
            _relayManager = relayManager;
            _poolManager = poolManager;
            _resourcesServices = resourcesServices;
        }
        protected override void AwakeInit()
        {
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SpawnObject();
            _poolManager.Create_NGO_Pooling_Object(); //네트워크 오브젝트 풀링 생성
        }
        protected override void StartInit()
        {
        }

        public void SpawnObject()
        {
            if (IsHost == false)
                return;
            
            _relayManager.SpawnToRPC_Caller();

            Dummy dummy = _resourcesServices.InstantiateByKey("Prefabs/NPC/DamageTestDummy").GetComponent<Dummy>();
            _relayManager.SpawnNetworkObj(dummy.gameObject, _relayManager.NgoRoot.transform,position: new Vector3(10f, 0, -2.5f));

            NgoBossRoomEntrance ngoBossRoomEntrance = _resourcesServices.InstantiateByKey("Prefabs/NGO/Scene_NGO/NGOBossRoomEntrance").GetComponent<NgoBossRoomEntrance>();
            _relayManager.SpawnNetworkObj(ngoBossRoomEntrance.gameObject, _relayManager.NgoRoot.transform);

            NgoStageTimerController ngoStageTimerController =_resourcesServices.InstantiateByKey("Prefabs/NGO/Scene_NGO/NgoStageTimerController").GetComponent<NgoStageTimerController>();
            _relayManager.SpawnNetworkObj(ngoStageTimerController.gameObject, _relayManager.NgoRoot.transform);
        }
    }
}