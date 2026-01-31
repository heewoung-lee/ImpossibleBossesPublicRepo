using GameManagers.Interface.NGOPoolManager;
using GameManagers.Pool;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers.NGOPool.Implementation
{
    public class NetworkObjectGetter : INetworkObjectGetter
    {
        private readonly NgoPoolManager _poolManager;

        public NetworkObjectGetter(NgoPoolManager poolManager)
        {
            _poolManager = poolManager;
        }
        public NetworkObject GetNetworkObject(string prefabPath)
        {
            NetworkObject networkObject = _poolManager.PooledObjects[prefabPath].Get();
            if (networkObject.TryGetComponent(out NgoPoolingInitializeBase poolingInitialize))
            {
                poolingInitialize.OnPoolGet();
            }
            
            return networkObject;
        }

        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation)
        {
            NetworkObject networkObject = GetNetworkObject(prefabPath);
            networkObject.transform.position = position;
            networkObject.transform.rotation = rotation;
            return networkObject;
        }

        
    }
}