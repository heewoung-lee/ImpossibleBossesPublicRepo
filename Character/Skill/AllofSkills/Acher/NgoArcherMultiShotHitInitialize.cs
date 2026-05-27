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
    public class NgoArcherMultiShotHitInitialize: NgoPoolingInitializeBase
    {
        private const string ArcherHitSoundCueId = "ArcherHitSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class ArcherMultiShotHitFactory : NgoZenjectFactory<NgoArcherMultiShotHitInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public ArcherMultiShotHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>(  "Prefabs/Player/VFX/Archer/Skill/MultiShot_hit");
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

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/MultiShot_hit";
        public override int PoolingCapacity => 100;
    }
}
