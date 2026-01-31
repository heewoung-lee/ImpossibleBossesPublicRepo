using GameManagers.Interface.ResourcesManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers.Interface.RelayManagerInterface.Implementation
{
    public class DefaultNetworkSpawner : INetworkSpawn
    {
        private readonly IResourcesServices _resourcesServices;

        public DefaultNetworkSpawner(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
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
                
                if (networkObj.IsSpawned == false)
                {
                    networkObj.SpawnWithOwnership(clientId, destroyOption);
                }
                if (parent != null)
                {
                    networkObj.TrySetParent(parent, false);
                }
            }

            return instanceObj;
        }
    }
}