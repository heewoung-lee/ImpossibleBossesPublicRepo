using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.NGO.UI;
using Scene.BattleScene.Spawner;
using Scene.CommonInstaller;
using Scene.GamePlayScene;
using Scene.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace Scene.BattleScene
{
    public class MockUnitNetworkBattleScene : ISceneSpawnBehaviour
    {
        private readonly RelayManager _relayManager;
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly NgoPoolManager _poolManager;
        private readonly ISceneSelectCharacter _sceneSelectedCharacter;
     
        
        [Inject]
        public MockUnitNetworkBattleScene(
            RelayManager relayManager,
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            NgoPoolManager poolManager)
        {
            _relayManager = relayManager;
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _poolManager = poolManager;
            _sceneSelectedCharacter =  Object.FindAnyObjectByType<BaseScene>()
                .GetComponent<ISceneSelectCharacter>();
        }
        private NgoBattleSceneSpawn _ngoGamePlaySceneSpawn;
        public Define.PlayerClass GetPlayableCharacter => _sceneSelectedCharacter.GetPlayerableCharacter();

        public void Init()
        {
            Debug.Assert(_sceneSelectedCharacter != null, "sceneSelectedCharacter is null");
            _relayManager.NetworkManagerEx.OnClientConnectedCallback -= ConnectClient;
            _relayManager.NetworkManagerEx.OnClientConnectedCallback += ConnectClient;
    
            if (_relayManager.NetworkManagerEx is not null)
            {
                ulong networkID = _relayManager.NetworkManagerEx.LocalClientId;
                _relayManager.RegisterSelectedCharacter(networkID, GetPlayableCharacter);

                //TODO: 테스트 도중에 RPCCALL가 없는 경우가 있었음. 이부분. 나중에 제거할지 말지 결정할것
                if (_relayManager.NgoRPCCaller == null)
                {
                    _relayManager.SpawnRpcCallerEvent += () =>
                    {
                        _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(networkID);
                    };
                }
                else
                {
                    _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(networkID);
                }
                LoadScene();
            }
        }
        private void ConnectClient(ulong clientID)
        {
            if (_relayManager.NgoRPCCaller == null)
            {
                _relayManager.SpawnRpcCallerEvent += SpawnPlayer;
            }
            else
            {
                SpawnPlayer();
            }
            void SpawnPlayer()
            {
                if (_relayManager.NetworkManagerEx.LocalClientId != clientID)
                    return;
                _relayManager.RegisterSelectedCharacter(clientID, GetPlayableCharacter);
                _relayManager.NgoRPCCaller.GetPlayerChoiceCharacterRpc(clientID);
                LoadScene();
            }
        }
        private void LoadScene()
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
