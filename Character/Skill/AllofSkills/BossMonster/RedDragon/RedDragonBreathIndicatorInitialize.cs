using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonBreathIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonBreathIndicatorFactory : NgoZenjectFactory<RedDragonBreathIndicatorInitialize>
        {
            [Inject]
            public RedDragonBreathIndicatorFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/NGODragonBreathIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/NGODragonBreathIndicator";
        public override int PoolingCapacity => 5;
    }
}
