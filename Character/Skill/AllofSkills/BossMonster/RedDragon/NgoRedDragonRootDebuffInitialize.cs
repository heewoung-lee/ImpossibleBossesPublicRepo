using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class NgoRedDragonRootDebuffInitialize : NgoPoolingInitializeBase
    {
        private const string RootSoundCueId = "RootSFX";

        public class RedDragonRootDebuffFactory : NgoZenjectFactory<NgoRedDragonRootDebuffInitialize>
        {
            [Inject]
            public RedDragonRootDebuffFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/RedDragon/RootDebuffVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RootDebuffVFX";
        public override int PoolingCapacity => 8;

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(RootSoundCueId);
            }
            _vfxManager.FollowParticleRoutine(targetGo.transform, gameObject);
        }
    }
}
