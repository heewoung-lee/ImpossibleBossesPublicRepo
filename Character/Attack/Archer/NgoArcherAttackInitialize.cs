using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork.BaseNGO;
using NetWork.NGO;
using Stats;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Archer
{
    public class NgoArcherAttackInitialize : NgoPoolingInitializeBase
    {
        private const float ProjectileLifeTime = 5f;
        private const string ArcherAttackSoundCueId = "ArcherAttackSFX";

        public class ArcherAttackFactory : NgoZenjectFactory<NgoArcherAttackInitialize>, IArcherFactoryMarker
        {
            [Inject]
            public ArcherAttackFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Archer/ArcherAttack");
            }
        }

        private IResourcesServices _resources;
        private SoundPlayerBinder _soundPlayerBinder;
        public PlayerStats Caller { get; private set; }

        [Inject]
        public void Construct(IResourcesServices resources)
        {
            _resources = resources;
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
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
            _soundPlayerBinder.PlayDetached(ArcherAttackSoundCueId);
        }

        public override void OnPoolRelease()
        {
            base.OnPoolRelease();
            Caller = null;
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/ArcherAttack";
        public override int PoolingCapacity => 5;
    }
}
