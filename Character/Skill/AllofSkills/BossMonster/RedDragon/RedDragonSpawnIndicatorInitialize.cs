using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonSpawnIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonSpawnIndicatorFactory : NgoZenjectFactory<RedDragonSpawnIndicatorInitialize>
        {
            [Inject]
            public RedDragonSpawnIndicatorFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/NGODragonSpawnIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/NGODragonSpawnIndicator";
        public override int PoolingCapacity => 10;
    }
}
