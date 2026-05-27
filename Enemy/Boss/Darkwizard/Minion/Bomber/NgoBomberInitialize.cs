using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss.Darkwizard.Minion
{
    public class NgoBomberInitialize : NgoPoolingInitializeBase
    {
        public class NgoBomberFactory : NgoZenjectFactory<NgoBomberInitialize>
        {
            public NgoBomberFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Minion/Bomber");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Minion/Bomber";
        public override int PoolingCapacity => 5;
    }
}
