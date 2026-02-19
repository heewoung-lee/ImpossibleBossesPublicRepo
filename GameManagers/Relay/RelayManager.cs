using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using NetWork.NGO;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers.RelayManager
{
    public interface IConnectionStrategy
    {
        public UniTask<string> StartHostAsync(NetworkManager networkManager,int maxConnections);
    }
    
    
    public class RelayManager : IInitializable
    {
        private readonly IResourcesServices _resourcesServices;
        private readonly SocketEventManager _socketEventManager;
        private readonly Dictionary<ulong, Define.PlayerClass> _choicePlayerCharactersDict;
        private readonly SignalBus _signalBus;
        private readonly IConnectionStrategy _connectionStrategy;
        
        private ISpawnController _spawnController;
    

        [Inject]
        public RelayManager(
            IResourcesServices resourcesServices,
            SocketEventManager socketEventManager,
            SignalBus signalBus,
            IConnectionStrategy connectionStrategy)
        {
            _resourcesServices = resourcesServices;
            _socketEventManager = socketEventManager;
            _choicePlayerCharactersDict = new Dictionary<ulong, Define.PlayerClass>();
            _signalBus = signalBus;
            _connectionStrategy = connectionStrategy;
        }

        private NetworkManager _netWorkManager;
        private string _joinCode;
        private NgoRPCCaller _ngoRPCCaller;
        private NgoRPCSpawnController _ngoRPCSpawnController;
        private Allocation _allocation;
        private GameObject _nGoRootUI;
        private GameObject _nGoRoot;
        private Define.PlayerClass _choicePlayerCharacter;


        public Define.PlayerClass ChoicePlayerCharacter => _choicePlayerCharacter;
        public Dictionary<ulong, Define.PlayerClass> ChoicePlayerCharactersDict => _choicePlayerCharactersDict;
        public int CurrentUserCount => _netWorkManager.ConnectedClientsList.Count;

        public void Initialize()
        {
            NetworkManagerInitialize(); //네트워크매니저 초기화 및 생성
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (NetworkManagerEx.IsConnectedClient == false)
            {
                _netWorkManager.OnServerStarted += SpawnToRPC_Caller;
            }
            else
            {
                SpawnToRPC_Caller();
            }
        }
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            // 호스트라면, 그리고 현재 씬에 RPC Caller가 없다면 생성 시도
            if (_netWorkManager != null && _netWorkManager.IsHost)
            {
                SpawnToRPC_Caller();
            }
        }
        private void NetworkManagerInitialize()
        {
            if (_netWorkManager == null)
            {
                GameObject networkPrefab = Resources.Load<GameObject>("Prefabs/NGO/NetworkManager");
                if (networkPrefab == null)
                {
                    UtilDebug.LogError("there is not Prefabs/NGO/NetworkManager");
                }

                UnityEngine.Object.Instantiate(networkPrefab);

                #region UpdateLog

                //6.28일 수정: 오브젝트가 생성될떄 부모값이 Null인결우 컨테이너를 통해 인젝션을 하면 컨테이너가 부모를 멋대로 넣음. 그래도 순서를 일반 생성 -> 컨테이너 주입으로 변경 
                //7.2일 수정: 어차피 NetworkManager는 inject이 필요없는 객체이므로 일반 스폰
                //7.3일 UtilDebug.Log("it is NetworkManager" + Object.ReferenceEquals(instantiateOBj.GetComponent<NetworkManager>(),NetworkManager.Singleton)); == true로 확인

                #endregion

                _netWorkManager = NetworkManager.Singleton;
                UnityEngine.Object.DontDestroyOnLoad(_netWorkManager.gameObject);
            }
        }

        public NetworkManager NetworkManagerEx => _netWorkManager;

        public GameObject NgoRootUI
        {
            get
            {
                if (_nGoRootUI == null && _netWorkManager.IsHost)
                {
                    _nGoRootUI = SpawnNetworkObj("Prefabs/NGO/NGO_ROOT_UI");
                }

                return _nGoRootUI;
            }
        }

        public GameObject NgoRoot
        {
            get
            {
                if (_nGoRoot == null && _netWorkManager.IsHost)
                {
                    _nGoRoot = SpawnNetworkObj("Prefabs/NGO/NGO_ROOT", destroyOption: false);
                }

                return _nGoRoot;
            }
        }

        public NgoRPCCaller NgoRPCCaller => _ngoRPCCaller;

        public string JoinCode
        {
            get => _joinCode;
        }

        public void RegisterSelectedCharacter(ulong clientId, Define.PlayerClass playerClass)
        {
            if (NgoRPCCaller == null || NgoRPCCaller.GetComponent<NetworkObject>().IsSpawned == false)
            {
                Action<RpcCallerReadySignal> onSignal = null;
                onSignal = (signal) =>
                {
                    _signalBus.Unsubscribe<RpcCallerReadySignal>(onSignal);
                    SubmitToSeverSelectCharacter(playerClass);
                };
                _signalBus.Subscribe<RpcCallerReadySignal>(onSignal);
            }
            else
            {
                SubmitToSeverSelectCharacter(playerClass);
            }
            _choicePlayerCharacter = playerClass;
        }

        private void SubmitToSeverSelectCharacter(Define.PlayerClass playerClass)
        {
            NgoRPCCaller.SubmitSelectedCharactertoServerRpc(NetworkManagerEx.LocalClientId,
                playerClass.ToString());

            UtilDebug.Log($"{NetworkManagerEx.LocalClientId}플레이어가 {playerClass.ToString()}를 선택했습니다 ");
        }
        
        public void RegisterSelectedCharacterInDict(ulong clientId, Define.PlayerClass playerClass)
        {
            _choicePlayerCharactersDict[clientId] = playerClass;
        }


        public void SetRPCCaller(GameObject ngo)
        {
            _ngoRPCCaller = ngo.GetComponent<NgoRPCCaller>();
        }


        private void SpawnToRPC_Caller()
        {
            if (_netWorkManager.IsHost == false)
                return;

            if (NgoRPCCaller != null)
                return;

            _ngoRPCCaller = _resourcesServices.InstantiateByKey("Prefabs/NGO/NgoRPCCaller")
                .GetComponent<NgoRPCCaller>();
            
            SpawnNetworkObj(_ngoRPCCaller.gameObject, destroyOption: false);
        }

        public async UniTask<string> StartHostWithRelay(int maxConnections)
        {
                if (NetworkManagerEx.IsHost && _joinCode != null)
                    return _joinCode;
                
                _joinCode = await _connectionStrategy.StartHostAsync(_netWorkManager, maxConnections);

                return _joinCode;
        }

        
        public NetworkObjectReference GetNetworkObject(GameObject gameobject)
        {
            if (gameobject == null)
                return default;

            if (gameobject.TryGetComponent(out NetworkObject ngo) == false)
                return default;

            if (ngo.IsSpawned == false)
                return default;
            
            //12.7일 수정 기존에는 TryGetComponent를 이용해서 안전하게 넘겼다고 생각했는데 씬이동하거나 종료될때 뜨는 에러가 있어서
            //안전하게 방어를 하기 위해 위같은 조건들을 걸어둠
            return new NetworkObjectReference(ngo);
        }


        public GameObject SpawnNetworkObj(string ngoPath, Transform parent = null,
            Vector3 position = default,
            bool destroyOption = true)
        {
            return SpawnNetworkObjInjectionOwner(NetworkManagerEx.LocalClientId, ngoPath, position,
                parent,
                destroyOption);
        }

        public GameObject SpawnNetworkObj(GameObject ngoInstance,
            Transform parent = null,
            Vector3 position = default, bool destroyOption = true)
        {
            return SpawnNetworkObjInjectionOwner(NetworkManagerEx.LocalClientId, ngoInstance, position,
                parent,
                destroyOption);
        }

        public GameObject SpawnNetworkObjInjectionOwner(ulong clientId, string ngoPath,
            Vector3 position = default,
            Transform parent = null, bool destroyOption = true)
        {
            GameObject loadObj = _resourcesServices.InstantiateByKey(ngoPath);
            return SpawnAndInjectionNgo(loadObj, clientId, position, parent, destroyOption);
        }

        public GameObject SpawnNetworkObjInjectionOwner(ulong clientId, GameObject ngo,
            Vector3 position = default,
            Transform parent = null, bool destroyOption = true)
        {
            return SpawnAndInjectionNgo(ngo, clientId, position, parent, destroyOption);
        }
        private GameObject SpawnAndInjectionNgo(GameObject instanceObj, ulong clientId,
            Vector3 position,
            Transform parent = null, bool destroyOption = true)
        {
            if (NetworkManagerEx.IsListening == true && NetworkManagerEx.IsHost)
            {
                instanceObj.transform.position = position;
                NetworkObject networkObj = _resourcesServices.GetOrAddComponent<NetworkObject>(instanceObj);

                if (_ngoRPCCaller != null)
                {
                    _ngoRPCCaller.SpawnControllerOption(networkObj, NgoDefaultSpawn);
                    //이쪽에 RPC의 스폰컨트롤이 들어가야함.
                }
                else
                {
                    NgoDefaultSpawn();
                }

                void NgoDefaultSpawn()
                {
                    if (networkObj.IsSpawned == false)
                    {
                        networkObj.SpawnWithOwnership(clientId, destroyOption);
                    }

                    if (parent != null)
                    {
                        networkObj.TrySetParent(parent, false);
                    }
                }
            }

            return instanceObj;
        }

        public async UniTask<bool> IsValidRelayJoinCode(string joinCode)
        {
            try
            {
                // 유효한 경우 할당 객체를 받아옴
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                return true;
            }
            catch (RelayServiceException ex) when (ex.Message.Contains("join code not found"))
            {
                UtilDebug.LogWarning($"RelayCode not Found: {joinCode}");
                return false;
            }
            catch (RelayServiceException ex) when (ex.ErrorCode == 404)
            {
                UtilDebug.LogWarning("RelayCode hasnt Available");
                return false;
            }
            catch (Exception ex)
            {
                UtilDebug.LogError($"Exception: {ex}");
                return false;
            }
        }

        public async UniTask<bool> JoinGuestRelay(string joinCode)
        {
            try
            {
                if (NetworkManagerEx.IsClient || NetworkManagerEx.IsServer)
                {
                    UtilDebug.LogWarning("Client or Server is already running.");
                    return false;
                }

                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData relaydata = AllocationUtils.ToRelayServerData(allocation, "dtls");
                NetworkManagerEx.GetComponent<UnityTransport>().SetRelayServerData(relaydata);
                _joinCode = joinCode;
                return !string.IsNullOrEmpty(joinCode) && NetworkManagerEx.StartClient();
            }
            catch (RelayServiceException ex) when (ex.ErrorCode == 404)
            {
                UtilDebug.Log("소켓에러");
                return false;
            }
            catch (RelayServiceException ex) when (ex.Message.Contains("join code not found"))
            {
                UtilDebug.LogWarning("로비에 릴레이코드가 유효하지 않음 새로 만들어야함");
                return false;
            }
            catch (Exception ex)
            {
                UtilDebug.LogError(ex);
                return false;
            }
        }

        public void ShutDownRelay()
        {
            NetworkManagerEx.Shutdown();
            _joinCode = null;
        }

        private UniTask DisConnectionRelay()
        {
            ShutDownRelay();
            return UniTask.CompletedTask;
        }
        public void JoinLocal()
        {
            UtilDebug.Log("[RelayManager] 로컬 모드로 접속을 시도합니다...");

            // 1. NetworkManager에서 UnityTransport 컴포넌트 가져오기
            var transport = _netWorkManager.GetComponent<UnityTransport>();

            // 2. [중요] 혹시 남아있을지 모르는 Relay 서버 데이터를 초기화
            // 이걸 안 하면 로컬 IP를 넣어도 릴레이 프로토콜을 타려고 해서 접속 실패할 수 있음
            transport.SetRelayServerData(default);

            // 3. 접속 주소를 로컬호스트(127.0.0.1)와 기본 포트(7777)로 설정
            // 만약 Host 쪽에서 포트를 바꿨다면 이 숫자도 맞춰줘야 함
            transport.SetConnectionData("127.0.0.1", 7777);

            // 4. 클라이언트 시작
            if (_netWorkManager.StartClient())
            {
                UtilDebug.Log("[RelayManager] Local Client 시작 성공 (Target: 127.0.0.1:7777)");
            }
            else
            {
                UtilDebug.LogError("[RelayManager] Local Client 시작 실패");
            }
        }

        public void SceneLoadInitializeRelayServer()
        {
            //NetworkManagerEx.NetworkConfig.EnableSceneManagement = false;
            //4.21일 주석처리함 과거의 내가 왜 이부분을 넣었는지 이해 안감.
            _socketEventManager.DisconnectRelayEvent += DisConnectionRelay;
        }

        #region 테스트용 함수

        public void SetPlayerClassforMockUnitTest(Define.PlayerClass playerClass)
        {
            _choicePlayerCharacter = playerClass;
        }

        #endregion

        public void Register(ISpawnController sceneContext)
        {
            _spawnController = sceneContext;
        }

        public void Unregister(ISpawnController sceneContext)
        {
            if (_spawnController != null)
            {
                _spawnController = null;
            }
        }
    }
}