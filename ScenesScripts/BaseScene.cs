using System;
using System.Collections.Generic;
using CoreScripts;
using Cysharp.Threading.Tasks;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SceneManagement;
using GameManagers.SocketManagement;
using GameManagers.UIFactoryManagement.SceneUI;
using NetWork;
using ScenesScripts.CommonInstaller.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Util;
using Zenject;
using Object = UnityEngine.Object;

namespace ScenesScripts
{
    public class MultiTestPlayerInfo
    {
        private Define.PlayerClass _playerClass;
        private PlayersTag _tag;

        public MultiTestPlayerInfo SetPlayerInfo(Define.PlayerClass playerClass, PlayersTag tag)
        {
            _playerClass = playerClass;
            _tag = tag;

            return this;
        }

        public Define.PlayerClass GetPlayerClassInfo() => _playerClass;
        public PlayersTag GetTagInfo() => _tag;
    }


    public abstract class BaseScene : ZenjectMonoBehaviour, IDisposable
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
            add => UniqueEventRegister.AddSingleEvent(ref _onZenjectInitializeAfterEvent, value);
            remove => UniqueEventRegister.AddSingleEvent(ref _onZenjectInitializeAfterEvent, value);
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

        public abstract Define.SceneName CurrentSceneName { get; }


        /// <summary>
        /// 팩토리에 등록되어있는 ISceneUI들을 전부 호출
        /// 따라서 씬에 UI를 호출하려면 팩토리만 등록하면 된다. 
        /// </summary>
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
                UtilDebug.LogWarning("There is no Scene Context");
            }
#endif
            AwakeInit();
        }

        protected virtual void StartInit()
        {
            _sceneManagerEx.SynchronizeBossSceneProgress(CurrentSceneName);

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

        protected virtual void AwakeInit()
        {
        }


        private bool OnWantsToQuit()
        {
            //이미 정리가 끝났다면 종료 허용 (true 반환)
            if (_isExitProcessDone)
            {
                return true;
            }

            // 정리가 안 됐다면 종료를 막고(false 반환), 비동기 정리 시작
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
                    //소켓 정리 시작
                    var cleanupTask = UniTask.Create(async () =>
                    {
                        await _socketEventManager.InvokeDisconnectRelayEvent(RelayDisconnectCause.ApplicationQuit);
                        await _socketEventManager.InvokeLogoutVivoxEvent();
                        await _socketEventManager.InvokeLogoutAllLeaveLobbyEvent();
                    });

                    //3초 동안 혹은 작업이 끝날 때까지 대기 함 무제한으로 기다릴 수 없으니.
                    await cleanupTask.TimeoutWithoutException(TimeSpan.FromSeconds(3f));
                }
                catch (Exception e)
                {
                    UtilDebug.LogError(e);
                }
            }


            // 방금 전 OnWantsToQuit에서 종료를 취소(return false)했기 때문에,
            // 유니티 내부 상태가 '취소 처리'를 완료하고 초기화될 때까지 1프레임 대기함.
            // 이 대기가 없으면 바로 이어지는 Application.Quit() 명령이 무시될 수 있음
            await UniTask.Yield(PlayerLoopTiming.Update);


            _isExitProcessDone = true;
            UtilDebug.Log("Safe Exit Completed. Quitting application.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit(); // 검문 없이 즉시 꺼집니다.
#endif
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            UtilDebug.Log($"Initializing scene + {SceneManagerEx.IsCurrentBootNormal}");

            UtilDebug.Log($"{gameObject.name}씬초기화 완료");
            if (_relayManager.NetworkManagerEx.IsConnectedClient == false)
            {
                UtilDebug.Log($"IsConnectedClient가 연결이 안되 구독해야함");
                _relayManager.NetworkManagerEx.OnClientConnectedCallback += ReadySender.SendClientReady;
            }
            else
            {
                UtilDebug.Log($"IsConnectedClient가 연결이 되어서 레디 신호보냄");
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
