using System.Collections.Generic;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace GameManagers.Interface.NGOPoolManager.Implementation
{
    public class NetworkObjectGetter : INetworkObjectGetter
    {
        private readonly NgoPoolManager _poolManager;

        public NetworkObjectGetter(NgoPoolManager poolManager)
        {
            _poolManager = poolManager;
        }


        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation)
        {
            NetworkObject networkObject = _poolManager.PooledObjects[prefabPath].Get();
            if (networkObject.TryGetComponent(out NgoPoolingInitializeBase poolingInitialize))
            {
                poolingInitialize.OnPoolGet();
            }
          
            Transform noTransform = networkObject.transform;
            noTransform.position = position;
            noTransform.rotation = rotation;

            return networkObject;
        }
    }
}