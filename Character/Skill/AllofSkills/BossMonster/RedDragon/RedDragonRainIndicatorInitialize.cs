using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonRainIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonRainIndicatorFactory : NgoZenjectFactory<RedDragonRainIndicatorInitialize>
        {
            [Inject]
            public RedDragonRainIndicatorFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/NGODragonRainIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/NGODragonRainIndicator";
        public override int PoolingCapacity => 30;
    }
}
