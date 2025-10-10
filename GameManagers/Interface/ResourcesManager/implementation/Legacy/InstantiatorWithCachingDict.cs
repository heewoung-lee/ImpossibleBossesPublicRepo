using System;
using Scene.CommonInstaller;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.Interface.ResourcesManager.implementation.Legacy
{
    public class InstantiatorWithCachingDict : IInstantiate<string>, IRegistrar<ICachingObjectDict<string>>
    {
        private readonly LocalPoolManager _localPoolManager;
        private readonly NgoPoolManager _ngoPoolManager;
        private readonly IResourcesServices _resourcesServices;
        private readonly IInstantiator _instantiator;

        [Inject]
        public InstantiatorWithCachingDict(
            LocalPoolManager localPoolManager,
            NgoPoolManager ngoPoolManager,
            IResourcesServices resourcesServices,
            IInstantiator instantiator)
        {
            _localPoolManager = localPoolManager;
            _ngoPoolManager = ngoPoolManager;
            _resourcesServices = resourcesServices;
            _instantiator = instantiator;
        }
        
        

        private ICachingObjectDict<string> _cachingObjectDict;
        
        
        public void Register(ICachingObjectDict<string> sceneContext)
        {
            _cachingObjectDict = sceneContext;
        }
        public void Unregister(ICachingObjectDict<string> sceneContext)
        {
            if (_cachingObjectDict == sceneContext)
            {
                _cachingObjectDict = null;
            }
        }

        public GameObject InstantiateByKey(object key, Transform parent = null)
        {
            string path = key.ToString();
            if (string.IsNullOrEmpty(path))
                return null;


            if (_cachingObjectDict?.TryGet(path, out GameObject cachedPrefab) == true)
            {
                if (IsCheckNetworkPrefab(cachedPrefab))
                {
                    return _ngoPoolManager.Pop(path);
                }
                else
                {
                    return _localPoolManager.Pop(cachedPrefab, parent).gameObject;
                }
            }

            GameObject prefab = _resourcesServices.Load<GameObject>(path); // 먼저 path를 시도 하고 없으면 prefab붙여서 시도
            
            if (prefab == null)
            {
                string prefabPath = "Prefabs/" + path;
                prefab = _resourcesServices.Load<GameObject>(prefabPath);
            }

            if (prefab == null)
            {
                Debug.Log($"Failed to Load Object Path:{path}");
                return null;
            }

            if (prefab.GetComponent<Poolable>() != null)
            {
                _cachingObjectDict?.OverwriteData(path, prefab); //주의점 대신에 경로에 대한 딕셔너리 키는 원본경로로 들어감
                if (IsCheckNetworkPrefab(prefab))
                {
                    return _ngoPoolManager.Pop(path, parent);
                }
                else
                {
                    return _localPoolManager.Pop(prefab, parent).gameObject;
                }
            }

            //GameObject go = Object.Instantiate(prefab, parent).RemoveCloneText();
            GameObject go = InstantiatePrefab(prefab, parent);
            return go;
            
        }

        public GameObject InstantiateByKey(string key, Transform parent = null)
        {
            throw new System.NotImplementedException();
        }

        public GameObject InstantiateByGameObject(GameObject gameObjecty, Transform parent = null)
        {
            throw new System.NotImplementedException();
        }

        public GameObject InstantiateByObject(GameObject gameobject, Transform parent = null)
        {
            return InstantiatePrefab(gameobject, parent);
        }
        public T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T component = null;
            component = go.GetComponent<T>();
            if (component == null)
            {
                go.SetActive(false);
                component = _instantiator.InstantiateComponent<T>(go);
                go.SetActive(true);
            }

            return component;
        }

        public Component GetOrAddComponent(Type componentType, GameObject go)
        {
            return _instantiator.InstantiateComponent(componentType, go);
        }

        public GameObject InstantiatePrefab(GameObject prefab, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefab, parent).RemoveCloneText();
        }
        private bool IsCheckNetworkPrefab(GameObject prefab)
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening == false)
                return false; 
            
            if (prefab.TryGetComponent(out NetworkObject ngo))
            {
                return true;
            }

            return false;
        }
    }
}
