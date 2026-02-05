using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using DataType.Skill;
using DataType.Skill.Factory;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.SkillManager;
using GameManagers.ResourcesEx;
using GameManagers.Scene;
using Player;
using Scene;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Skill;
using Stats;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace Module.PlayerModule.PlayerClassModule
{
    public abstract class ModulePlayerClass : NetworkBehaviour
    {
        private IResourcesServices _resourcesServices;
        private SceneManagerEx _sceneManagerEx;
        private ISkillManager _skillManager;
        private IPlayerSpawnManager _playerSpawnManager;
        private IRuntimeSkillFactory _runtimeSkillFactory;
        private IUIManagerServices _uiManagerServices;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            SceneManagerEx sceneManagerEx,
            IRuntimeSkillFactory strategyFactory,
            IPlayerSpawnManager playerSpawnManager,
            ISkillManager skillManager,
            IUIManagerServices uiManagerServices,
            SignalBus signalBus
        )
        {
            _resourcesServices = resourcesServices;
            _sceneManagerEx = sceneManagerEx;
            _runtimeSkillFactory = strategyFactory;
            _playerSpawnManager = playerSpawnManager;
            _skillManager = skillManager;
            _uiManagerServices = uiManagerServices;
            _signalBus = signalBus;
        }


        private BaseController _controller;
        private Animator _animator;
        private PlayerStats _playerStats;
        private CommonSkillState _commonSkillState;
        private Dictionary<string, float> _lazyAnimLengthCache;
        private UISkillBar _uiSkillBar;

        public BaseController Controller => _controller;
        public Animator Animator => _animator;
        public PlayerStats Stats => _playerStats;
        public CommonSkillState CommonSkillState => _commonSkillState;

        public abstract Define.PlayerClass PlayerClass { get; }
        private Dictionary<string, RuntimeSkill> _playerSkill;
        private string _initializedSceneName = null;
        private Dictionary<int, IUnitStat> _cachedStatTable;

        public Dictionary<int, IUnitStat> GetStatTable()
        {
            if (_cachedStatTable == null)
            {
                Debug.LogError(
                    $"[{gameObject.name}] StatTable is empty! You must call the InitializeStatTable method in your module");
            }

            return _cachedStatTable;
        }

        protected void InitializeStatTable<T>(Dictionary<int, T> originData) where T : IUnitStat
        {
            if (originData == null)
            {
                Debug.LogError($"[{GetType().Name}] Origin Data is Null.");
                return;
            }

            // 공통 변환 로직
            _cachedStatTable = originData.ToDictionary(k => k.Key, v => (IUnitStat)v.Value);
        }

        protected virtual void InitOnDisable()
        {
        }

        protected virtual void InitOnAwake()
        {
        }

        protected virtual void InitOnStart()
        {
        }

        private void Start()
        {
            InitOnStart();
            if (TryGetComponent(out BaseController controller))
            {
                InitializeController(controller);
            }
            else
            {
                _playerSpawnManager.OnPlayerSpawnEvent += OnPlayerSpawned;
            }
        }

        private void OnPlayerSpawned(PlayerStats playerStats)
        {
            _playerSpawnManager.OnPlayerSpawnEvent -= OnPlayerSpawned;
            BaseController controller = playerStats.GetComponent<BaseController>();
            InitializeController(controller);
        }


        private void InitializeController(BaseController controller)
        {
            _controller = controller;

            if (_commonSkillState == null)
            {
                _commonSkillState = new CommonSkillState(_controller);
            }

            if (_controller is PlayerController playerController)
            {
                playerController.StateAnimDict.RegisterState(_commonSkillState,
                    () =>
                    {
                        playerController.RunAnimation(_commonSkillState.CurrentAnimHash,
                            _commonSkillState.CurrentTransitionDuration);
                    });
            }

            OnControllerInitialized();
        }

        protected virtual void OnControllerInitialized()
        {
        }

        private void Awake()
        {
            _playerSkill = new Dictionary<string, RuntimeSkill>();
            _animator = GetComponentInChildren<Animator>();
            _playerStats = GetComponent<PlayerStats>();
            _lazyAnimLengthCache = new Dictionary<string, float>();
            InitOnAwake();
        }

        private void OnDisable()
        {
            InitOnDisable();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _signalBus.TryUnsubscribe<RuntimeSkillFactoryReadySignal>(InitializeSkillsFromManager);
            _signalBus.TryUnsubscribe<UISkillBarReadySignal>(AssignSkillsToUISlots);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (_sceneManagerEx.GetCurrentScene is ISkillInit)
            {
                
                if (_runtimeSkillFactory.CheckInitDone) //팩토리의 초기화 완료시점이 더 빠르면 바로 실행
                {
                    InitializeSkillsFromManager();
                }
                else //아니면 이벤트 시그널에 구독
                {
                    _signalBus.TryUnsubscribe<RuntimeSkillFactoryReadySignal>(InitializeSkillsFromManager);
                    _signalBus.Subscribe<RuntimeSkillFactoryReadySignal>(InitializeSkillsFromManager);
                }
            }

            if (IsOwner == true)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (_sceneManagerEx.GetCurrentScene is not ISkillInit)
                return;


            if (_runtimeSkillFactory.CheckInitDone)
            {
                InitializeSkillsFromManager();
            }
            else
            {
                _signalBus.TryUnsubscribe<RuntimeSkillFactoryReadySignal>(InitializeSkillsFromManager);
                _signalBus.Subscribe<RuntimeSkillFactoryReadySignal>(InitializeSkillsFromManager);
            }
        }

        #region 12.3 일 변경 넷워크 로드-> 로컬 로드로 변경

        // 이전방식은 씬 로드가 완료되면 스킬을 초기화 하는것이었으나. 생각해보니 로컬에서만 초기화 해도 된다고 판단해서 로컬에서 초기화 시킴
        //해당 내용으로 수정하니 버그도 없어지고 편함.
        // private void ChangeLoadScene(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
        //     List<ulong> clientsTimedOut)
        // {
        //     if (_sceneManagerEx.GetCurrentScene is not ISkillInit)
        //         return;
        //
        //     if (clientsCompleted.Contains(_relayManager.NetworkManagerEx.LocalClientId) is false)
        //         return;
        //     
        //     InitializeSkillsFromManager();
        // }

        #endregion

        public bool TryGetCachedLength(string stateName, out float animlen)
        {
            animlen = -1;
            if (_lazyAnimLengthCache.TryGetValue(stateName, out float length))
            {
                animlen = length;
                return true;
            }

            return false;
        }

        //캐시 저장 함수
        public void CacheLength(string stateName, float length)
        {
            if (_lazyAnimLengthCache.ContainsKey(stateName) == false)
            {
                _lazyAnimLengthCache.Add(stateName, length);
            }
        }

        private void InitializeSkillsFromManager()
        {
            string currentSceneName = _sceneManagerEx.CurrentScene.ToString();
            if (_initializedSceneName == currentSceneName)
                return;

            _initializedSceneName = currentSceneName;

            if (IsOwner == false) return;
            List<SkillDataSO> skillDatas = _skillManager.GetSkillDataList(PlayerClass);
            BaseController controller = GetComponent<BaseController>();
            _playerSkill.Clear();
            foreach (var data in skillDatas)
            {
                //공장을 통해 스킬 생성 (데이터 + 주인 주입)
                RuntimeSkill newSkill = _runtimeSkillFactory.CreateSkill(data, controller);
                if (newSkill != null)
                {
                    _playerSkill.Add(data.dataName, newSkill);
                }
            }
            if(_uiManagerServices.Try_Get_Scene_UI(out UISkillBar skillbar) == true)
            {
                _uiSkillBar = skillbar;
            }
            _uiSkillBar = GetUISkillBar();
            if (_uiSkillBar != null)
            {
                AssignSkillsToUISlots();
            }
            else
            {
                _signalBus.TryUnsubscribe<UISkillBarReadySignal>(AssignSkillsToUISlots);
                _signalBus.Subscribe<UISkillBarReadySignal>(AssignSkillsToUISlots);
            }
        }
        
        


        private UISkillBar GetUISkillBar()
        {
            if (_uiSkillBar == null)
            {
                if(_uiManagerServices.Try_Get_Scene_UI(out UISkillBar skillbar))
                {
                    _uiSkillBar = skillbar;
                }
            }
            return _uiSkillBar;
        }

        
        
        public void AssignSkillsToUISlots()
        {
          
            foreach (RuntimeSkill skill in _playerSkill.Values)
            {
                GameObject skillPrefab = _resourcesServices.InstantiateByKey("Prefabs/UI/Skill/UI_SkillComponent");
                SkillComponent skillcomponent = _resourcesServices.GetOrAddComponent<SkillComponent>(skillPrefab);

                skillcomponent.SetSkillComponent(skill);

                Transform skillLocation =  GetUISkillBar().SetLocationSkillSlot(skillcomponent);
                skillcomponent.AttachItemToSlot(skillcomponent.gameObject, skillLocation);
            }
        }
    }
}