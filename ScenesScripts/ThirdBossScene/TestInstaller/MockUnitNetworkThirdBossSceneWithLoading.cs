using System;
using System.Reflection;
using CustomEditor.Interfaces;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using NetWork.NGO.Scene_NGO;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.FirstBossScene;
using ScenesScripts.FirstBossScene.Installer;
using ScenesScripts.GamePlayScene;
using ScenesScripts.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace ScenesScripts.ThirdBossScene.TestInstaller
{
    public class MockUnitNetworkThirdBossSceneWithLoading : ISceneSpawnBehaviour
    {
        private static readonly FieldInfo TotalPlayerCountField =
            typeof(GamePlaySceneLoadingProgress).GetField("_totalPlayerCount", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly BaseScene _baseScene;
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly NgoPoolManager _poolManager;
        private readonly ISceneSelectCharacter _sceneSelectedCharacter;
        private readonly IMultiTestScene _multiTestScene;
        private readonly SignalBus _signalBus;
        private readonly IMockSceneLoadingSyncFactory _mockSceneLoadingSyncFactory;
        private readonly SpawnPosition _spawnPosition;
        private GamePlaySceneLoadingProgress _loadingProgress;
        private int _testPlayerCount;
        private bool _hasSpawnedMockLoadingSync;

        [Inject]
        public MockUnitNetworkThirdBossSceneWithLoading(
            RelayManager relayManager,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            NgoPoolManager poolManager,
            IMultiTestScene multiTestScene,
            SpawnPosition spawnPosition,
            SignalBus signalBus,
            IMockSceneLoadingSyncFactory mockSceneLoadingSyncFactory)
        {
            _baseScene = UnityEngine.Object.FindAnyObjectByType<BaseScene>();
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _poolManager = poolManager;
            _multiTestScene = multiTestScene;
            _sceneSelectedCharacter = _baseScene.GetComponent<ISceneSelectCharacter>();
            _spawnPosition = spawnPosition;
            _signalBus = signalBus;
            _mockSceneLoadingSyncFactory = mockSceneLoadingSyncFactory;
        }

        private NgoThirdBossSceneSpawn _ngoThirdBossSceneSpawn;
        public Define.PlayerClass GetPlayableCharacter => _sceneSelectedCharacter.GetPlayerableCharacter();

        public void Init()
        {
            if (_sceneSelectedCharacter == null)
            {
                throw new InvalidOperationException("sceneSelectedCharacter is null");
            }

            _testPlayerCount = Mathf.Max(1, _multiTestScene.GetMultiTestPlayers().Count);
            RegisterMyCharacter();
            EnsureLoadingUi();
            RegisterLoadingUiPlayerCount();
            EnsureMockLoadingSync();
        }

        private void RegisterMyCharacter()
        {
            if (_relayManager.NetworkManagerEx.IsConnectedClient)
            {
                RegisterAndSpawnPlayer(_relayManager.NetworkManagerEx.LocalClientId);
                return;
            }

            _relayManager.NetworkManagerEx.OnClientConnectedCallback -= ConnectClient;
            _relayManager.NetworkManagerEx.OnClientConnectedCallback += ConnectClient;
        }

        private void ConnectClient(ulong clientID)
        {
            if (clientID != _relayManager.NetworkManagerEx.LocalClientId)
            {
                return;
            }

            if (_relayManager.NgoRPCCaller == null)
            {
                Action<RpcCallerReadySignal> onSignal = null;

                onSignal = signal =>
                {
                    _signalBus.Unsubscribe<RpcCallerReadySignal>(onSignal);
                    RegisterAndSpawnPlayer(clientID);
                };
                _signalBus.Subscribe<RpcCallerReadySignal>(onSignal);
                return;
            }

            RegisterAndSpawnPlayer(clientID);
        }

        private void RegisterAndSpawnPlayer(ulong clientID)
        {
            _relayManager.RegisterSelectedCharacter(clientID, GetPlayableCharacter);
            _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientID, true, _spawnPosition);
        }

        private void EnsureLoadingUi()
        {
            UILoading uiLoading = _uiManagerServices.GetOrCreateSceneUI<UILoading>();
            _loadingProgress = _resourceService.GetOrAddComponent<GamePlaySceneLoadingProgress>(uiLoading.gameObject);
        }

        private void RegisterLoadingUiPlayerCount()
        {
            ApplyTestPlayerCountToLoadingProgress();

            if (_relayManager.NgoRPCCaller == null)
            {
                Action<RpcCallerReadySignal> onSignal = null;

                onSignal = signal =>
                {
                    _signalBus.Unsubscribe<RpcCallerReadySignal>(onSignal);
                    ApplyTestPlayerCountToLoadingProgress();
                };
                _signalBus.Subscribe<RpcCallerReadySignal>(onSignal);
                return;
            }

            ApplyTestPlayerCountToLoadingProgress();
        }

        private void ApplyTestPlayerCountToLoadingProgress()
        {
            if (_loadingProgress == null)
            {
                return;
            }

            TotalPlayerCountField?.SetValue(_loadingProgress, _testPlayerCount);
        }

        private void EnsureMockLoadingSync()
        {
            if (_relayManager.NetworkManagerEx.IsListening)
            {
                SpawnMockLoadingSyncOnHost();
                return;
            }

            _relayManager.NetworkManagerEx.OnServerStarted -= SpawnMockLoadingSyncOnHost;
            _relayManager.NetworkManagerEx.OnServerStarted += SpawnMockLoadingSyncOnHost;
        }

        private void SpawnMockLoadingSyncOnHost()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false || _hasSpawnedMockLoadingSync)
            {
                return;
            }

            _relayManager.NetworkManagerEx.OnServerStarted -= SpawnMockLoadingSyncOnHost;

            MockSceneLoadingSync loadingSync = _mockSceneLoadingSyncFactory.Create(_testPlayerCount);
            _relayManager.SpawnNetworkObj(loadingSync.gameObject, destroyOption: false);
            _hasSpawnedMockLoadingSync = true;
        }

        public void SpawnObj()
        {
            if (_relayManager.NetworkManagerEx.IsListening)
            {
                InitNgoPlaySceneOnHost();
            }

            _relayManager.NetworkManagerEx.OnServerStarted += InitNgoPlaySceneOnHost;

            void InitNgoPlaySceneOnHost()
            {
                if (_relayManager.NetworkManagerEx.IsHost)
                {
                    _ngoThirdBossSceneSpawn = _resourceService.InstantiateByKey("Prefabs/NGO/NgoThirdBossSceneSpawn")
                        .GetComponent<NgoThirdBossSceneSpawn>();
                    _relayManager.SpawnNetworkObj(_ngoThirdBossSceneSpawn.gameObject, _relayManager.NgoRoot.transform);
                }
            }

            _poolManager.Create_NGO_Pooling_Object();
        }
    }
}
