using Character.Skill.AllofSkills.Mage;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using Module.PlayerModule.PlayerClassModule;
using Module.PlayerModule.PlayerClassModule.Mage;
using Module.PlayerModule.PlayerClassModule.Necromancer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.NecromancerSkillScripts
{
    public class NgoNecromancerSkillDeathFingerHitInitialize : NgoPoolingInitializeBase
    {
        private const string DeathFingerHitSoundCueId = "DeathfinkgerSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoDeathFingerHitFactory : NgoZenjectFactory<NgoNecromancerSkillDeathFingerHitInitialize>, INecromancerFactoryMarker
        {
            [Inject]
            public NgoDeathFingerHitFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/NecromancerSkillPrefab/DeathFingerHit");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
            _soundPlayerBinder.PlayDetached(DeathFingerHitSoundCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/NecromancerSkillPrefab/DeathFingerHit";
        public override int PoolingCapacity => 5;
    }
}
