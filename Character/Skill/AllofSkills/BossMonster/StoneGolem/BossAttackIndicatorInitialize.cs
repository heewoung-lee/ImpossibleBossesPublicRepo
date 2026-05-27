using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class BossAttackIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class BossAttackIndicatorFactory : NgoZenjectFactory<BossAttackIndicatorInitialize>
        {
            [Inject]
            public BossAttackIndicatorFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/Boss_Attack_Indicator");
            }
            
        }
        
        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/Boss_Attack_Indicator";
        public override int PoolingCapacity => 5;

    }
}
