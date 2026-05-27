using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class NgoBossSkill1AttackHitInitialize : NgoPoolingInitializeBase
    {
        private const string BossSkill1HitCueId = "BossSkill1HitSFX";

        private SoundPlayerBinder _soundPlayerBinder;

        public class NgoBossSkill1AttackHitFactory : NgoZenjectFactory<NgoBossSkill1AttackHitInitialize>
        {
            [Inject]
            public NgoBossSkill1AttackHitFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>(
                    "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1AttackHit");
            }
        }

        private void Awake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            _soundPlayerBinder.PlayDetached(BossSkill1HitCueId);
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1AttackHit";
        public override int PoolingCapacity => 30;
    }
}
