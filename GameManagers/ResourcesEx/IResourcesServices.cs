using System;
using UnityEngine;
using Object = System.Object;

namespace GameManagers.ResourcesEx
{
    public interface IResourcesServices
    {
        public T Load<T>(string key) where T : UnityEngine.Object;
        public T[] LoadAll<T>(string key) where T : UnityEngine.Object;
        public bool TryGetLoad<T>(string key, out T loadItem) where T : UnityEngine.Object;
        public GameObject InstantiateByKey(string key, Transform parent = null);
        public T GetOrAddComponent<T>(GameObject go) where T : Component;
        public Component GetOrAddComponent(Type componentType, GameObject go);
        public void DestroyObject(GameObject go,float delay = 0f);
        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null);
    }
}
