using System;
using GameManagers.ResourcesEx;
using ProjectContextInstaller;
using Scene.CommonInstaller;
using UnityEngine;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.CommonImplements
{
    public class DefaultGameObjectFactory : IDefaultGameObjectFactory,IDisposable
    {
        private readonly DiContainer _container;
        private readonly IRegistrar<IDefaultGameObjectFactory> _registrarIDefaultGameObjectFactory;
        public DefaultGameObjectFactory(
            DiContainer container,
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IRegistrar<IDefaultGameObjectFactory> registrarIDefaultGameObjectFactory)
        {
            _container = container;
            _registrarIDefaultGameObjectFactory = registrarIDefaultGameObjectFactory;
            _registrarIDefaultGameObjectFactory.Register(this);
        }
        public void Dispose()
        {
            _registrarIDefaultGameObjectFactory.Unregister(this);
        }
        public T GetorAddComponent<T>(GameObject go) where T : Component
        {
            return _container.InstantiateComponent<T>(go);
        }
        public Component GetorAddComponent(Type componentType, GameObject go)
        {
            return _container.InstantiateComponent(componentType, go);
        }
        public GameObject Create(GameObject prefab, Transform parent = null)
        {
            if (prefab == null) return null;
            return _container.InstantiatePrefab(prefab, parent);
        }
        public void InjectionGameObject(GameObject go)
        {
            _container.InjectGameObject(go);
            
        }

    }
}