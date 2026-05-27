using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class NgoRedDragonAttackInitialize : NgoPoolingInitializeBase
    {
        public class RedDragonAttackFactory : NgoZenjectFactory<NgoRedDragonAttackInitialize>
        {
            [Inject]
            public RedDragonAttackFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonAttackVFX");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/AttackPattern/RedDragon/RedDragonAttackVFX";
        public override int PoolingCapacity => 5;
    }
}
