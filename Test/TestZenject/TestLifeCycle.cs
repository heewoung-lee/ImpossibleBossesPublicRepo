using GameManagers.Interface.ResourcesManager;
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
            public TestLifeCycleFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
               _requestGO = loadService.Load<GameObject>("TestPrefab/TestLifeCycle");
            }
        }

    }
}