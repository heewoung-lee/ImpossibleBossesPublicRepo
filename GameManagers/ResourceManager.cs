using System;
using GameManagers.Interface.ResourcesManager;
using ProjectContextInstaller;
using UnityEngine;
using Util;
using Zenject;


namespace GameManagers
{
    internal sealed class ResourceManager<TKey> : IResourcesServices<TKey>,IResourcesServices
    {
        private readonly IResourcesLoader<TKey> _assetLoader;
        private readonly IInstantiate<TKey> _instantiator;
        private readonly IDestroyObject _destroyObject;
        
        public ResourceManager(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IResourcesLoader<TKey> assetLoader, 
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IInstantiate<TKey> instantiator, 
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IDestroyObject destroyObject)
        {
            _assetLoader = assetLoader;
            _instantiator = instantiator;
            _destroyObject = destroyObject;
        }
        
        
        public T Load<T>(TKey key) where T : UnityEngine.Object
        {
            return _assetLoader.Load<T>(key);
        }
        public T[] LoadAll<T>(TKey key) where T : UnityEngine.Object
        {
            return _assetLoader.LoadAll<T>(key);
        }
        public bool TryGetLoad<T>(TKey key, out T loadItem) where T : UnityEngine.Object
        {
           return _assetLoader.TryGetLoad<T>(key, out loadItem);
        }
        
        public GameObject InstantiateByKey(TKey key, Transform parent = null)
        {
            return _instantiator.InstantiateByKey(key, parent);
        }


        public T Load<T>(System.Object key) where T : UnityEngine.Object
        {
           return _assetLoader.Load<T>((TKey)key);
        }

        public T[] LoadAll<T>(System.Object key) where T : UnityEngine.Object
        {
            return _assetLoader.LoadAll<T>((TKey)key);
        }

        public bool TryGetLoad<T>(System.Object key, out T loadItem) where T : UnityEngine.Object
        {
            return _assetLoader.TryGetLoad<T>((TKey)key, out loadItem);
        }

        public GameObject InstantiateByKey(System.Object key, Transform parent = null)
        {
            return _instantiator.InstantiateByKey((TKey)key, parent);
        }

        public T GetOrAddComponent<T>(GameObject go) where T : Component
        {
           return _instantiator.GetOrAddComponent<T>(go);
        }

        public Component GetOrAddComponent(Type componentType, GameObject go)
        {
            return _instantiator.GetOrAddComponent(componentType, go);
        }

        public void DestroyObject(GameObject go, float delay = 0)
        {
            _destroyObject.DestroyObject(go, delay);
        }

        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefabObj, parent);
        }
    }
}