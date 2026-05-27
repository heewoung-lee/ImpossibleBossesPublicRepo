using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class StoneGolemRollingRockInitialize : NgoPoolingInitializeBase
    {
        public class StoneGolemRollingRockFactory : NgoZenjectFactory<StoneGolemRollingRockInitialize>
        {
            [Inject]
            public StoneGolemRollingRockFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoMinionRock");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoMinionRock";
        public override int PoolingCapacity => 5;
    }
}
