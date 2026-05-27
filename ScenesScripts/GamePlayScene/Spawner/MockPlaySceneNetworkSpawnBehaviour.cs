using System;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using NetWork.NGO.UI;

using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;
using Object = UnityEngine.Object;

namespace ScenesScripts.GamePlayScene.Spawner
{
    public class MockPlaySceneNetworkSpawnBehaviour : ISceneSpawnBehaviour,IInitializable,IDisposable
    {
        
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly NgoPoolManager _poolManager;
        private readonly ISceneSelectCharacter _sceneSelectedCharacter;
        private readonly ISceneMover _sceneMover;
        private readonly SignalBus _signalBus;
        
        [Inject]
        public MockPlaySceneNetworkSpawnBehaviour(
            RelayManager relayManager,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            ISceneMover sceneMover,
            NgoPoolManager poolManager,
            SignalBus signalBus)
        {
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _sceneMover = sceneMover;
            _poolManager = poolManager;
            _sceneSelectedCharacter =  Object.FindAnyObjectByType<BaseScene>()
                .GetComponent<ISceneSelectCharacter>();
            
            _signalBus = signalBus;
        }
        private NgoGamePlaySceneSpawn _ngoGamePlaySceneSpawn;
        private UIStageTimer _uiStageTimer;
        public Define.PlayerClass GetPlayableCharacter => _sceneSelectedCharacter.GetPlayerableCharacter();
        public void Initialize()
        {
            _signalBus.Subscribe<RpcCallerReadySignal>(RegisterAndSpawnPlayer);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<RpcCallerReadySignal>(RegisterAndSpawnPlayer);
            if (_relayManager != null && _relayManager.NetworkManagerEx != null)
            {
                _relayManager.NetworkManagerEx.OnServerStarted -= InitNgoPlaySceneOnHost;
            }
        }
        private void RegisterAndSpawnPlayer(RpcCallerReadySignal signal)
        {
            ulong networkID = _relayManager.NetworkManagerEx.LocalClientId;
            _relayManager.RegisterSelectedCharacter(networkID, GetPlayableCharacter); 
            signal.CallerInstance.GetPlayerChoiceCharacterRpc(networkID);
        }
        public void Init()
        {
            Debug.Assert(_sceneSelectedCharacter != null, "sceneSelectedCharacter is null");
            SetTimerEvent();
            LoadGamePlayScene();
            void SetTimerEvent()
            {
                _uiStageTimer = _uiManagerServices.GetOrCreateSceneUI<UIStageTimer>();
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
            _poolManager.Create_NGO_Pooling_Object();
        }
        
        private void InitNgoPlaySceneOnHost()
        {
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                _ngoGamePlaySceneSpawn = _resourceService.InstantiateByKey("Prefabs/NGO/NgoGamePlaySceneSpawn")
                    .GetComponent<NgoGamePlaySceneSpawn>();
                _relayManager.SpawnNetworkObj(_ngoGamePlaySceneSpawn.gameObject, _relayManager.NgoRoot.transform);
            }
        }
       
    }
}
