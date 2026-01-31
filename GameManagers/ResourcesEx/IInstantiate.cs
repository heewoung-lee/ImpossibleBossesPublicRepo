using System;
using UnityEngine;

namespace GameManagers.ResourcesEx
{
    public interface IInstantiate
    {
        public GameObject InstantiateByKey(string key, Transform parent = null);
        public T GetOrAddComponent<T>(GameObject go) where T : Component;
        public Component GetOrAddComponent(Type componentType, GameObject go);
        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null);
    }

}