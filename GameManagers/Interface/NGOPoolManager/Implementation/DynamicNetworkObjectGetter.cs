using NetWork.BaseNGO;
using NetWork.NGO.InitializeNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using GameManagers;
using GameManagers.Interface.ResourcesManager;

namespace GameManagers.Interface.NGOPoolManager.Implementation
{
    public class DynamicNetworkObjectGetter : INetworkObjectGetter
    {
        private const string NgoPoolRootPrefabPath = "Prefabs/NGO/NGO_Pooling_ROOT";
        
        private readonly NgoPoolManager _poolManager;
        private readonly GameManagers.RelayManager _relayManager;
        private readonly IResourcesServices _resourcesServices;
        

        public DynamicNetworkObjectGetter(NgoPoolManager poolManager, IResourcesServices resourcesServices, GameManagers.RelayManager relayManager)
        {
            _poolManager = poolManager;
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }


        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation)
        {
            if (_poolManager.PooledObjects.ContainsKey(prefabPath) == false)
            {
                NgoPoolRootInitialize ngoPoolRootInitialize =_resourcesServices.InstantiateByKey(NgoPoolRootPrefabPath).GetComponent<NgoPoolRootInitialize>();
               
                Assert.IsNotNull(ngoPoolRootInitialize,$"ngoPoolRootInitialize is null check the Path {NgoPoolRootPrefabPath}");
                
                if (ngoPoolRootInitialize != null)
                {
                    _relayManager.SpawnNetworkObj(ngoPoolRootInitialize.gameObject);
                    ngoPoolRootInitialize.SetRootObjectName(prefabPath);
                }
            }
            
            
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
