using Unity.Netcode;
using UnityEngine;

namespace GameManagers.RelayManager
{
    
    public interface INetworkSpawn
    {
        public GameObject SpawnNetworkObj(NetworkManager networkManager,string ngoPath, Transform parent = null, Vector3 position = default,
            bool destroyOption = true);

        public GameObject SpawnNetworkObj(NetworkManager networkManager,GameObject ngoInstance, Transform parent = null, Vector3 position = default,
            bool destroyOption = true);

        public GameObject SpawnNetworkObjInjectionOwner(NetworkManager networkManager,ulong clientId, string ngoPath, Vector3 position = default,
            Transform parent = null, bool destroyOption = true);

        public GameObject SpawnNetworkObjInjectionOwner(NetworkManager networkManager,ulong clientId, GameObject ngoInstance, Vector3 position = default,
            Transform parent = null, bool destroyOption = true);

    }
    
}
    