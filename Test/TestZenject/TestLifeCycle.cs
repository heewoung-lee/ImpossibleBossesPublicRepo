using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Test.TestZenject
{
    public class TestLifeCycle : NetworkBehaviour
    {
        public class TestLifeCycleFactory : NgoZenjectFactory<TestLifeCycle>
        {
            public TestLifeCycleFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
               _requestGO = loadService.Load<GameObject>("TestPrefab/TestLifeCycle");
            }
        }

    }
}