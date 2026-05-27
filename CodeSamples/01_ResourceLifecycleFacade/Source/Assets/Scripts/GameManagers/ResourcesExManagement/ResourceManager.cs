using System.Collections.Generic;
using System;
using UnityEngine;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.ResourcesExManagement
{
    internal sealed class ResourceManager : IResourcesServices
    {
        private readonly IResourcesLoader _assetLoader;
        private readonly IInstantiate _instantiator;
        private readonly IDestroyObject _destroyObject;
        private readonly IFactoryManager _factoryManager;
        
        public ResourceManager(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IResourcesLoader assetLoader, 
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IInstantiate instantiator, 
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IDestroyObject destroyObject,
            IFactoryManager factoryManager)
        {
            _assetLoader = assetLoader;
            _instantiator = instantiator;
            _destroyObject = destroyObject;
            _factoryManager = factoryManager;
        }
        
        
        public T Load<T>(string key) where T : UnityEngine.Object
        {
            return _assetLoader.Load<T>(key);
        }
        public T[] LoadAll<T>(string key) where T : UnityEngine.Object
        {
            return _assetLoader.LoadAll<T>(key);
        }
        public bool TryGetLoad<T>(string key, out T loadItem) where T : UnityEngine.Object
        {
           return _assetLoader.TryGetLoad<T>(key, out loadItem);
        }
        
        
        public GameObject InstantiateByKey(string key, Transform parent = null)
        {
            return _instantiator.InstantiateByKey(key, parent);
        }

        public T GetOrAddComponent<T>(GameObject go) where T : Component
        {
           return _instantiator.GetOrAddComponent<T>(go);
        }
        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefabObj, parent);
        }
        
        public Component GetOrAddComponent(Type componentType, GameObject go)
        {
            return _instantiator.GetOrAddComponent(componentType, go);
        }

        
        
        public void DestroyObject(GameObject go, float delay = 0)
        {
            _destroyObject.DestroyObject(go, delay);
        }

        public IReadOnlyCollection<GameObject> GetRegisteredFactoryPrefabs()
        {
            return _factoryManager.GetRegisteredFactoryPrefabs();
        }

    }
}
