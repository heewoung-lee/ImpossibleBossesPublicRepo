using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace GameManagers.Interface.NGOPoolManager
{
    public interface INetworkObjectGetter
    {
        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation);
        
        public NetworkObject GetNetworkObject(string prefabPath);
    }
}