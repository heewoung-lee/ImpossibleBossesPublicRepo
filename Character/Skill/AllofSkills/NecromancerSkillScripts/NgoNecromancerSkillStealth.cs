using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule.Necromancer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.NecromancerSkillScripts
{
    public class NgoNecromancerSkillStealth: NgoPoolingInitializeBase
    {
        private const string StealthSoundCueId = "StealthSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NecromancerStealthFactory : NgoZenjectFactory<NgoNecromancerSkillStealth>,INecromancerFactoryMarker
        {
            [Inject]
            public NecromancerStealthFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/NecromancerSkillPrefab/Stealth");
            }
      
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position + (Vector3.up * 0.5f);
            
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
            _soundPlayerBinder.PlayDetached(StealthSoundCueId);
            
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/NecromancerSkillPrefab/Stealth";
        public override int PoolingCapacity => 5;
    }
}
