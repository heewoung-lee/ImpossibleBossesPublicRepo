using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Enemy.Boss.Installer;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.ResourcesEx;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace ZenjectContext.GameObjectContext
{
    public abstract class GameObjectContextFactory<T> : IFactory<Transform, T>, IRegisteredFactoryObject,
        IInitializable, IDisposable where T : MonoBehaviour
    {
        protected GameObject _requestGO;
        protected readonly DiContainer _container;
        protected readonly IFactoryManager _factoryManager;

        protected GameObjectContextFactory(
            DiContainer container,
            IFactoryManager factoryManager)
        {
            _container = container;
            _factoryManager = factoryManager;
        }

        public T Create(Transform parantTr = null)
        {
            bool isActive = RequestObject.gameObject.activeSelf;
            RequestObject.gameObject.SetActive(false);
            try
            {
                GameObject instance = UnityEngine.Object.Instantiate(RequestObject, parantTr);
                _container.InjectGameObject(instance);

                instance.SetActive(isActive);
                RequestObject.SetActive(isActive);
                //0905 기존에 InstantiatePrefabForComponent를 쓰면 부모Tr을 null로 설정해도 호출한 오브젝트를 부모로 삼는 문제가있음
                //UnityEngine.Object.Instantiate를 쓰고 생성된 인스턴스에 주입을 하기도 전에 Awake가 실행되니 주입이 안되는문제가 생겨서
                //로드된 오브젝트를 끄고 주입한뒤 원래의 활성값을 넣어주는것으로 마무리

                return instance.GetComponent<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Factory] {typeof(T).Name} 생성 중 오류 발생: {ex.Message}");
                throw;
            }
            finally
            {
                if (RequestObject != null)
                {
                    RequestObject.SetActive(isActive);
                }
            }
        }

        public GameObject RequestObject => _requestGO;
        public Func<Transform, GameObject> Creator => (parentTr) => Create(parentTr).gameObject;

        public void Initialize()
        {
            _factoryManager.TryRegisterFactory(RequestObject, Creator);
        }

        //When the GameObject that is attached to the GameObjectContext is removed,
        //you should remove the key–value pair from the dictionary in the CreateFactory.
        public void Dispose()
        {
            // Debug.Log($"factoryName : {typeof(T)} Dispose called");

            if (_factoryManager.IsKeyRegistered(_requestGO))
            {
                _factoryManager.RemoveFactory(_requestGO);
            }
        }
    }

    public abstract class NgoZenjectFactory<T> : IFactory<Transform, T>, IRegisteredFactoryObject, IInitializable,
        IDisposable where T : MonoBehaviour
    {
        private NgoZenjectHandler _ngoZenjectHandler;
        private NgoZenjectHandler.NgoZenjectHandlerFactory _handlerFactory;

        protected GameObject _requestGO;
        protected readonly DiContainer _container;
        protected readonly IFactoryManager _factoryManager;

        [Inject]
        protected NgoZenjectFactory(
            DiContainer container,
            IFactoryManager factoryManager,
            NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService)
        {
            _container = container;
            _factoryManager = factoryManager;
            _handlerFactory = handlerFactory;
        }

        public T Create(Transform parantTr = null)
        {
            bool isRequestObjActive = RequestObject.activeSelf;
            RequestObject.gameObject.SetActive(false);
            //12.30일 tryCatch 추가
            //추가한 이유는 팩토리에서 로드된 객체를 읽고 주입을 하기 위해선
            //로드된객체가 꺼져있어야 함. 그런데 만약 생성중에 에러가 났다면,
            //원본 프리펩이 비활성화 된 상태로 그대로 남아있게 돼서
            //에러를 찾기가 너무 힘들어짐. 그래서 에러가 나더라도, 기존의 상태를 그대로 복구할 수 있게
            //TryCatch를 씀
            
            try
            {
                GameObject instance = UnityEngine.Object.Instantiate(RequestObject, parantTr);

                _container.InjectGameObject(instance);
                //한번 강제 주입을 해줘야함.

                instance.SetActive(isRequestObjActive);
                RequestObject.SetActive(isRequestObjActive);
                //0905 기존에 InstantiatePrefabForComponent를 쓰면 부모Tr을 null로 설정해도 호출한 오브젝트를 부모로 삼는 문제가있음
                //UnityEngine.Object.Instantiate를 쓰고 생성된 인스턴스에 주입을 하기도 전에 Awake가 실행되니 주입이 안되는문제가 생겨서
                //로드된 오브젝트를 끄고 주입한뒤 원래의 활성값을 넣어주는것으로 마무리

                return instance.GetComponent<T>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Factory] {typeof(T).Name} 생성 중 오류 발생: {e.Message}");
                throw;
            }
            finally
            {
                if (RequestObject != null)
                {
                   RequestObject.SetActive(isRequestObjActive);
                }
            }
        }

        public GameObject RequestObject => _requestGO;
        public Func<Transform, GameObject> Creator => (parentTr) => Create(parentTr).gameObject;

        public void Initialize()
        {
            //로드된 객체에 컨테이너 주입과 네트워크 핸들러 등록
            //네트워크 해제는 따로 작성을 안했다.            
            //어차피 프로그램이 종료되면 메모리가 자동으로 날라가기 때문에 괜찮을꺼라 생각한다.
            //나중에 문제 생기면 그때 등록할것
            _ngoZenjectHandler = _handlerFactory.Create(_container, RequestObject);
            NetworkManager.Singleton.PrefabHandler.AddHandler(RequestObject, _ngoZenjectHandler);
            _factoryManager.TryRegisterFactory(RequestObject, Creator);
        }

        //When the GameObject that is attached to the GameObjectContext is removed,
        //you should remove the key–value pair from the dictionary in the CreateFactory.
        public void Dispose()
        {
            if (_factoryManager.IsKeyRegistered(RequestObject))
            {
                _factoryManager.RemoveFactory(RequestObject); //생성 팩토리 모음에 등록된 것 제거
            }
            //Debug.Log($"factoryName : {typeof(T)} Dispose called");
        }
    }
}