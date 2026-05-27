using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonAttackIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonAttackIndicatorFactory : NgoZenjectFactory<RedDragonAttackIndicatorInitialize>
        {
            [Inject]
            public RedDragonAttackIndicatorFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/RedDragonAttackIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/RedDragonAttackIndicator";
        public override int PoolingCapacity => 5;
    }
}
