using System;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using Module.EnemyModule;
using ScenesScripts.CommonInstaller;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene.Spawner;
using NetWork.NGO.Scene_NGO;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;
using Object = UnityEngine.Object;

namespace ScenesScripts.ThirdBossScene.TestInstaller
{
    public class MockUnitNetworkThirdBossScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly NgoPoolManager _poolManager;
        private readonly ISceneSelectCharacter _sceneSelectedCharacter;
        private readonly SignalBus _signalBus;
        private readonly SpawnPosition _spawnPosition;
        private NgoThirdBossSceneSpawn _ngoGamePlaySceneSpawn;

        [Inject]
        public MockUnitNetworkThirdBossScene(
            RelayManager relayManager,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            NgoPoolManager poolManager,
            SpawnPosition spawnPosition,
            SignalBus signalBus)
        {
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _poolManager = poolManager;
            _sceneSelectedCharacter =  Object.FindAnyObjectByType<BaseScene>()
                .GetComponent<ISceneSelectCharacter>();
            _spawnPosition = spawnPosition;
            _signalBus = signalBus;
        }

        public Define.PlayerClass GetPlayableCharacter => _sceneSelectedCharacter.GetPlayerableCharacter();

        public void Init()
        {
            Debug.Assert(_sceneSelectedCharacter != null, "sceneSelectedCharacter is null");
            DisableOpeningCinematicCoordinator();
            RegisterImmediateBossOpeningComplete();
            RegisterMyCharacter();
            LoadGamePlayScene();


            void RegisterMyCharacter()
            {
                if (_relayManager.NetworkManagerEx.IsConnectedClient == true) //네트워크를 확인했을때 내가 네트워크에 할당되었는지 확인.
                {
                    ulong networkID = _relayManager.NetworkManagerEx.LocalClientId;
                    RegisterAndSpawnPlayer(networkID);
                }
                else
                {
                    _relayManager.NetworkManagerEx.OnClientConnectedCallback -= ConnectClient;
                    _relayManager.NetworkManagerEx.OnClientConnectedCallback += ConnectClient;
                }
            }
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

                onSignal = (signal) =>
                {
                    _signalBus.Unsubscribe(onSignal);
                    RegisterAndSpawnPlayer(clientID);
                };
                _signalBus.Subscribe(onSignal);
            }
            else
            {
                RegisterAndSpawnPlayer(clientID);
            }
        }

        private void RegisterAndSpawnPlayer(ulong clientId)
        {
            _relayManager.RegisterSelectedCharacter(clientId, GetPlayableCharacter);
            _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientId, true, _spawnPosition);
        }
        private void LoadGamePlayScene()
        {
            _uiManagerServices.GetOrCreateSceneUI<UILoading>().gameObject.SetActive(false);
        }

        private void DisableOpeningCinematicCoordinator()
        {
            BossSceneOpeningCinematicCoordinator cinematicCoordinator =
                Object.FindAnyObjectByType<BossSceneOpeningCinematicCoordinator>();

            if (cinematicCoordinator != null)
            {
                cinematicCoordinator.enabled = false;
            }
        }

        private void RegisterImmediateBossOpeningComplete()
        {
            _signalBus.TryUnsubscribe<BossAnimationNetworkReadySignal>(HandleBossSpawnedWithoutCinematic);
            _signalBus.Subscribe<BossAnimationNetworkReadySignal>(HandleBossSpawnedWithoutCinematic);
        }

        private void HandleBossSpawnedWithoutCinematic(BossAnimationNetworkReadySignal signal)
        {
            _signalBus.TryUnsubscribe<BossAnimationNetworkReadySignal>(HandleBossSpawnedWithoutCinematic);

            GameObject bossMonster = signal.BossMonster;
            bossMonster.GetComponent<ModuleBossHpUI>().ShowBossHpUI();
            bossMonster.GetComponent<IBossSpawnCinematicTarget>().OnCombatStart();
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
                    _poolManager.Create_NGO_Pooling_Object();
                    
                    _ngoGamePlaySceneSpawn = _resourceService.InstantiateByKey("Prefabs/NGO/NgoThirdBossSceneSpawn")
                        .GetComponent<NgoThirdBossSceneSpawn>();
                    
                    _relayManager.SpawnNetworkObj(_ngoGamePlaySceneSpawn.gameObject, _relayManager.NgoRoot.transform);
                    
                    //세번째 보스 스폰 로직
                }
            }
        }
    }
}
