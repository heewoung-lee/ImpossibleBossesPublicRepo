using System;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Pool;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using NetWork.NGO.UI;
using Scene.BattleScene.Spawner;
using Scene.CommonInstaller;
using Scene.GamePlayScene;
using Scene.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;
using Object = UnityEngine.Object;

namespace Scene.BattleScene
{
    public class MockUnitNetworkBattleScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly NgoPoolManager _poolManager;
        private readonly ISceneSelectCharacter _sceneSelectedCharacter;
        private readonly SignalBus _signalBus;
        
        [Inject]
        public MockUnitNetworkBattleScene(
            RelayManager relayManager,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            NgoPoolManager poolManager,
            SignalBus signalBus)
        {
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _poolManager = poolManager;
            _sceneSelectedCharacter =  Object.FindAnyObjectByType<BaseScene>()
                .GetComponent<ISceneSelectCharacter>();
            _signalBus = signalBus;
        }
        private NgoBattleSceneSpawn _ngoGamePlaySceneSpawn;
        public Define.PlayerClass GetPlayableCharacter => _sceneSelectedCharacter.GetPlayerableCharacter();

        public void Init()
        {
            Debug.Assert(_sceneSelectedCharacter != null, "sceneSelectedCharacter is null");
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
            if (_relayManager.NgoRPCCaller == null)
            {
                Action<RpcCallerReadySignal> onSignal = null;

                onSignal = (signal) =>
                {
                    _signalBus.Unsubscribe<RpcCallerReadySignal>(onSignal);
                    RegisterAndSpawnPlayer(clientID);
                };
                _signalBus.Subscribe<RpcCallerReadySignal>(onSignal);
            }
            else
            {
                RegisterAndSpawnPlayer(clientID);
            }
        }

        private void RegisterAndSpawnPlayer(ulong clientID)
        {
            _relayManager.RegisterSelectedCharacter(clientID, GetPlayableCharacter); 
            _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientID);
        }
        private void LoadGamePlayScene()
        {
            _uiManagerServices.GetOrCreateSceneUI<UILoading>().gameObject.SetActive(false);
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
                    _ngoGamePlaySceneSpawn = _resourceService.InstantiateByKey("Prefabs/NGO/NgoBattleSceneSpawn")
                        .GetComponent<NgoBattleSceneSpawn>();
                    _relayManager.SpawnNetworkObj(_ngoGamePlaySceneSpawn.gameObject, _relayManager.NgoRoot.transform);
                }
            }
            _poolManager.Create_NGO_Pooling_Object();
        }
    }
}
