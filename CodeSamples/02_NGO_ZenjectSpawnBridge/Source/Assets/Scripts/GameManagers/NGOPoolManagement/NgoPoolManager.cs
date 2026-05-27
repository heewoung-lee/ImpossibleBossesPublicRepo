using System;
using System.Collections.Generic;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.NGO;
using NetWork.NGO.InitializeNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.NGOPoolManagement
{
    public class NgoPoolManager : IResettable
    {
        private const string PoolRootPrefabPath = "Prefabs/NGO/NGO_Pooling_ROOT";
        private const int DefaultPoolingCapacity = 5;
        private readonly RelayManager _relayManager;
        private readonly SignalBus _signalBus;
        private readonly IResourcesServices _resourceManager;
        private readonly Dictionary<string, int> _poolingCapacityDict = new Dictionary<string, int>();
        private readonly Dictionary<string, Transform> _pendingPoolRootDict = new Dictionary<string, Transform>();

        [Inject]
        public NgoPoolManager(RelayManager relayManager, SignalBus signalBus,
            IResourcesServices resourceManager)
        {
            _relayManager = relayManager;
            _signalBus = signalBus;
            _resourceManager = resourceManager;
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
            FlushPendingPoolRegistrations();
        }

        public bool TryGetPool(string prefabPath, out ObjectPool<NetworkObject> poolObject)
        {
            poolObject = null;

            if (_ngoPool == null)
            {
                return false;
            }

            return _ngoPool.PooledObjects.TryGetValue(prefabPath, out poolObject);
        }

        public int GetPoolingCapacity(string prefabPath)
        {
            if (_poolingCapacityDict.TryGetValue(prefabPath, out int poolingCapacity))
            {
                return poolingCapacity;
            }

            return ResolvePoolingCapacity(prefabPath, out _);
        }

        /// <summary>
        /// 외부 시스템이 특정 NGO 풀을 확장하고 싶을 때 사용하는 매니저 진입점.
        /// 실제 생성과 prewarm은 NetworkObjectPool이 소유하므로, 이 메서드는 유효성만 확인한 뒤 위임한다.
        /// </summary>
        /// <param name="prefabPath">확장 대상 풀의 키. INgoPooldata.PoolingNgoPath 값을 전달한다.</param>
        /// <param name="expandCount">추가로 예열할 수량. 0 이하이면 무시한다.</param>
        public void ExpandPool(string prefabPath, int expandCount)
        {
            if (_ngoPool == null || expandCount <= 0)
            {
                return;
            }

            _ngoPool.ExpandPool(prefabPath, expandCount);
        }

        public void Create_NGO_Pooling_Object()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false || _ngoPool != null)
                return;

            if (_relayManager.NgoRPCCaller == null ||
                _relayManager.NgoRPCCaller.GetComponent<NetworkObject>().IsSpawned == false)
            {
                Action<RpcCallerReadySignal> onSignal = null;

                onSignal = signal =>
                {
                    _signalBus.Unsubscribe<RpcCallerReadySignal>(onSignal);
                    SpawnNgoPolling();
                };
                _signalBus.Subscribe<RpcCallerReadySignal>(onSignal);
            }
            else
            {
                SpawnNgoPolling();
            }
        }

        private void SpawnNgoPolling()
        {
            _relayManager.NgoRPCCaller.SpawnPrefabNeedToInitializeRpc("Prefabs/NGO/NGO_Pooling", Vector3.zero,
                Quaternion.identity);
        }

        public void SetPool_NGO_ROOT_Dict(string poolNgoPath, Transform rootTr)
        {
            if (string.IsNullOrEmpty(poolNgoPath) || rootTr == null || _ngoPool == null)
            {
                return;
            }

            PoolNgoRootDict[poolNgoPath] = rootTr;
        }

        public GameObject Pop(string prefabPath)
        {
            return _ngoPool.GetNetworkObject(prefabPath).gameObject;
        }

        public void Push(NetworkObject ngo)
        {
            if (ngo == null || ngo.IsSpawned == false)
                return;

            if (_relayManager.NetworkManagerEx.IsHost)
            {
                ngo.Despawn();
            }
        }

        public void NGO_Pool_RegisterPrefab(string prefabPath, NgoPoolRootInitialize rootInitialize)
        {
            if (string.IsNullOrEmpty(prefabPath) || rootInitialize == null)
            {
                return;
            }

            if (_ngoPool == null)
            {
                _pendingPoolRootDict[prefabPath] = rootInitialize.transform;
                return;
            }

            RegisterPoolPrefab(prefabPath, rootInitialize.transform);
        }

        public NetworkObject GetPooledObject(string prefabPath)
        {
            if (_ngoPool.PooledObjects.ContainsKey(prefabPath) == false)
            {
                EnsurePoolRegistered(prefabPath);
            }

            return _ngoPool.PooledObjects[prefabPath].Get();
        }

        public void EnsurePoolRegistered(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath) || _ngoPool == null)
            {
                return;
            }

            if (_ngoPool.PooledObjects.ContainsKey(prefabPath))
            {
                return;
            }

            if (_relayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            CreatePoolRoot(prefabPath);
        }

        public bool TryGetPoolRoot(string prefabPath, out Transform rootTransform)
        {
            rootTransform = null;

            if (_ngoPool == null || string.IsNullOrEmpty(prefabPath))
            {
                return false;
            }

            return PoolNgoRootDict.TryGetValue(prefabPath, out rootTransform);
        }

        private void CreatePoolRoot(string prefabPath)
        {
            var rootObj = _resourceManager.InstantiateByKey(PoolRootPrefabPath);
            var rootInitialize = rootObj.GetComponent<NgoPoolRootInitialize>();
            Assert.IsNotNull(rootInitialize, "Root 프리팹에 스크립트가 없습니다!");

            if (rootInitialize != null)
            {
                _relayManager.SpawnNetworkObj(rootInitialize.gameObject);
                rootInitialize.SetRootObjectName(prefabPath);
                NGO_Pool_RegisterPrefab(prefabPath, rootInitialize);
            }
        }

        private void RegisterPoolPrefab(string prefabPath, Transform rootTransform)
        {
            SetPool_NGO_ROOT_Dict(prefabPath, rootTransform);

            if (_ngoPool.PooledObjects.ContainsKey(prefabPath))
            {
                return;
            }

            int poolingCapacity = ResolvePoolingCapacity(prefabPath, out bool hasPoolData);
            _ngoPool.RegisterPrefabInternal(prefabPath, poolingCapacity);
            if (hasPoolData == false)
            {
                UtilDebug.LogWarning($"[NgoPoolManager] {prefabPath} does not have INgoPoolData. Using default capacity.");
            }
        }

        private void FlushPendingPoolRegistrations()
        {
            if (_ngoPool == null || _pendingPoolRootDict.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, Transform> pendingRoot in _pendingPoolRootDict)
            {
                RegisterPoolPrefab(pendingRoot.Key, pendingRoot.Value);
            }

            _pendingPoolRootDict.Clear();
        }

        public void Clear()
        {
            if (_ngoPool != null)
            {
                PoolNgoRootDict.Clear();
            }

            _poolingCapacityDict.Clear();
            _pendingPoolRootDict.Clear();
        }

        private int ResolvePoolingCapacity(string prefabPath, out bool hasPoolData)
        {
            GameObject poolObject = _resourceManager.Load<GameObject>(prefabPath);
            Debug.Assert(poolObject != null, $"poolobj is null Check the Path{prefabPath}");

            if (poolObject != null && poolObject.TryGetComponent(out INgoPooldata poolData))
            {
                int poolingCapacity = poolData.PoolingCapacity;
                _poolingCapacityDict[prefabPath] = poolingCapacity;
                hasPoolData = true;
                return poolingCapacity;
            }

            _poolingCapacityDict[prefabPath] = DefaultPoolingCapacity;
            hasPoolData = false;
            return DefaultPoolingCapacity;
        }
    }
}
