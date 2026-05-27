using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers.NGOPoolManagement.Implementation
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

    public class CurrentNetworkObjectPoolExpansionStrategy : INetworkObjectPoolExpansionStrategy
    {
        private readonly NgoPoolManager _poolManager;

        public CurrentNetworkObjectPoolExpansionStrategy(NgoPoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        public void TryExpand(string prefabPath)
        {
            if (_poolManager.TryGetPool(prefabPath, out var poolObject) == false)
            {
                return;
            }

            if (poolObject.CountInactive > 0)
            {
                return;
            }

            _poolManager.ExpandPool(prefabPath, 1);
        }
    }
}
