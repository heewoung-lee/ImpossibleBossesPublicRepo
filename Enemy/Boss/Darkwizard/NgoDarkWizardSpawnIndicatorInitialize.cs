using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard
{
    public class NgoDarkWizardSpawnIndicatorInitialize : NgoPoolingInitializeBase
    {
        public class NgoDarkWizardSpawnIndicatorFactory : NgoZenjectFactory<NgoDarkWizardSpawnIndicatorInitialize>
        {
            public NgoDarkWizardSpawnIndicatorFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Indicator/NGOSpawnIndicator");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Boss/Indicator/NGOSpawnIndicator";
        public override int PoolingCapacity => 5;
    }
}
