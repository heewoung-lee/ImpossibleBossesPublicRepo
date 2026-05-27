using System.Collections.Generic;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO
{
    public class NetworkObjectPool : NetworkBehaviour
    {
        public class NetworkObjectPoolFactory : NgoZenjectFactory<NetworkObjectPool>
        {
            public NetworkObjectPoolFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_Pooling");
            }
        }

        private IResourcesServices _resourcesServices;
        private INetworkObjectGetter _networkObjectGetter;
        private INetworkObjectPoolExpansionStrategy _poolExpansionStrategy;
        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            INetworkObjectGetter networkObjectGetter,
            INetworkObjectPoolExpansionStrategy poolExpansionStrategy,
            RelayManager relayManager,
            NgoPoolManager poolManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _poolManager = poolManager;
            _networkObjectGetter = networkObjectGetter;
            _poolExpansionStrategy = poolExpansionStrategy;

            m_PooledObjects = new Dictionary<string, ObjectPool<NetworkObject>>();
            _poolNgoRootDict = new Dictionary<string, Transform>();
        }

        private Dictionary<string, ObjectPool<NetworkObject>> m_PooledObjects;
        public Dictionary<string, ObjectPool<NetworkObject>> PooledObjects => m_PooledObjects;
        private Dictionary<string, Transform> _poolNgoRootDict;
        public Dictionary<string, Transform> PoolNgoRootDict => _poolNgoRootDict;

        public override void OnNetworkDespawn()
        {
            foreach (string prefabPath in m_PooledObjects.Keys)
            {
                m_PooledObjects[prefabPath].Clear();
                GameObject prefab = _resourcesServices.Load<GameObject>(prefabPath);
                _relayManager.NetworkManagerEx.PrefabHandler.RemoveHandler(prefab);
            }

            m_PooledObjects.Clear();
            _poolManager.Set_NGO_Pool(null);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _poolManager.Set_NGO_Pool(this);

            if (_relayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            gameObject.RemoveCloneText();
            PreloadRegisteredFactoryPools();
        }

        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation)
        {
            _poolExpansionStrategy.TryExpand(prefabPath);
            return _networkObjectGetter.GetNetworkObject(prefabPath, position, rotation);
        }

        public NetworkObject GetNetworkObject(string prefabPath)
        {
            _poolExpansionStrategy.TryExpand(prefabPath);
            return _networkObjectGetter.GetNetworkObject(prefabPath);
        }

        public void ReturnNetworkObject(NetworkObject networkObject)
        {
            if (networkObject.TryGetComponent(out NgoPoolingInitializeBase ngoPoolingInitializeBase))
            {
                ngoPoolingInitializeBase.OnPoolRelease();
                if (m_PooledObjects.TryGetValue(ngoPoolingInitializeBase.PoolingNgoPath, out ObjectPool<NetworkObject> poolObj))
                {
                    poolObj.Release(networkObject);
                }
                else
                {
                    UtilDebug.Log($"{networkObject.name} can't return the Pool");
                }
            }
        }

        public void RegisterPrefabInternal(string prefabPath, int prewarmCount = 5)
        {
            GameObject prefab = _resourcesServices.Load<GameObject>(prefabPath);
            if (_relayManager.NetworkManagerEx.GetNetworkPrefabOverride(prefab) == null)
            {
                UtilDebug.Log($"{prefab.name} is not registed the NetworkManager");
                return;
            }

            m_PooledObjects[prefabPath] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy,
                defaultCapacity: prewarmCount);

            _relayManager.NetworkManagerEx.PrefabHandler.RemoveHandler(prefab);
            PooledPrefabInstanceHandler handler = new PooledPrefabInstanceHandler(this, prefabPath);
            _relayManager.NetworkManagerEx.PrefabHandler.AddHandler(prefab, handler);

            ExpandPool(prefabPath, prewarmCount);

            NetworkObject CreateFunc()
            {
                return _resourcesServices.InstantiatePrefab(prefab, _poolManager.PoolNgoRootDict[prefabPath])
                    .RemoveCloneText()
                    .GetComponent<NetworkObject>();
            }

            void ActionOnGet(NetworkObject networkObject)
            {
                networkObject.gameObject.SetActive(true);
            }

            void ActionOnRelease(NetworkObject networkObject)
            {
                networkObject.gameObject.SetActive(false);
            }

            void ActionOnDestroy(NetworkObject networkObject)
            {
                Destroy(networkObject.gameObject);
            }
        }

        //3.21일 추가 
        /// <summary>
        /// 이미 등록된 특정 NGO 풀에 비활성 인스턴스를 추가로 확보하는 prewarm 메서드.
        /// 이 메서드는 객체를 "실제 사용 상태로 꺼내는 것"이 아니라,
        /// Get/Release를 이용해 풀 내부에 inactive 상태 객체를 더 쌓아 두는 역할을 한다.
        /// </summary>
        /// <param name="prefabPath">확장할 풀의 키. INgoPooldata.PoolingNgoPath 값을 사용한다.</param>
        /// <param name="expandCount">추가로 미리 생성해 둘 수량.</param>
        public void ExpandPool(string prefabPath, int expandCount)
        {
            // ExpandPool은 "이미 등록된 풀을 더 채우는" 책임만 가진다.
            // 따라서 수량이 0 이하이거나, 아직 prefabPath가 등록되지 않았다면 바로 종료한다.
            if (expandCount <= 0 || m_PooledObjects.TryGetValue(prefabPath, out ObjectPool<NetworkObject> poolObject) == false)
            {
                return;
            }

            // Get으로 확보한 객체는 active 상태가 되므로,
            // 모두 확보한 뒤 마지막에 Release 하기 위해 임시 리스트에 모아 둔다.
            var prewarmNetworkObjects = new List<NetworkObject>(expandCount);

            // Get은 inactive 객체가 있으면 재사용하고,
            // 부족하면 ObjectPool의 CreateFunc을 호출해 새 인스턴스를 만든다.
            // 즉 이 루프는 expandCount 개수만큼 객체를 확보하는 단계다.
            for (int i = 0; i < expandCount; i++)
            {
                prewarmNetworkObjects.Add(poolObject.Get());
            }

            // 확보한 객체들을 모두 Release 하여 다시 inactive 상태로 돌려놓는다.
            // 결과적으로 풀 내부 총 보유 수량은 필요 시 증가하고,
            // 이후 실제 gameplay Get 호출에서는 이미 준비된 객체를 즉시 꺼낼 수 있다.
            foreach (NetworkObject networkObject in prewarmNetworkObjects)
            {
                poolObject.Release(networkObject);
            }
        }

        private void PreloadRegisteredFactoryPools()
        {
            IReadOnlyCollection<GameObject> registeredFactoryPrefabs = _resourcesServices.GetRegisteredFactoryPrefabs();
            HashSet<string> preloadTargets = new HashSet<string>();

            foreach (GameObject candidate in registeredFactoryPrefabs)
            {
                if (candidate == null || candidate.TryGetComponent(out NetworkObject _) == false)
                {
                    continue;
                }

                if (candidate.TryGetComponent(out INgoPooldata poolData) == false)
                {
                    continue;
                }

                if (poolData.PreloadOnSceneEnter == false || string.IsNullOrEmpty(poolData.PoolingNgoPath))
                {
                    continue;
                }

                preloadTargets.Add(poolData.PoolingNgoPath);
            }

            foreach (string preloadTarget in preloadTargets)
            {
                _poolManager.EnsurePoolRegistered(preloadTarget);
            }
        }
    }

    public class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        private readonly NetworkObjectPool m_NetworkObjectPool;
        private readonly string m_PrefabPath;

        public PooledPrefabInstanceHandler(NetworkObjectPool pool, string prefabPath)
        {
            m_NetworkObjectPool = pool;
            m_PrefabPath = prefabPath;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return m_NetworkObjectPool.GetNetworkObject(m_PrefabPath, position, rotation);
        }

        public void Destroy(NetworkObject networkObject)
        {
            m_NetworkObjectPool.ReturnNetworkObject(networkObject);
        }
    }
}
