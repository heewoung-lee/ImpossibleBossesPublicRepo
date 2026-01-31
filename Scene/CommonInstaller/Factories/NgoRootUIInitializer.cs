using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Scene.CommonInstaller.Factories
{
    public class NgoRootUIInitializer : NetworkBehaviour
    {
        public class NgoRootUIInitializerFactory : NgoZenjectFactory<NgoRootUIInitializer>
        {
            public NgoRootUIInitializerFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_ROOT_UI");
            }
        }
    }
}