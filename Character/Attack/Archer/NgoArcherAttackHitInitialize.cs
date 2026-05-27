using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Archer
{
    public class NgoArcherAttackHitInitialize: NgoPoolingInitializeBase
    {
        private const string ArcherHitSoundCueId = "ArcherHitSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class ArcherAttackHitFactory : NgoZenjectFactory<NgoArcherAttackHitInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public ArcherAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Archer/ArcherAttackHit");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            _soundPlayerBinder.PlayDetached(ArcherHitSoundCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/ArcherAttackHit";
        public override int PoolingCapacity => 5;
    }
}
