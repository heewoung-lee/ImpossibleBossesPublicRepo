using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class NgoRockToThrowSkyInitialize : NgoPoolingInitializeBase
    {
        public class NgoRockToThrowSkyFactory : NgoZenjectFactory<NgoRockToThrowSkyInitialize>
        {
            [Inject]
            public NgoRockToThrowSkyFactory(
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
                    "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoRockToThrowSky");
            }
        }

        public override void StartParticleOption(float duration, NetWork.NetworkParams networkParams)
        {
            if (TryGetComponent(out NgoRockToThrowSkyBehaviour throwBehaviour) == false)
            {
                throw new MissingComponentException(
                    $"[{nameof(NgoRockToThrowSkyInitialize)}] {nameof(NgoRockToThrowSkyBehaviour)} is missing.");
            }

            throwBehaviour.ConfigureThrowOnHost(duration, networkParams.ArgPosVector3);
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoRockToThrowSky";
        public override int PoolingCapacity => 20;
    }
}
