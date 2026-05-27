using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonProjectileHitVFXInitialize : NgoPoolingInitializeBase
    {
        private const string ProjectileHitCueId = "RedDragonProjectileHitSFX";

        public class RedDragonProjectileHitVFXFactory : NgoZenjectFactory<RedDragonProjectileHitVFXInitialize>
        {
            [Inject]
            public RedDragonProjectileHitVFXFactory(
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
                    "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonProjectileHitVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonProjectileHitVFX";
        public override int PoolingCapacity => 30;

        public override void StartParticleOption(float duration, NetworkParams networkParams)
        {
            base.StartParticleOption(duration, networkParams);
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(ProjectileHitCueId);
            }
        }
    }
}
