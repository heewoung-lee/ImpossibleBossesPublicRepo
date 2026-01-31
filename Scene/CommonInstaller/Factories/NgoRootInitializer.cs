using System.Resources;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using GameManagers.Scene;
using NetWork.NGO;
using UI.Scene.Interface;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Scene.CommonInstaller.Factories
{
    public class NgoRootInitializer : NetworkBehaviour,ISceneChangeBehaviour
    {
        private RelayManager _relayManager;
        private SceneManagerEx _sceneManager;
        private IResourcesServices _resourceManager;

        [Inject]
        public void Construct(RelayManager relayManager, SceneManagerEx sceneManager, IResourcesServices resourceManager)
        {
            _relayManager = relayManager;
            _sceneManager = sceneManager;
            _resourceManager = resourceManager;
        }

        public class NgoRootFactory : NgoZenjectFactory<NgoRootInitializer>
        {
            public NgoRootFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_ROOT");
            }
        }


        [Rpc(SendTo.Server)]
        public void RemoveNgoRpc()
        {
            NetworkObject[] networkObjs = gameObject.GetComponentsInChildren<NetworkObject>();
            foreach (NetworkObject networkObject in networkObjs)
            {
                if (networkObject.DestroyWithScene == true)
                {
                    _resourceManager.DestroyObject(networkObject.gameObject);
                }
            }
        }

        public void OnBeforeSceneUnload()
        {
            RemoveNgoRpc();
        }
    }
}