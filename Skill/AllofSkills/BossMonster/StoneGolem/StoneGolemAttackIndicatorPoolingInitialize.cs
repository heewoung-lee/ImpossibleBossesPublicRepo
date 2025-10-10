using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.BossMonster.StoneGolem
{
    public class StoneGolemAttackIndicatorPoolingInitialize : NgoPoolingInitializeBase
    {
        public class StoneGolemAttackIndicatorPoolingFactory : NgoZenjectFactory<StoneGolemAttackIndicatorPoolingInitialize>
        {
            [Inject]
            public StoneGolemAttackIndicatorPoolingFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/Boss_Attack_Indicator");
            }
            
        }
        
        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/Boss_Attack_Indicator";
        public override int PoolingCapacity => 5;

    }
}
