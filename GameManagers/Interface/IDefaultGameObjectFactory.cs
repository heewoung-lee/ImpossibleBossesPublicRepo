// IDefaultGameObjectFactory.cs

using System;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface
{
    public interface IDefaultGameObjectFactory
    {
        public GameObject Create(GameObject prefab, Transform parent = null);
        public T GetorAddComponent<T>(GameObject go) where T : Component;
        public Component GetorAddComponent(Type type, GameObject go);
        public void InjectionGameObject(GameObject go);
    }
}