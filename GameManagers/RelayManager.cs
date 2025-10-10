using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameManagers.Interface.RelayManagerInterface;
using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using Scene.CommonInstaller;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers
{
    public class RelayManager : IInitializable
    {
                
        private readonly IResourcesServices _resourcesServices;
        private readonly SocketEventManager _socketEventManager;
        private readonly Dictionary<ulong, Define.PlayerClass> _choicePlayerCharactersDict;
        
        private ISpawnController _spawnController;
        
        
        [Inject]
        public RelayManager(
            IResourcesServices resourcesServices,
            SocketEventManager socketEventManager)
        {
            _resourcesServices = resourcesServices;
            _socketEventManager = socketEventManager;
            _choicePlayerCharactersDict = new Dictionary<ulong, Define.PlayerClass>();
        }

        private Action _spawnRpcCallerEvent;
        
        private NetworkManager _netWorkManager;
        private string _joinCode;
        private NgoRPCCaller _ngoRPCCaller;
        private NgoRPCSpawnController _ngoRPCSpawnController;
        private Allocation _allocation;
        private GameObject _nGoRootUI;
        private GameObject _nGoRoot;
        private Define.PlayerClass _choicePlayerCharacter;

        public event Action SpawnRpcCallerEvent
        {
            add=>  UniqueEventRegister.AddSingleEvent(ref _spawnRpcCallerEvent, value);
            remove => UniqueEventRegister.RemovedEvent(ref _spawnRpcCallerEvent, value);
        }
        
        public void Invoke_Spawn_RPCCaller_Event()
        {
            _spawnRpcCallerEvent?.Invoke();
            _spawnRpcCallerEvent = null;
        }


        public Define.PlayerClass ChoicePlayerCharacter => _choicePlayerCharacter;
        public Dictionary<ulong, Define.PlayerClass> ChoicePlayerCharactersDict => _choicePlayerCharactersDict;
        public int CurrentUserCount => _netWorkManager.ConnectedClientsList.Count;
        public void Initialize()
        {
            NetworkManagerInitialize(); //네트워크매니저 초기화 및 생성
            if (NetworkManagerEx.IsConnectedClient == false)
            {
                _netWorkManager.OnServerStarted += SpawnToRPC_Caller;
            }
            else
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
                    Debug.LogError("there is not Prefabs/NGO/NetworkManager");
                }
                UnityEngine.Object.Instantiate(networkPrefab);
                #region UpdateLog
                //6.28일 수정: 오브젝트가 생성될떄 부모값이 Null인결우 컨테이너를 통해 인젝션을 하면 컨테이너가 부모를 멋대로 넣음. 그래도 순서를 일반 생성 -> 컨테이너 주입으로 변경 
                //7.2일 수정: 어차피 NetworkManager는 inject이 필요없는 객체이므로 일반 스폰
                //7.3일 Debug.Log("it is NetworkManager" + Object.ReferenceEquals(instantiateOBj.GetComponent<NetworkManager>(),NetworkManager.Singleton)); == true로 확인
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
                    _nGoRoot = SpawnNetworkObj("Prefabs/NGO/NGO_ROOT");
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
            if (NgoRPCCaller == null)
            {
                SpawnRpcCallerEvent += SubmitToSeverSelectCharacter;
            }
            else
            {
                SubmitToSeverSelectCharacter();
            }

            void SubmitToSeverSelectCharacter()
            {
                NgoRPCCaller.SubmitSelectedCharactertoServerRpc(NetworkManagerEx.LocalClientId,
                    playerClass.ToString());
            }
            
            _choicePlayerCharacter = playerClass;
        }

        public void RegisterSelectedCharacterInDict(ulong clientId, Define.PlayerClass playerClass)
        {
            _choicePlayerCharactersDict[clientId] = playerClass;
        }


        public void SetRPCCaller(GameObject ngo)
        {
            _ngoRPCCaller = ngo.GetComponent<NgoRPCCaller>();
        }

        
        public void SpawnToRPC_Caller()
        {
            if (_netWorkManager.IsHost == false)
                return;

            if (NgoRPCCaller != null)
                return;

            _ngoRPCCaller = _resourcesServices.InstantiateByKey("Prefabs/NGO/NgoRPCCaller").GetComponent<NgoRPCCaller>();
            SpawnNetworkObj(_ngoRPCCaller.gameObject, destroyOption: false);
        }

        public async Task<string> StartHostWithRelay(int maxConnections)
        {
            try
            {
                if (NetworkManagerEx.IsHost && _joinCode != null)
                    return _joinCode;

                _allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                RelayServerData relaydata = AllocationUtils.ToRelayServerData(_allocation, "dtls");
                NetworkManagerEx.GetComponent<UnityTransport>().SetRelayServerData(relaydata);
                _joinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log($"호출 됐나요 릴레이코드: {_joinCode}");
                if (NetworkManagerEx.StartHost())
                {
                    return _joinCode;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public NetworkObjectReference GetNetworkObject(GameObject gameobject)
        {
            if (gameobject.TryGetComponent(out NetworkObject ngo))
            {
                return new NetworkObjectReference(ngo);
            }

            Debug.Log("GameObject hasn't a BaseStats");
            return default;
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
                        networkObj.transform.SetParent(parent, false);
                    }
                }
            }

            return instanceObj;
        }

        
        public void DeSpawn_NetWorkOBJ(ulong networkObjectID)
        {
            NgoRPCCaller.DeSpawnByIDServerRpc(networkObjectID);
        }

        public void DeSpawn_NetWorkOBJ(GameObject ngoGameobject)
        {
            NetworkObjectReference despawnNgo = GetNetworkObject(ngoGameobject);
            NgoRPCCaller.DeSpawnByReferenceServerRpc(despawnNgo);
        }

        public async Task<bool> IsValidRelayJoinCode(string joinCode)
        {
            try
            {
                // 유효한 경우 할당 객체를 받아옴
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                return true;
            }
            catch (RelayServiceException ex) when (ex.Message.Contains("join code not found"))
            {
                Debug.LogWarning($"RelayCode not Found: {joinCode}");
                return false;
            }
            catch (RelayServiceException ex) when (ex.ErrorCode == 404)
            {
                Debug.LogWarning("RelayCode hasnt Available");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex}");
                return false;
            }
        }

        public async Task<bool> JoinGuestRelay(string joinCode)
        {
            try
            {
                if (NetworkManagerEx.IsClient || NetworkManagerEx.IsServer)
                {
                    Debug.LogWarning("Client or Server is already running.");
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
                Debug.Log("소켓에러");
                return false;
            }
            catch (RelayServiceException ex) when (ex.Message.Contains("join code not found"))
            {
                Debug.LogWarning("로비에 릴레이코드가 유효하지 않음 새로 만들어야함");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public void ShutDownRelay()
        {
            NetworkManagerEx.Shutdown();
            _joinCode = null;
        }

        private Task DisConnectionRelay()
        {
            ShutDownRelay();
            return Task.CompletedTask;
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