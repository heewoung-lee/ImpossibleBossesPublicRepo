using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class NgoBossSkill1AttackInitialize : NgoPoolingInitializeBase
    {
        public class NgoBossSkill1AttackFactory : NgoZenjectFactory<NgoBossSkill1AttackInitialize>
        {
            [Inject]
            public NgoBossSkill1AttackFactory(
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
                    "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1Attack");
            }
        }

        public override void StartParticleOption(float duration, NetWork.NetworkParams networkParams)
        {
            if (TryGetComponent(out NgoBossSkill1AttackBehaviour projectileBehaviour) == false)
            {
                throw new MissingComponentException(
                    $"[{nameof(NgoBossSkill1AttackInitialize)}] {nameof(NgoBossSkill1AttackBehaviour)} is missing.");
            }

            projectileBehaviour.ConfigureLaunchOnHost(
                duration,
                networkParams.ArgPosVector3,
                networkParams.ArgUlong,
                networkParams.ArgInt,
                networkParams.ArgBoolean);
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1Attack";
        public override int PoolingCapacity => 30;
    }
}
