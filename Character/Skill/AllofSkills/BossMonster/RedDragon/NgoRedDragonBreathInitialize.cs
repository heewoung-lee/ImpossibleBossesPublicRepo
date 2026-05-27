using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class NgoRedDragonBreathInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonBreathFactory : NgoZenjectFactory<NgoRedDragonBreathInitialize>
        {
            [Inject]
            public RedDragonBreathFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonBreathVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/RedDragon/DragonBreathVFX";
        public override int PoolingCapacity => 5;
    }
}
