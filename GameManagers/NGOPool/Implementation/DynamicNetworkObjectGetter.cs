using GameManagers.Interface.NGOPoolManager;
using GameManagers.Pool;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers.NGOPool.Implementation
{
    public class DynamicNetworkObjectGetter : INetworkObjectGetter
    {
        private readonly NgoPoolManager _poolManager;

        public DynamicNetworkObjectGetter(NgoPoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation)
        {
            NetworkObject networkObject = GetNetworkObject(prefabPath);
            networkObject.transform.position = position;
            networkObject.transform.rotation = rotation;
            return networkObject;
        }
        
        public NetworkObject GetNetworkObject(string prefabPath)
        {
            NetworkObject networkObject = _poolManager.GetPooledObject(prefabPath);

            if (networkObject.TryGetComponent(out NgoPoolingInitializeBase poolingInitialize))
            {
                poolingInitialize.OnPoolGet();
            }
            return networkObject;
        }
        
    }
}