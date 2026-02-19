using System;
using System.Collections.Generic;
using GameManagers.Interface;
using Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Zenject;

namespace GameManagers.Scene
{
    public class SceneManagerEx
    {
        private readonly RelayManager.RelayManager _relayManager;
        private readonly IEnumerable<IResettable> _resettables;

        [Inject]
        public SceneManagerEx(RelayManager.RelayManager relayManager, IEnumerable<IResettable> resettables)
        {
            _relayManager = relayManager;
            _resettables = resettables;
        }

        private static bool _isCurrentBootNormal = false;

        private Define.Scene _currentScene;
        private Define.Scene _nextScene;
        private bool[] _loadingSceneTaskChecker;

        private Action _onBeforeSceneUnloadLocalEvent;
        private Action<ulong> _onClientLoadedEvent;
        private Action _onAllPlayerLoadedEvent;


        public static bool IsCurrentBootNormal => _isCurrentBootNormal;

        public event Action OnBeforeSceneUnloadLocalEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onBeforeSceneUnloadLocalEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onBeforeSceneUnloadLocalEvent, value); }
        }


        /// <summary>
        /// 클라이언트가 로드될때 쓰는 이벤트 주의 할점은 구독하면 다음 씬에 호출되고 없어지는 일회용
        /// 형태이기 때문에 이전 씬에서 특정 클라이언트에게 무언가 호출하고 싶을때 사용 
        /// </summary>
        public event Action<ulong> OnClientLoadedEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onClientLoadedEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onClientLoadedEvent, value); }
        }


        public event Action OnAllPlayerLoadedEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onAllPlayerLoadedEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onAllPlayerLoadedEvent, value); }
        }


        public BaseScene GetCurrentScene
        {
            get => GameObject.FindAnyObjectByType<BaseScene>();
        }

        public BaseScene[] GetCurrentScenes
        {
            get => GameObject.FindObjectsByType<BaseScene>(FindObjectsSortMode.None);
        }

        public Define.Scene CurrentScene => GetCurrentScene.CurrentScene;
        public Define.Scene NextScene => _nextScene;

        public bool[] LoadingSceneTaskChecker => _loadingSceneTaskChecker;

        public void SetNormalBootMode(bool bootMode)
        {
            _isCurrentBootNormal = bootMode;
        }

        public void LoadScene(Define.Scene nextscene)
        {
            SceneManager.LoadScene(GetSceneNameByEnumName(nextscene));
        }

        public void SetCheckTaskChecker(bool[] checkTaskChecker)
        {
            _loadingSceneTaskChecker = checkTaskChecker;
        }

        public void LoadSceneWithLoadingScreen(Define.Scene nextscene)
        {
            _nextScene = nextscene;
            LoadScene(Define.Scene.LoadingScene);
        }

        public void InvokeOnBeforeSceneUnloadLocalEvent()
        {
            _onBeforeSceneUnloadLocalEvent?.Invoke();
            UtilDebug.Log("씬 로드 되기전 호출");
        }


        public void NetworkLoadScene(Define.Scene nextscene)
        {
            _relayManager.NgoRPCCaller.OnBeforeSceneUnloadLocalRpc(); //모든 플레이어가 씬 호출전 실행해야할 이벤트(로컬 각자가 맡음)
            _relayManager.NgoRPCCaller.OnBeforeSceneUnloadRpc(); //모든 플레이어가 씬 호출전 실행해야할 넷워크 오브젝트 초기화(호스트가 맡음)
            _relayManager.NetworkManagerEx.SceneManager.OnLoadComplete += SceneManagerOnLoadCompleteAsync;
            _relayManager.NetworkManagerEx.SceneManager.LoadScene(GetSceneNameByEnumName(nextscene),
                UnityEngine.SceneManagement.LoadSceneMode.Single);
            void SceneManagerOnLoadCompleteAsync(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
            {
                if (sceneName == nextscene.ToString() && loadSceneMode == LoadSceneMode.Single)
                {
                    _relayManager.NgoRPCCaller.LoadedPlayerCount++;


                    //이걸 만든 이유는 아래 플레이어를 스폰할 때 만약 젠젝트의 주입을 기다리고 있는 상황에서
                    //호스트가 로딩을 전부 완료한 상황이라면 호스트는 _onClientLoadedEvent = null 으로 바꿈
                    //그러면 다른 클라이언트들이 젠젝트 주입이 완료될 때, _onClientLoadedEvent == null이여서
                    //자신의 캐릭터를 스폰을 못하는상황이 생길 수 도 있어서 만듦
                    Action<ulong> actionToTemp = _onClientLoadedEvent;
                    
                    
                    //10.2 수정
                    //씬을 넘길때 젠젝트의 초기화가 완료되기도 전에 플레이어를 호출하는 에러가 생김
                    //BaseScene에 이벤트를 하나 만들어서 씬이 초기화가 완료되기 이전이면
                    //초기화 완료 이전 이벤트에 구독하고
                    //초기화 이후면 그대로 실행
                    UnityEngine.SceneManagement.Scene loadScene = SceneManager.GetSceneByName(sceneName);
                    GameObject[] sceneAllitems = loadScene.GetRootGameObjects();
                    foreach (GameObject sceneObject in sceneAllitems)
                    {
                        if (sceneObject.TryGetComponent(out BaseScene baseScene))
                        {
                            if (baseScene.CheckDoneZenjectInitialize == false)
                            {
                                baseScene.OnZenjectInitializeAfterEvent += () =>
                                {
                                    actionToTemp?.Invoke(clientId);
                                };
                            }
                            else
                            {
                                actionToTemp?.Invoke(clientId);
                            }
                            break;
                        }
                    }
                    
                    if (_relayManager.NgoRPCCaller.LoadedPlayerCount == _relayManager.CurrentUserCount)
                    {
                        _relayManager.NgoRPCCaller.IsAllPlayerLoaded = true; //로딩창 90% 이후로 넘어가게끔
                        _onAllPlayerLoadedEvent?.Invoke();
                        _onClientLoadedEvent = null;
                        _relayManager.NetworkManagerEx.SceneManager.OnLoadComplete -= SceneManagerOnLoadCompleteAsync;
                    }
                    
                }
            }
        }

        private string GetSceneNameByEnumName(Define.Scene type)
        {
            string name = System.Enum.GetName(typeof(Define.Scene), type);
            return name;
        }
    }
}