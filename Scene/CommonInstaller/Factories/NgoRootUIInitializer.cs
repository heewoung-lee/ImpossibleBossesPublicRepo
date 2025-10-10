using GameManagers.Interface.ResourcesManager;
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
            public NgoRootUIInitializerFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_ROOT_UI");
            }
        }
    }
}