using GameManagers.Interface.ResourcesManager;
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
            public DummyFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NPC/DamageTestDummy");
            }
        }
    }
}