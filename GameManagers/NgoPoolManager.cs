using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using GameManagers.Interface;
using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace GameManagers
{
    public class NgoPoolManager : IResettable
    {
        private readonly RelayManager _relayManager;
        
        [Inject] 
        public NgoPoolManager(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }
        public Dictionary<string, ObjectPool<NetworkObject>> PooledObjects => _ngoPool.PooledObjects;
        public Dictionary<string, Transform> PoolNgoRootDict => _ngoPool.PoolNgoRootDict;
        
        private NetworkObjectPool _ngoPool;

        public Transform GetNgoPoolTransform()
        {
            return _ngoPool.transform;
        }
        public void Set_NGO_Pool(NetworkObjectPool ngo)
        {
            _ngoPool = ngo;
        }
        
        
        public void Create_NGO_Pooling_Object()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false || _ngoPool != null)
                return;

            if (_relayManager.NgoRPCCaller == null)
            {
                _relayManager.SpawnRpcCallerEvent += SpawnNgoPolling;
            }
            else
            {
                SpawnNgoPolling();
            }

            void SpawnNgoPolling()
            {
                _relayManager.NgoRPCCaller.SpawnPrefabNeedToInitializeRpc("Prefabs/NGO/NGO_Pooling");
            }
        }
        public void SetPool_NGO_ROOT_Dict(string poolNgoPath,Transform rootTr)
        {
            PoolNgoRootDict.Add(poolNgoPath, rootTr);
        }
        public GameObject Pop(string prefabPath,Transform parantTr = null)
        {
            return _ngoPool.GetNetworkObject(prefabPath, Vector3.zero, Quaternion.identity).gameObject;
        }
        public void Push(NetworkObject ngo)
        {
            if (ngo == null)
                return;

            if (_relayManager.NetworkManagerEx.IsHost)
            {
                ngo.Despawn();
            }
        }

        public void NGO_Pool_RegisterPrefab(string path,int capacity = 5)
        {
            _ngoPool.RegisterPrefabInternal(path, capacity);
        }

        public void Clear()
        {
            PoolNgoRootDict.Clear();
        }

    }
}