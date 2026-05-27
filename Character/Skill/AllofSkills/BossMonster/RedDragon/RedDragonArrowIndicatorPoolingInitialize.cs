using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonArrowIndicatorPoolingInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonArrowIndicatorPoolingFactory : NgoZenjectFactory<RedDragonArrowIndicatorPoolingInitialize>
        {
            [Inject]
            public RedDragonArrowIndicatorPoolingFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/NGODragonArrowIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/NGODragonArrowIndicator";
        public override int PoolingCapacity => 5;
    }
}