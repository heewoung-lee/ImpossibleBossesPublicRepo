using Controller.BossState;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Scene.BattleScene.Spawner
{
    public class NgoBattleSceneSpawn : NetworkBehaviourBase
    {
        public class NgoBattleSceneSpawnFactory : NgoZenjectFactory<NgoBattleSceneSpawn>
        {
            public NgoBattleSceneSpawnFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoBattleSceneSpawn");
            }
        }

        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        private IResourcesServices _resourcesServices;


        [Inject]
        public void Construct(RelayManager relayManager, NgoPoolManager poolManager,
            IResourcesServices resourcesServices)
        {
            _relayManager = relayManager;
            _poolManager = poolManager;
            _resourcesServices = resourcesServices;
        }


        GameObject _player;

        protected override void AwakeInit()
        {
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            HostSpawnObject();
            _poolManager.Create_NGO_Pooling_Object();
        }

        private void HostSpawnObject()
        {
            if (IsHost == false)
                return;

            _relayManager.SpawnToRPC_Caller();

            BossGolemController bossGolemController = _resourcesServices
                .InstantiateByKey("Prefabs/Enemy/Boss/Character/StoneGolem").GetComponent<BossGolemController>();
            _relayManager.SpawnNetworkObj(bossGolemController.gameObject, _relayManager.NgoRoot.transform,
                new Vector3(10f, 0f, 10f));
        }

        protected override void StartInit()
        {
        }
    }
}