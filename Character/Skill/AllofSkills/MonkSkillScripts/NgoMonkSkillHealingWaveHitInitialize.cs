using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Monk;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.MonkSkillScripts
{
    public class NgoMonkSkillHealingWaveHitInitialize : NgoPoolingInitializeBase
    {
        private const string HealingWaveSoundCueId = "HealingWaveSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoMonkSkillHealingWaveHitFactory : NgoZenjectFactory<NgoMonkSkillHealingWaveHitInitialize>,
            IMonkFactoryMarker
        {
            [Inject]
            public NgoMonkSkillHealingWaveHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/MonkSkillPrefab/HealingWaveHit");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _soundPlayerBinder.PlayDetached(HealingWaveSoundCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/MonkSkillPrefab/HealingWaveHit";
        public override int PoolingCapacity => 5;
        
    }
}
