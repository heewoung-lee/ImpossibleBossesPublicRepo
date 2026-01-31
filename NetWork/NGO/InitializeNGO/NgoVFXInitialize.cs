using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.ResourcesEx;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.InitializeNGO
{
    public class NgoVFXInitialize : NetworkBehaviour
    {
        private IVFXManagerServices _vfxManager;

        
        [Inject] 
        public void Construct(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }
        
        public class VFXRootNgoFactory : NgoZenjectFactory<NgoVFXInitialize>
        {
            public VFXRootNgoFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/VFXRootNGO");
            }
        }
        private NetworkObject _vfxRootNgo;
    }
}
