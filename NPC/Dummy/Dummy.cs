using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NPC.Dummy
{
    public class Dummy : NetworkBehaviour
    {
        public class DummyFactory : NgoZenjectFactory<Dummy>
        {
            public DummyFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NPC/DamageTestDummy");
            }
        }

    }
}