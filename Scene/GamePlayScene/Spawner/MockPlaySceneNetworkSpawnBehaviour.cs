using CustomEditor;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using NetWork.NGO.Interface;
using NetWork.NGO.UI;
using Scene.BattleScene;
using Scene.CommonInstaller;
using Scene.GamePlayScene.Spawner;
using Scene.GamePlayScene.Spwaner;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.GamePlayScene
{
    public class MockPlaySceneNetworkSpawnBehaviour : ISceneSpawnBehaviour
    {
        
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly NgoPoolManager _poolManager;
        private readonly ISceneSelectCharacter _sceneSelectedCharacter;
        private readonly ISceneMover _sceneMover;
     
        
        [Inject]
        public MockPlaySceneNetworkSpawnBehaviour(
            RelayManager relayManager,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            ISceneMover sceneMover,
            NgoPoolManager poolManager)
        {
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _sceneMover = sceneMover;
            _poolManager = poolManager;
            _sceneSelectedCharacter =  Object.FindAnyObjectByType<BaseScene>()
                .GetComponent<ISceneSelectCharacter>();
        }
        private NgoGamePlaySceneSpawn _ngoGamePlaySceneSpawn;
        private UIStageTimer _uiStageTimer;
        public Define.PlayerClass GetPlayableCharacter => _sceneSelectedCharacter.GetPlayerableCharacter();

        public void Init()
        {
            Debug.Assert(_sceneSelectedCharacter != null, "sceneSelectedCharacter is null");
            RegisterMyCharacter();
            SetTimerEvent();
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
            void SetTimerEvent()
            {
                _uiStageTimer = _uiManagerServices.GetOrCreateSceneUI<UIStageTimer>();
                _uiStageTimer.OnTimerCompleted += _sceneMover.MoveScene;
            }
        }
        private void ConnectClient(ulong clientID)
        {
            if (_relayManager.NgoRPCCaller == null)
            {
                _relayManager.SpawnRpcCallerEvent += () => {RegisterAndSpawnPlayer(clientID); };
            }
            else
            {
                RegisterAndSpawnPlayer(clientID);
            }
        }

        private void RegisterAndSpawnPlayer(ulong clientID)
        {
            _relayManager.RegisterSelectedCharacter(clientID, GetPlayableCharacter); 
            if (_relayManager.NgoRPCCaller == null)
            {
                _relayManager.SpawnRpcCallerEvent += () =>
                {
                    _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientID);
                };
            }
            else
            {
                _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientID);
            }
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
                    _ngoGamePlaySceneSpawn = _resourceService.InstantiateByKey("Prefabs/NGO/NgoGamePlaySceneSpawn")
                        .GetComponent<NgoGamePlaySceneSpawn>();
                    _relayManager.SpawnNetworkObj(_ngoGamePlaySceneSpawn.gameObject, _relayManager.NgoRoot.transform);
                }
            }
            _poolManager.Create_NGO_Pooling_Object();
        }
    }
}
