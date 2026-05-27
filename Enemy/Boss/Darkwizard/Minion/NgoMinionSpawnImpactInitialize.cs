using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard.Minion.Bomber
{
    public class NgoMinionSpawnImpactInitialize : NgoPoolingInitializeBase
    {
        public class NgoMinionSpawnImpactFactory : NgoZenjectFactory<NgoMinionSpawnImpactInitialize>
        {
            public NgoMinionSpawnImpactFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/AttackPattern/DarkWizard/MinionSpawnImpact");
            }
        }

        public override string PoolingNgoPath =>
            "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/MinionSpawnImpact";
        public override int PoolingCapacity => 5;
    }
}