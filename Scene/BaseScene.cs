using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controller;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.SceneUIManager;
using GameManagers.Interface.UIManager;
using NetWork;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Util;
using Zenject;
using Object = UnityEngine.Object;

namespace Scene
{
    
    public class MultiTestPlayerInfo
    {
        private Define.PlayerClass _playerClass;
        private PlayersTag _tag;
        public MultiTestPlayerInfo SetPlayerInfo(Define.PlayerClass playerClass,PlayersTag tag)
        {
            _playerClass = playerClass;
            _tag = tag;
            
            return this;
        }

        public Define.PlayerClass GetPlayerClassInfo() => _playerClass;
        public PlayersTag GetTagInfo() => _tag;
    }

    
    public abstract class BaseScene : MonoBehaviour,IInitializable,IDisposable
    {
        private IResourcesServices _resourcesServices;
        private IEnumerable<ISceneUI> _sceneUIs;
        private SocketEventManager _socketEventManager;
        private SceneManagerEx _sceneManagerEx;
        private RelayManager _relayManager;
        private Action _onZenjectInitializeAfterEvent;

        private bool _checkDoneZenjectInitilaize = false;
        public bool CheckDoneZenjectInitialize => _checkDoneZenjectInitilaize;
        public event Action OnZenjctInitializeAfterEvent
        {
            add=> UniqueEventRegister.AddSingleEvent(ref _onZenjectInitializeAfterEvent,value);
            remove => UniqueEventRegister.AddSingleEvent(ref _onZenjectInitializeAfterEvent,value);
        }
        
        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            IEnumerable<ISceneUI> sceneUIs,
            SocketEventManager socketEventManager,
            SceneManagerEx sceneManagerEx,
            RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _sceneUIs = sceneUIs;
            _socketEventManager = socketEventManager;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
        }
        
        public abstract Define.Scene CurrentScene { get; }


        private void CallSceneUIs()
        {
            foreach (ISceneUI scneeUI in _sceneUIs)
            {
                scneeUI.SceneGameObjectCreate();
            }
        }
        
        
        void Start()
        {
            StartInit();
            CallSceneUIs();
            _sceneManagerEx.SetBootMode(true);
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (FindAnyObjectByType<SceneContext>() == null)
            { 
                Debug.LogWarning("There is no Scene Context");
            }
#endif
            AwakeInit();
        }

        protected virtual void StartInit()
        {
            Object go = GameObject.FindAnyObjectByType<EventSystem>();
            if (go == null)
            {
                _resourcesServices.InstantiateByKey("Prefabs/UI/EventSystem").name = "@EventSystem";
            }
            _checkDoneZenjectInitilaize = true;
            _onZenjectInitializeAfterEvent?.Invoke();
            _onZenjectInitializeAfterEvent = null;
        }

        protected virtual void AwakeInit()  {}
        public virtual void Clear() {}
        
        public async void OnApplicationQuit()
        {
            Assert.IsNotNull(_socketEventManager,"socketEventManager is null");
            if (_socketEventManager == null) return;
            try
            {
                await _socketEventManager.InvokeDisconnectRelayEvent();
                await _socketEventManager.InvokeLogoutVivoxEvent();
                await _socketEventManager.InvokeLogoutAllLeaveLobbyEvent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            //순서대로 끊어야 함
            
        }
        public void Initialize()
        {
            Debug.Log($"{gameObject.name}씬초기화 완료");
            if (_relayManager.NetworkManagerEx.IsConnectedClient == false)
            {
                _relayManager.NetworkManagerEx.OnClientConnectedCallback += ReadySender.SendClientReady;
            }
            else
            {
                ReadySender.SendClientReady(_relayManager.NetworkManagerEx.LocalClientId);
            }
        }

        public void Dispose()
        {
            if (_relayManager.NetworkManagerEx.IsConnectedClient == true)
            {
                _relayManager.NetworkManagerEx.OnClientConnectedCallback -= ReadySender.SendClientReady;
            }
        }
    }
}