using System.Collections.Generic;
using GameManagers;
using GameManagers.Interface.NGOPoolManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Pool;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
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
        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        
        [Inject]
        public void Construct(
            IResourcesServices resourcesServices, 
            INetworkObjectGetter networkObjectGetter,
            RelayManager relayManager, 
            NgoPoolManager poolManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _poolManager = poolManager;
            _networkObjectGetter = networkObjectGetter;
            
            m_PooledObjects = new Dictionary<string, ObjectPool<NetworkObject>>();
            _poolNgoRootDict = new Dictionary<string, Transform>();
        }

        private Dictionary<string, ObjectPool<NetworkObject>> m_PooledObjects;
        public Dictionary<string, ObjectPool<NetworkObject>> PooledObjects => m_PooledObjects;
        private Dictionary<string, Transform> _poolNgoRootDict;
        public Dictionary<string, Transform>  PoolNgoRootDict => _poolNgoRootDict;
        

        public override void OnNetworkDespawn()
        {
            foreach (string prefabPath in m_PooledObjects.Keys)
            {
                m_PooledObjects[prefabPath].Clear();
                GameObject prefab = _resourcesServices.Load<GameObject>(prefabPath);
                _relayManager.NetworkManagerEx.PrefabHandler.RemoveHandler(prefab);
            }
            m_PooledObjects.Clear();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _poolManager.Set_NGO_Pool(this);

            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            gameObject.RemoveCloneText();
        }

        public NetworkObject GetNetworkObject(string prefabPath, Vector3 position, Quaternion rotation)
        {
          return _networkObjectGetter.GetNetworkObject(prefabPath, position, rotation);
        }
        public NetworkObject GetNetworkObject(string prefabPath)
        {
            return _networkObjectGetter.GetNetworkObject(prefabPath);
        }
        

        public void ReturnNetworkObject(NetworkObject networkObject)
        {
            if (networkObject.TryGetComponent(out NgoPoolingInitializeBase ngoPoolingInitializeBase))
            {
                ngoPoolingInitializeBase.OnPoolRelease();
                if (m_PooledObjects.TryGetValue(ngoPoolingInitializeBase.PoolingNgoPath,out ObjectPool<NetworkObject> poolObj))//씬 전환될때 오브젝트 풀이 비어지는데 이 풀로 반납되려는 객체가 있을때를 대비에 TryGet으로 수정
                {
                    poolObj.Release(networkObject);
                }
                else
                {
                    Debug.Log($"{networkObject.name} can't return the Pool");
                }
            }
        }
        public void RegisterPrefabInternal(string prefabPath, int prewarmCount = 5)
        {
            GameObject prefab = _resourcesServices.Load<GameObject>(prefabPath);
            if (_relayManager.NetworkManagerEx.GetNetworkPrefabOverride(prefab) == null)
            {
                Debug.Log($"{prefab.name} is not registed the NetworkManager");
                return;
            }
            m_PooledObjects[prefabPath] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);
            
            
            _relayManager.NetworkManagerEx.PrefabHandler.RemoveHandler(prefab);
            PooledPrefabInstanceHandler handler = new PooledPrefabInstanceHandler(this, prefabPath);
            _relayManager.NetworkManagerEx.PrefabHandler.AddHandler(prefab, handler);

            var prewarmNetworkObjects = new List<NetworkObject>();
            for (var i = 0; i < prewarmCount; i++)
            {
                prewarmNetworkObjects.Add(m_PooledObjects[prefabPath].Get());
            }
            foreach (var networkObject in prewarmNetworkObjects)
            {
                m_PooledObjects[prefabPath].Release(networkObject);
            }
            NetworkObject CreateFunc()
            {
                NetworkObject ngo = _resourcesServices.InstantiatePrefab(prefab, _poolManager.PoolNgoRootDict[prefabPath]).RemoveCloneText().GetComponent<NetworkObject>();
                
                 //_relayManager.NetworkManagerEx.PrefabHandler.RemoveHandler(ngo);
                //등록했던, NGO프리펩들이 가지고 있던 핸들러가 있기에 가지고 있는 핸들러를 없애고 풀용 핸들러를 장착해줘야함
                //문제없는 이후는 처음에 핸들러로 인해 컨테이너를 주입해 줬기때문에 문제없음
                return ngo;
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