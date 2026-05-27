using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace GameManagers.NGOPoolManagement.Implementation
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

    public class ThresholdNetworkObjectPoolExpansionStrategy : INetworkObjectPoolExpansionStrategy
    {
        private const int ThresholdNumerator = 2;
        private const int ThresholdDenominator = 3;

        private readonly NgoPoolManager _poolManager;

        public ThresholdNetworkObjectPoolExpansionStrategy(NgoPoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        public void TryExpand(string prefabPath)
        {
            if (_poolManager.TryGetPool(prefabPath, out ObjectPool<NetworkObject> poolObject) == false)
            {
                return;
            }

            int totalCount = poolObject.CountAll;
            if (totalCount <= 0)
            {
                return;
            }

            int nextActiveCount = poolObject.CountActive + 1;
            bool isThresholdExceeded = nextActiveCount * ThresholdDenominator > totalCount * ThresholdNumerator;
            if (isThresholdExceeded == false)
            {
                return;
            }

            int expandCount = _poolManager.GetPoolingCapacity(prefabPath);
            _poolManager.ExpandPool(prefabPath, expandCount);
        }
    }
}
