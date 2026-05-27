using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonLandingVfxInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonLandingVfxFactory : NgoZenjectFactory<RedDragonLandingVfxInitialize>
        {
            [Inject]
            public RedDragonLandingVfxFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonLandingVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonLandingVFX";
        public override int PoolingCapacity => 5;
    }
}
