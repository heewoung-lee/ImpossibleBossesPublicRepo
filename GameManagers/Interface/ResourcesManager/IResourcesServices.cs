using System;
using System.Collections;
using UnityEngine;
using Object = System.Object;

namespace GameManagers.Interface.ResourcesManager
{
    public interface IResourcesServices<in TKey>
    {
        public T Load<T>(TKey key) where T : UnityEngine.Object;
        public T[] LoadAll<T>(TKey key) where T :  UnityEngine.Object;
        public bool TryGetLoad<T>(TKey key, out T loadItem) where T : UnityEngine.Object;
        public GameObject InstantiateByKey(TKey key, Transform parent = null);
        public T GetOrAddComponent<T>(GameObject go) where T : Component;
        public Component GetOrAddComponent(Type componentType, GameObject go);
    }

    public interface IResourcesServices
    {
        public T Load<T>(Object key) where T : UnityEngine.Object;
        public T[] LoadAll<T>(Object key) where T : UnityEngine.Object;
        public bool TryGetLoad<T>(Object key, out T loadItem) where T : UnityEngine.Object;
        public GameObject InstantiateByKey(Object key, Transform parent = null);
        public T GetOrAddComponent<T>(GameObject go) where T : Component;
        public Component GetOrAddComponent(Type componentType, GameObject go);
        public void DestroyObject(GameObject go,float delay = 0f);
        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null);
    }
}
