using GameManagers.ResourcesExManagement;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Acher
{
    public class NgoArcherMultiShotInitialize : NgoPoolingInitializeBase
    {
        private const float ProjectileLifeTime = 5f;

        public class ArcherMultiShotFactory : NgoZenjectFactory<NgoArcherMultiShotInitialize>, IArcherFactoryMarker
        {
            [Inject]
            public ArcherMultiShotFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Archer/Skill/MultiShot_projectile");
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

            Caller = targetGo.GetComponent<PlayerStats>();
            Debug.Assert(Caller != null, "_caller is null check the Scripts");
        }

        public override void OnPoolRelease()
        {
            base.OnPoolRelease();
            Caller = null;
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/MultiShot_projectile";
        public override int PoolingCapacity => 100;
    }
}
