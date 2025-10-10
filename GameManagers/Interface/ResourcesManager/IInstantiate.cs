using System;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.ResourcesManager
{
    public interface IInstantiate<in TKey>
    {
        public GameObject InstantiateByKey(TKey key, Transform parent = null);
        public T GetOrAddComponent<T>(GameObject go) where T : Component;
        public Component GetOrAddComponent(Type componentType, GameObject go);
        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null);
    }

}