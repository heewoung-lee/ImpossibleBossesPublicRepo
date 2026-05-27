using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Attack.Mage
{
    public class NgoMageAttackHitInitialize: NgoPoolingInitializeBase
    {
        private const string MageAttackHitSoundCueId = "MageAttackHitSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class MageAttackHitFactory : NgoZenjectFactory<NgoMageAttackHitInitialize>,IMageFactoryMarker
        {
            [Inject]
            public MageAttackHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Mage/MageAttackHit");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            _soundPlayerBinder.PlayDetached(MageAttackHitSoundCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/MageAttackHit";
        public override int PoolingCapacity => 5;
    }
}
