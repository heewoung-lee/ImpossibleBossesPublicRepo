using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonLandingIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonLandingIndicatorFactory : NgoZenjectFactory<RedDragonLandingIndicatorInitialize>
        {
            [Inject]
            public RedDragonLandingIndicatorFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/NGOLandingIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/NGOLandingIndicator";
        public override int PoolingCapacity => 5;
    }
}
