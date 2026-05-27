using GameManagers.ResourcesExManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Mage
{
    public class NgoMageAttackInitialize : NgoPoolingInitializeBase
    {
        private const float ProjectileLifeTime = 4f;

        public class MageAttackFactory : NgoZenjectFactory<NgoMageAttackInitialize>, IMageFactoryMarker
        {
            public MageAttackFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Mage/MageAttack");
            }
        }

        private IResourcesServices _resources;
        public PlayerStats Caller { get; private set; }

        [Inject]
        public void Construct(IResourcesServices resources)
        {
            _resources = resources;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Caller = null;
            _resources.DestroyObject(gameObject, ProjectileLifeTime);
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position
                                 + targetGo.transform.forward
                                 + (targetGo.transform.up * 0.5f);
            transform.rotation = targetGo.transform.rotation;

            Caller = targetGo.GetComponent<PlayerStats>();
            Debug.Assert(Caller != null, "_caller is null check the Scripts");
        }

        public override void OnPoolRelease()
        {
            base.OnPoolRelease();
            Caller = null;
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/MageAttack";
        public override int PoolingCapacity => 5;
    }
}
