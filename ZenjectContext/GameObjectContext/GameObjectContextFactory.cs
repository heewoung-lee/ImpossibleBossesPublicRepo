using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Enemy.Boss.Installer;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace ZenjectContext.GameObjectContext
{
    
    public abstract class GameObjectContextFactory<T> : IFactory<Transform,T>,IRegisteredFactoryObject,IInitializable,IDisposable where T : MonoBehaviour
    {
        protected GameObject _requestGO;
        protected readonly DiContainer _container;
        protected readonly IFactoryController _registerableFactory;
      
        protected GameObjectContextFactory(
            DiContainer container,
            IResourcesServices loadService,
            IFactoryController registerableFactory)
        {
            _container = container;
            _registerableFactory = registerableFactory;
        }
        
        public T Create(Transform parantTr = null)
        {
            bool isActive = RequestObject.gameObject.activeSelf;
            
            RequestObject.gameObject.SetActive(false);
            GameObject instance = UnityEngine.Object.Instantiate(RequestObject, parantTr);
            _container.InjectGameObject(instance);
            
            instance.SetActive(isActive);
            RequestObject.SetActive(isActive);
            //0905 기존에 InstantiatePrefabForComponent를 쓰면 부모Tr을 null로 설정해도 호출한 오브젝트를 부모로 삼는 문제가있음
            //UnityEngine.Object.Instantiate를 쓰고 생성된 인스턴스에 주입을 하기도 전에 Awake가 실행되니 주입이 안되는문제가 생겨서
            //로드된 오브젝트를 끄고 주입한뒤 원래의 활성값을 넣어주는것으로 마무리
            
            return  instance.GetComponent<T>();
        }

        public GameObject RequestObject => _requestGO;
        public Func<Transform,GameObject> Creator => (parentTr) => Create(parentTr).gameObject;

        public void Initialize()
        {
            _registerableFactory.TryRegisterGameObjectContextFactory(RequestObject,Creator);
        }

        //When the GameObject that is attached to the GameObjectContext is removed,
        //you should remove the key–value pair from the dictionary in the CreateFactory.
        public void Dispose()
        {
            Debug.Log($"factoryName : {typeof(T)} Dispose called");
            _registerableFactory.RemoveFactory(_requestGO);
        }
    }
    
    
    
    public abstract class NgoZenjectFactory<T> : IFactory<Transform,T>,IRegisteredFactoryObject,IInitializable,IDisposable where T : MonoBehaviour
    {
        private NgoZenjectHandler _ngoZenjectHandler;
        private NgoZenjectHandler.NgoZenjectHandlerFactory _handlerFactory;
        
        protected GameObject _requestGO;
        protected readonly DiContainer _container;
        protected readonly IFactoryRegister _registerableFactory;
        [Inject]
        protected NgoZenjectFactory(
            DiContainer container,
            IFactoryRegister registerableFactory,
            NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService)
        {
            _container = container;
            _registerableFactory = registerableFactory;
            _handlerFactory = handlerFactory;
        }
        
        public T Create(Transform parantTr = null)
        {
            bool isRequestObjActive = RequestObject.activeSelf;
            
            RequestObject.gameObject.SetActive(false);
            GameObject instance = UnityEngine.Object.Instantiate(RequestObject, parantTr);
            _container.InjectGameObject(instance);
            //한번 강제 주입을 해줘야함.
            
            instance.SetActive(isRequestObjActive);
            RequestObject.SetActive(isRequestObjActive);
            //0905 기존에 InstantiatePrefabForComponent를 쓰면 부모Tr을 null로 설정해도 호출한 오브젝트를 부모로 삼는 문제가있음
            //UnityEngine.Object.Instantiate를 쓰고 생성된 인스턴스에 주입을 하기도 전에 Awake가 실행되니 주입이 안되는문제가 생겨서
            //로드된 오브젝트를 끄고 주입한뒤 원래의 활성값을 넣어주는것으로 마무리
            
            return  instance.GetComponent<T>();
        }

        public GameObject RequestObject => _requestGO;
        public Func<Transform,GameObject> Creator => (parentTr) => Create(parentTr).gameObject;

        public void Initialize()
        {
            //로드된 객체에 컨테이너 주입과 네트워크 핸들러 등록
            //어차피 종료되면 메모리가 자동으로 날려주기 때문에 괜찮을꺼라 생각한다.
            _ngoZenjectHandler = _handlerFactory.Create(_container,RequestObject);
            NetworkManager.Singleton.PrefabHandler.AddHandler(RequestObject, _ngoZenjectHandler);
            _registerableFactory.TryRegisterFactory(RequestObject,Creator);
            
        }

        //When the GameObject that is attached to the GameObjectContext is removed,
        //you should remove the key–value pair from the dictionary in the CreateFactory.
        public void Dispose()
        {
            Debug.Log($"factoryName : {typeof(T)} Dispose called");
            _registerableFactory.RemoveFactory(RequestObject); //생성 팩토리 모음에 등록된 것 제거
        }
        
    }
    
    public abstract class NgoZenjectFactory<TKey,T> : IFactory<Transform,T>,IRegisteredFactoryObject,IInitializable,IDisposable where T : MonoBehaviour
    {
        private NgoZenjectHandler _ngoZenjectHandler;
        private NgoZenjectHandler.NgoZenjectHandlerFactory _handlerFactory;
        
        protected GameObject _requestGO;
        protected readonly DiContainer _container;
        protected readonly IFactoryRegister _registerableFactory;
        protected TKey key;
        [Inject]
        protected NgoZenjectFactory(
            DiContainer container,
            IFactoryRegister registerableFactory,
            NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService,TKey key)
        {
            _container = container;
            _registerableFactory = registerableFactory;
            _handlerFactory = handlerFactory;
        }
        
        public T Create(Transform parantTr = null)
        {
            bool isRequestObjActive = RequestObject.activeSelf;
            
            RequestObject.gameObject.SetActive(false);
            GameObject instance = UnityEngine.Object.Instantiate(RequestObject, parantTr);
            _container.InjectGameObject(instance);
            //한번 강제 주입을 해줘야함.
            
            instance.SetActive(isRequestObjActive);
            RequestObject.SetActive(isRequestObjActive);
            //0905 기존에 InstantiatePrefabForComponent를 쓰면 부모Tr을 null로 설정해도 호출한 오브젝트를 부모로 삼는 문제가있음
            //UnityEngine.Object.Instantiate를 쓰고 생성된 인스턴스에 주입을 하기도 전에 Awake가 실행되니 주입이 안되는문제가 생겨서
            //로드된 오브젝트를 끄고 주입한뒤 원래의 활성값을 넣어주는것으로 마무리
            
            return  instance.GetComponent<T>();
        }

        public GameObject RequestObject => _requestGO;
        public Func<Transform,GameObject> Creator => (parentTr) => Create(parentTr).gameObject;

        public void Initialize()
        {
            //로드된 객체에 컨테이너 주입과 네트워크 핸들러 등록
            //어차피 종료되면 메모리가 자동으로 날려주기 때문에 괜찮을꺼라 생각한다.
            _ngoZenjectHandler = _handlerFactory.Create(_container,RequestObject);
            NetworkManager.Singleton.PrefabHandler.AddHandler(RequestObject, _ngoZenjectHandler);
            _registerableFactory.TryRegisterFactory(RequestObject,Creator);
            
        }

        //When the GameObject that is attached to the GameObjectContext is removed,
        //you should remove the key–value pair from the dictionary in the CreateFactory.
        public void Dispose()
        {
            Debug.Log($"factoryName : {typeof(T)} Dispose called");
            _registerableFactory.RemoveFactory(RequestObject); //생성 팩토리 모음에 등록된 것 제거
        }
        
    }
    
}
