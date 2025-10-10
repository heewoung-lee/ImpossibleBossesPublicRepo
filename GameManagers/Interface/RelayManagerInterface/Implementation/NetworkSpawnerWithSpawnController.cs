using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using Scene.CommonInstaller;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.RelayManagerInterface.Implementation
{
    public class NetworkSpawnerWithSpawnController : INetworkSpawn, IRegistrar<ISpawnController>
    {
        private readonly IResourcesServices _resourcesServices;
        private ISpawnController _spawnController;

        [Inject]
        public NetworkSpawnerWithSpawnController(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        public void Register(ISpawnController sceneContext)
        {
            _spawnController = sceneContext;
        }

        public void Unregister(ISpawnController sceneContext)
        {
            if (_spawnController != null)
            {
                _spawnController = null;
            }
        }

        public GameObject SpawnNetworkObj(NetworkManager networkManager, string ngoPath, Transform parent = null,
            Vector3 position = default,
            bool destroyOption = true)
        {
            return SpawnNetworkObjInjectionOwner(networkManager, networkManager.LocalClientId, ngoPath, position,
                parent,
                destroyOption);
        }

        public GameObject SpawnNetworkObj(NetworkManager networkManager, GameObject ngoInstance, Transform parent = null,
            Vector3 position = default, bool destroyOption = true)
        {
            return SpawnNetworkObjInjectionOwner(networkManager, networkManager.LocalClientId, ngoInstance, position,
                parent,
                destroyOption);
        }

        public GameObject SpawnNetworkObjInjectionOwner(NetworkManager networkManager, ulong clientId, string ngoPath,
            Vector3 position = default,
            Transform parent = null, bool destroyOption = true)
        {
            GameObject loadObj = _resourcesServices.InstantiateByKey(ngoPath);
            return SpawnAndInjectionNgo(networkManager, loadObj, clientId, position, parent, destroyOption);
        }

        public GameObject SpawnNetworkObjInjectionOwner(NetworkManager networkManager, ulong clientId, GameObject ngo,
            Vector3 position = default,
            Transform parent = null, bool destroyOption = true)
        {
            return SpawnAndInjectionNgo(networkManager, ngo, clientId, position, parent, destroyOption);
        }

        private GameObject SpawnAndInjectionNgo(NetworkManager networkManager, GameObject instanceObj, ulong clientId,
            Vector3 position,
            Transform parent = null, bool destroyOption = true)
        {
            if (networkManager.IsListening == true && networkManager.IsHost)
            {
                instanceObj.transform.position = position;
                NetworkObject networkObj = _resourcesServices.GetOrAddComponent<NetworkObject>(instanceObj);

                if (_spawnController != null)
                {
                    _spawnController.SpawnControllerOption(networkObj, NgoDefaultSpawn);
                }
                else
                {
                    NgoDefaultSpawn();
                }
                
                void NgoDefaultSpawn()
                {
                    if (networkObj.IsSpawned == false)
                    {
                        networkObj.SpawnWithOwnership(clientId, destroyOption);
                    }
                    if (parent != null)
                    {
                        networkObj.transform.SetParent(parent, false);
                    }
                }
            }

            return instanceObj;
        }
    }
}