using System;
using System.Collections.Generic;
using System.Text;
using GameManagers.ResourcesEx;
using Scene.CommonInstaller;
using UnityEngine;
using UnityEngine.Assertions;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;
using IInitializable = Zenject.IInitializable;

namespace GameManagers.ResourcesEx.implementation
{
    public class Instantiator : IInstantiate, IRegistrar<ICachingObjectDict>,
        IRegistrar<IDefaultGameObjectFactory>
    {
        private readonly IResourcesLoader _resourcesLoader;
        private readonly IFactoryManager _factoryController;

        private ICachingObjectDict _cachingObjectDict;
        private IDefaultGameObjectFactory _defaultGameObjectFactory;

        //여기에 씬에 등록된 IFactory객체들을 담는 거 가져와야함. 딕셔너리 <Type,Func<GameObject> 이렇게
        //어차피 NGOObject등록 인스톨러에서 바인딩을 할 예정이니 상관 없을 것 같다.


        public Instantiator(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IResourcesLoader resourcesLoader, IFactoryManager factoryController)
        {
            _resourcesLoader = resourcesLoader;
            _factoryController = factoryController;
        }

        public void Register(ICachingObjectDict sceneContext)
        {
            _cachingObjectDict = sceneContext;
        }

        public void Unregister(ICachingObjectDict sceneContext)
        {
            if (_cachingObjectDict == sceneContext)
            {
                _cachingObjectDict = null;
            }
        }

        public void Register(IDefaultGameObjectFactory sceneContext)
        {
            _defaultGameObjectFactory = sceneContext;
        }

        public void Unregister(IDefaultGameObjectFactory sceneContext)
        {
            if (_defaultGameObjectFactory != null)
            {
                _defaultGameObjectFactory = null;
            }
        }

        public GameObject InstantiateByKey(string key, Transform parent = null)
        {
            GameObject prefabObj = null;
            if (_cachingObjectDict?.TryGet(key, out GameObject cachedPrefab) == true)
            {
                prefabObj = cachedPrefab;
            }
            else
            {
                prefabObj = _resourcesLoader.Load<GameObject>(key); // 키를 통해 Load prefab붙여서 시도
                //_defaultGameObjectFactory?.InjectionGameObject(prefabObj);//모든 객체들이 처음 등록될때 컨테이너 주입
                //8.21일 모든 로드 되는 객체에 SceneContext의 Container를 주입하니 GameObjectContext쪽에서 주입이 필요한 경우 에러가 생김
                //씬컨테이너 주입 후 -> GameObject컨테이너 주입 (이중 주입 오류) 각자 컨테이너에 맞는 Inject를 할것
                //해결 씬컨테이너로 생성되는 객체외에 다른 객체들을 전부 Factory로 밀어넣었음.
                //그래서 이후에 팩토리를 통해 생성되는 객체가 자신의 컨테이너를 들고 스폰되게끔 만들었고.
                //팩토리가 없다면 기본 컨테이너(씬 컨테이너) 에서 생성되도록 만듦
                _cachingObjectDict?.OverwriteData(key, prefabObj);
            }
  

            if (prefabObj.TryGetComponent(out Poolable poolable) == true)
            {
                _defaultGameObjectFactory.InjectionGameObject(poolable.gameObject);
                return poolable.Pop();
            }

            GameObject go = InstantiatePrefab(prefabObj, parent);
            return go;
        }

        public GameObject InstantiatePrefab(GameObject prefabObj, Transform parent = null)
        {
            Debug.Assert(_defaultGameObjectFactory != null, "defaultContainer Is null"); //멈추면 안됨

            Assert.IsNotNull(prefabObj, $"{prefabObj.name} Is null"); // 멈춰야함

            if (_factoryController != null &&
                _factoryController.TryGetCreator(prefabObj, out Func<Transform, GameObject> factoryCreator))
            {
                Assert.IsNotNull(factoryCreator, $"{prefabObj.name} Is null");
                return factoryCreator.Invoke(parent).ReplaceWithText(new StringBuilder("(Factory)"));
            }

            return _defaultGameObjectFactory.Create(prefabObj, parent).RemoveCloneText();
        }
        public T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            Debug.Assert(go != null, "gameObject Is null");
            
            T component = go.GetComponent<T>(); 
            if (component != null)
                return component;

            component = _defaultGameObjectFactory.GetorAddComponent<T>(go);

            if (component is IInitializable init)
            {
                init.Initialize();  
            }
            
            return component;
        }

        public Component GetOrAddComponent(Type componentType, GameObject go)
        {
            if (go == null || componentType == null) return null;
            if (!typeof(Component).IsAssignableFrom(componentType))
                throw new ArgumentException($"{componentType} is not a Component.");

            Component component = go.GetComponent(componentType);
            if (component != null)
                return component;
            
            component = _defaultGameObjectFactory.GetorAddComponent(componentType, go);
            
            if (component is IInitializable init)
            {
                init.Initialize();  
            }

            return component;
        }
    }
}