using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.LootItem
{
    public class ItemRootInitialize : NetworkBehaviour
    {
        public class ItemRootFactory : NgoZenjectFactory<ItemRootInitialize>
        {
            public ItemRootFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/ItemRootNetwork");
            }
        }
    }
}