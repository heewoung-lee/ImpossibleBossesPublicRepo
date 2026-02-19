using System;
using System.Collections.Generic;
using GameManagers.Interface;
using GameManagers.Interface.PoolManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using NetWork.BaseNGO;
using NetWork.NGO;
using NetWork.NGO.InitializeNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.Pool
{
    public class NgoPoolManager : IResettable
    {
        private const string PoolRootPrefabPath = "Prefabs/NGO/NGO_Pooling_ROOT";
        private readonly RelayManager.RelayManager _relayManager;
        private readonly SignalBus _signalBus;
        private readonly IResourcesServices _resourceManager;

        [Inject]
        public NgoPoolManager(RelayManager.RelayManager relayManager, SignalBus signalBus,
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
        }


        public void Create_NGO_Pooling_Object()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false || _ngoPool != null)
                return;

            if (_relayManager.NgoRPCCaller == null ||
                _relayManager.NgoRPCCaller.GetComponent<NetworkObject>().IsSpawned == false)
            {
                Action<RpcCallerReadySignal> onSignal = null;

                onSignal = (signal) =>
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
            PoolNgoRootDict.Add(poolNgoPath, rootTr);
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
            if (_ngoPool.PooledObjects.ContainsKey(prefabPath)) return;
            //처음에 호스트가 만들고 난다음에 이름을 변경해서 게스트와 함께 여기 다시 올 수 있음 호스트가 두번실행되는걸 막기위해 걸어놓음

            SetPool_NGO_ROOT_Dict(prefabPath, rootInitialize.transform);
            GameObject poolobj = _resourceManager.Load<GameObject>(prefabPath);
            Debug.Assert(poolobj != null, $"poolobj is null Check the Path{prefabPath}");
            //갖고있는 초기화 갯수값을 확인하기위해 로드함. 
            if (poolobj != null && poolobj.TryGetComponent(out INgoPooldata poolData))
            {
                _ngoPool.RegisterPrefabInternal(prefabPath, poolData.PoolingCapacity);
            }
            else
            {
                _ngoPool.RegisterPrefabInternal(prefabPath);
                UtilDebug.LogWarning($"[NgoPoolManager] {prefabPath} does not have INgoPoolData. Using default capacity.");
            }
        }

        public NetworkObject GetPooledObject(string prefabPath)
        {
            if (_ngoPool.PooledObjects.ContainsKey(prefabPath) == false)
            {
                CreatePoolRoot(prefabPath);
            }
            return _ngoPool.PooledObjects[prefabPath].Get();
        }

        private void CreatePoolRoot(string prefabPath)
        {
            //빈박스 만들기
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


        public void Clear()
        {
            PoolNgoRootDict.Clear();
        }

    }
}