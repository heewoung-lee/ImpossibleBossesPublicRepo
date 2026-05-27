using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Acher
{
    public class NgoArcherSkillArrowRainInitialize : NgoPoolingInitializeBase
    {
        private const string ArrowRainSoundCueId = "ArrowRainSFX";
        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoArcherSkillArrowRainFactory : NgoZenjectFactory<NgoArcherSkillArrowRainInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public NgoArcherSkillArrowRainFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Archer/Skill/ArrowRain");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }
    
        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            _soundPlayerBinder.PlayDetached(ArrowRainSoundCueId);
        }
    
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/ArrowRain";
        public override int PoolingCapacity => 5;

    }
        
}
