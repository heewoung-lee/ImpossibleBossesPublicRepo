using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controller;
using CoreScripts;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using GameManagers.Scene;
using GameManagers.UIFactory.SceneUI;
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

    
    public abstract class BaseScene : ZenjectMonoBehaviour,IDisposable
    {
        private IResourcesServices _resourcesServices;
        private IEnumerable<ISceneUI> _sceneUIs;
        private SocketEventManager _socketEventManager;
        private SceneManagerEx _sceneManagerEx;
        private RelayManager _relayManager;
        private Action _onZenjectInitializeAfterEvent;

        private bool _checkDoneZenjectInitialize = false;
        private bool _isExitProcessDone = false;
        public bool CheckDoneZenjectInitialize => _checkDoneZenjectInitialize;
        public event Action OnZenjectInitializeAfterEvent
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
            _checkDoneZenjectInitialize = true;
            _onZenjectInitializeAfterEvent?.Invoke();
            _onZenjectInitializeAfterEvent = null;
            Application.wantsToQuit += OnWantsToQuit;
        }

        protected virtual void AwakeInit()  {}
        
        
        
        
        private bool OnWantsToQuit()
        {
            // 1. 이미 정리가 끝났다면 종료 허용 (true 반환)
            if (_isExitProcessDone)
            {
                return true;
            }

            // 2. 정리가 안 됐다면 종료를 막고(false 반환), 비동기 정리 시작
            ExitSequence().Forget();
            return false;
        }
        
        private async UniTaskVoid ExitSequence()
        {
            Assert.IsNotNull(_socketEventManager, "socketEventManager is null");
            
            if (_socketEventManager != null)
            {
                try
                {
                    var cleanupTask = UniTask.Create(async () =>
                    {
                        await _socketEventManager.InvokeDisconnectRelayEvent();
                        await _socketEventManager.InvokeLogoutVivoxEvent();
                        await _socketEventManager.InvokeLogoutAllLeaveLobbyEvent();
                    });
                    await cleanupTask.TimeoutWithoutException(TimeSpan.FromSeconds(3f));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _isExitProcessDone = true;
            Debug.Log("Safe Exit Completed. Quitting application.");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            Debug.Log($"Initializing scene + {SceneManagerEx.IsCurrentBootNormal}");
            
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
                Application.wantsToQuit -= OnWantsToQuit;
                _relayManager.NetworkManagerEx.OnClientConnectedCallback -= ReadySender.SendClientReady;
            }
        }
    }
}