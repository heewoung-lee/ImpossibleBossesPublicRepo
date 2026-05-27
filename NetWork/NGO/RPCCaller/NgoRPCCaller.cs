
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Controller;
using Cysharp.Threading.Tasks;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.BufferManagement;
using GameManagers.GameManagerExManagement;
using GameManagers.ItemDataManagement;
using GameManagers.ItemDataManagement.Interface;
using GameManagers.LobbyManagement;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using GameManagers.VFXManagement;
using GameManagers.VivoxManagement;
using NetWork.BaseNGO;
using NetWork.Item;
using NetWork.NGO.Interface;
using NetWork.NGO.Scene_NGO;
using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using Stats.BaseStats;
using UI.Popup.PopupUI;
using UI.Scene;
using UI.Scene.Interface;
using UI.Scene.SceneUI;
using UI.SubItem;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;
using ZenjectContext.ProjectContextInstaller;

namespace NetWork.NGO
{
    public interface INetworkDeSpawner
    {
        public void DeSpawnByReferenceServerRpc(NetworkObjectReference ngoRef, RpcParams rpcParams = default);
    }

    public class NgoRPCCaller : NetworkBehaviour, IRegistrar<ISpawnController>, INetworkDeSpawner
    {
        public class NgoRPCCallerFactory : NgoZenjectFactory<NgoRPCCaller>
        {
            public NgoRPCCallerFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoRPCCaller");
            }
        }

        
        private IUIManagerServices _uiManagerServices;
        private IResourcesServices _resourcesServices;
        private IBufferManager _bufferManager;
        private IPlayerSpawnManager _gameManagerEx;
        private IItemDataManager _itemDataManager;
        private LootItemFactory _lootItemFactory;
        private LobbyManager _lobbyManager;
        private IVivoxSession _vivoxSession;
        private SceneManagerEx _sceneManagerEx;
        private RelayManager _relayManager;
        private NgoPoolManager _poolManager;
        private IVFXManagerServices _vfxManager;
        private LootItemManager _lootItemManager;
        private SignalBus _signalBus;

        private IRegistrar<INetworkDeSpawner> _networkDeSpawner;

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            IResourcesServices resourcesServices,
            IItemDataManager itemDataManager,
            LootItemFactory lootItemFactory,
            IBufferManager bufferManager,
            IPlayerSpawnManager gameManagerEx,
            LobbyManager lobbyManager,
            IVivoxSession vivoxSession,
            SceneManagerEx sceneManagerEx,
            RelayManager relayManager,
            NgoPoolManager poolManager,
            IVFXManagerServices vfxManager,
            LootItemManager lootItemManager,
            SignalBus signalBus)
        {
            _uiManagerServices = uiManagerServices;
            _resourcesServices = resourcesServices;
            _itemDataManager = itemDataManager;
            _lootItemFactory = lootItemFactory;
            _bufferManager = bufferManager;
            _gameManagerEx = gameManagerEx;
            _lobbyManager = lobbyManager;
            _vivoxSession = vivoxSession;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
            _poolManager = poolManager;
            _vfxManager = vfxManager;
            _lootItemManager = lootItemManager;
            _signalBus = signalBus;
        }

        [Inject]
        public void IDConstruct(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            IRegistrar<INetworkDeSpawner> networkDeSpawner)
        {
            _networkDeSpawner = networkDeSpawner;
        }


        private ISpawnController _spawnController;

        public const ulong Invalidobjectid = ulong.MaxValue; //타겟 오브젝트가 있고 없고를 가려내기 위한 상수

        private NetworkVariable<int> _loadedPlayerCount = new NetworkVariable<int>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        private NetworkVariable<bool> _isAllPlayerLoaded = new NetworkVariable<bool>
            (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public int LoadedPlayerCount
        {
            get { return _loadedPlayerCount.Value; }
            set
            {
                if (IsHost == false)
                    return;

                _loadedPlayerCount.Value = value;
            }
        }

        public bool IsAllPlayerLoaded
        {
            get { return _isAllPlayerLoaded.Value; }
            set
            {
                if (IsHost == false)
                    return;

                _isAllPlayerLoaded.Value = value;
            }
        }


        [Rpc(SendTo.Server)]
        public void GetPlayerChoiceCharacterRpc(ulong clientId, bool hasSpawnPosition = false,
            SpawnPosition spawnPosition = default, RpcParams rpcParams = default)
        {
            string choiceCharacterName = _relayManager.ChoicePlayerCharactersDict[clientId].ToString();
            Vector3 targetPosition = hasSpawnPosition
                ? spawnPosition.PlayerSpawnPosition + new Vector3(clientId, 0f, 0f)
                : new Vector3(clientId, 0f, 1f);

            _relayManager.SpawnNetworkObjInjectionOwner(clientId,
                $"Prefabs/Player/SpawnCharacter/{choiceCharacterName}Base",
                targetPosition, _relayManager.NgoRoot.transform, false);
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _relayManager.SetRPCCaller(gameObject);
            SpawnRpcCallerTools();

            _loadedPlayerCount.OnValueChanged += LoadedPlayerCountValueChanged;
            _isAllPlayerLoaded.OnValueChanged += IsAllPlayerLoadedValueChanged;

            _networkDeSpawner.Register(this);
            
            _signalBus.Fire(new RpcCallerReadySignal()
            {
                CallerInstance = this
            });
            
        }


        private void SpawnRpcCallerTools()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;

            _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoRPCSpawnController", transform);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _loadedPlayerCount.OnValueChanged -= LoadedPlayerCountValueChanged;
            _isAllPlayerLoaded.OnValueChanged -= IsAllPlayerLoadedValueChanged;
            _networkDeSpawner.Unregister(this);
        }


        public void IsAllPlayerLoadedValueChanged(bool previousValue, bool newValue)
        {
            SetisAllPlayerLoadedRpc(newValue);
        }

        private void LoadedPlayerCountValueChanged(int previousValue, int newValue)
        {
            //UtilDebug.Log($"이전값{previousValue} 이후값{newValue}");
            LoadedPlayerCountRpc();
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void DeSpawnByReferenceServerRpc(NetworkObjectReference ngoRef, RpcParams rpcParams = default)
        {
            if (ngoRef.TryGet(out NetworkObject ngo))
            {
                ngo.Despawn(true);
            }
        }


        [Rpc(SendTo.Server)]
        public void Spawn_Loot_ItemRpc(IteminfoStruct itemStruct, Vector3 dropPosition, bool destroyOption = true,
            NetworkObjectReference addLootItemBehaviour = default)
        {
            if (_itemDataManager.TryGetItemData(itemStruct.ItemNumber, out var data))
            {
                GameObject lootObj = _lootItemFactory.CreateLootItem(data, dropPosition);

                if (addLootItemBehaviour.Equals(default(NetworkObjectReference)) == false)
                {
                    if (addLootItemBehaviour.TryGet(out NetworkObject ngo))
                    {
                        ILootItemBehaviour lootItemBehaviour = ngo.GetComponent<ILootItemBehaviour>();
                        if (lootItemBehaviour is MonoBehaviour monoBehaviour)
                        {
                            _resourcesServices.GetOrAddComponent(monoBehaviour.GetType(), lootObj);
                        }
                    }
                }
                
                LootItem lootItem = lootObj.GetComponent<LootItem>();
                lootItem.SetPosition(dropPosition);
                lootItem.Initialize(data);
                _relayManager.SpawnNetworkObj(lootItem.gameObject, _lootItemManager.ItemRoot, dropPosition);
            }
            else
            {
                UtilDebug.LogWarning($"아이템 데이터를 찾을 수 없습니다. ID: {itemStruct.ItemNumber}");
            }
        }


        [Rpc(SendTo.Server)]
        public void SpawnPrefabNeedToInitializeRpc(string path,Vector3 position,Quaternion rotation)
        {
            NetworkObject networkObj = SpawnObjectToResources(path,position,rotation);
            NotifyPrefabSpawnedClientRpc(networkObj.NetworkObjectId);
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyPrefabSpawnedClientRpc(ulong networkObjectId)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
            {
                if (obj.TryGetComponent(out NgoInitializeBase ngoInitialize))
                {
                    ngoInitialize.SetInitialize(obj);
                }
            }
        }

        private NetworkObject SpawnVFXObjectToResources(string path, Vector3 position = default, Quaternion rotation = default,Vector3 scale = default)
        {
            if (rotation.Equals(default(Quaternion))) //(0,0,0,0)은 유효하지 않는 회전 일 수 있으므로 identity
                rotation = Quaternion.identity;
            
            
            if (_poolManager.PooledObjects.ContainsKey(path) ||
                _resourcesServices.Load<NgoPoolingInitializeBase>(path) != null)
            {
                //0908 수정 VFX가 풀 오브젝트이면 해당 함수를 실행하도록 했는데 문제는, 풀 오브젝트가 처음 생성될때 풀 오브젝트에 자신을 등록하도록
                //하는 동적방식을 채택한 이후로 _poolManager.PooledObjects.ContainsKey(path) 이부분이 fasle가 되는바람에 아랫부분이 실행됨.
                //그래서 처음에는 Load된 객체에 NgoPoolingInitializeBase이 있는 지를 처음만 체크 해두게 납둠.
                //이방식이 맘에 들진 않지만, 나중에 문제가 생기면 수정할 것 
                
                return SpawnObjectToResources(path, position,rotation,localScale:scale);
            }

            
            //4.28일 NGO_CALLER가 부모까지 지정하는건 책임소재에서 문제가 될 수 있어서 이부분은 각자 풀 오브젝트 초기화 부분에서 부모를 지정하도록 함
            return SpawnObjectToResources(path, position,rotation ,_vfxManager.VFXRootNgo,scale);
        }


        private NetworkObject SpawnObjectToResources(string path, Vector3 position, Quaternion rotation, 
            Transform parentTr = null,Vector3 localScale = default)
        {

            GameObject obj = _resourcesServices.InstantiateByKey(path);
            if (rotation == Quaternion.identity)
            {
                //1.8일 수정. 로테이션 값이 안들어가면 프리펩고유의 회전값을 수행하도록 설정
                rotation = obj.transform.rotation;
            }
            obj.transform.SetPositionAndRotation(position, rotation);
            if(localScale.Equals(default)) localScale = obj.transform.localScale;
            obj.transform.localScale = localScale;
            NetworkObject networkObj =
                _relayManager.SpawnNetworkObj(obj, parentTr, position).GetComponent<NetworkObject>();
            return networkObj;
        }
        
        /// <summary>
        /// 전에 쓰던 SpawnVFXPrefabServerRpc에 결함이 있었음.
        /// 호스트가 VFX를 생성하고 난 뒤 초기화를 진행할때
        /// 호스트는 당연히 스폰뒤에 초기화 메서드를 실행하지만
        /// 클라이언트는 네트워크 사정에 따라 스폰이 뒤에 될 수 있음
        /// 그래서 VFX의 초기화는 VFX오브젝트의 RPC를 만들어서 해당 RPC로 보내는걸로 수정
        /// 이렇게 하면 순서 상관없이 스폰 뒤에 함수를 붙여서 실행할 수 있다고 함
        /// </summary>
        /// <param name="path"></param>
        /// <param name="duration"></param>
        /// <param name="targerObjectID"></param>
        
        [Rpc(SendTo.Server)]
        public void SpawnVFXPrefabServerRpc(string path, float duration,bool isUnique ,ulong targerObjectID,Quaternion rotation,Vector3 localScale)
        {
            //파티클 중복을 막기위한 로직 얘를들어 버프 이펙트들이 중복으로 겹치지 않게 만든 로직 단, 여러발의 투사체가 필요한경우는 예외로 거름
            if (isUnique)
            {
                HashSet<NetworkObject> allSpawnedObjects = _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList;
                List<NetworkObject> vfxToDelete = new List<NetworkObject>();
                foreach (NetworkObject vfxNetWorkObj in allSpawnedObjects)
                {
                    if (vfxNetWorkObj.TryGetComponent(out NgoPoolingInitializeBase ngoPoolingInitialize))
                    {
                        if (ngoPoolingInitialize.PoolingNgoPath == path && 
                            ngoPoolingInitialize.TargetObjectId == targerObjectID)
                        {
                            vfxToDelete.Add(vfxNetWorkObj);
                        }
                    }
                }
                foreach (var targetVfx in vfxToDelete)
                {
                    if (targetVfx.IsSpawned)
                    {
                        _resourcesServices.DestroyObject(targetVfx.gameObject);
                    }
                }
            }
            
            // 2026-05-19: 클라이언트별 현재 위치 재계산을 막기 위해 서버가 VFX 시작 위치를 확정한다.
            Vector3 spawnPosition = Vector3.zero;
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(
                    targerObjectID,
                    out NetworkObject targetObject))
            {
                spawnPosition = targetObject.transform.position;
            }

            NetworkObject vfxObj = SpawnVFXObjectToResources(path,spawnPosition,rotation,localScale);
            
            //매니저가 RPC를 쏘는 게 아니라, 스폰된 객체의 컴포넌트를 가져와서 그 객체의 RPC를 호출
            if (vfxObj.TryGetComponent(out NgoPoolingInitializeBase vfxScript))
            {
                
                vfxScript.InitializeVfxClientRpc(targerObjectID, duration, spawnPosition);
            }
        }

        [Rpc(SendTo.Server)]
        public void SpawnVFXPrefabServerRpc(string path, float duration, Vector3 spawnPosition, Quaternion rotation,Vector3 localScale,NetworkParams networkParams)
        {
            NetworkObject vfxObj = SpawnVFXObjectToResources(path, spawnPosition,rotation,localScale);
    
            if (vfxObj.TryGetComponent(out NgoPoolingInitializeBase vfxScript))
            {
                vfxScript.InitializeVfxClientRpc(duration,networkParams);
            }
        }
        
        [Rpc(SendTo.Server)]
        public void Call_InitBuffer_ServerRpc(StatEffect effect, string buffIconImagePath, float duration,ulong targerObjectID)
        {
            Call_InitBuffer_ClientRpc(effect, buffIconImagePath,duration,targerObjectID);
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void Call_InitBuffer_ClientRpc(StatEffect effect ,string buffIconImagePath, float duration,ulong targerObjectID)
        {
            Debug.Assert(_gameManagerEx.GetPlayer() != null ,  "[Call_InitBuffer_ClientRpc] Player hasn't been registered" +
                                                               "Check the GameManagerEx.GetPlayer()");
            
            //1.3일 수정 원래 PlayerStats만 받았는데 몬스터 디버프를 걸어야 하는 상황이 생겨 BaseStats로 변경
            BaseStats baseStats = _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects[targerObjectID].GetComponent<BaseStats>();
            
            
            //1.5일 수정[중요] 1.3일에 몬스터를 디버프 걸어야 하는 상황을 고려해서 네트워크 스폰객체들로 판명을 했는데
            //문제는 호스트와 클라이언트 둘다 이로직을 수행하는 문제가 발생함. 따라서 타겟이 자기가 소유한
            //객체라면 자기가 버프를 관리하도록 수정함 몬스터는 대부분 호스트 소유이므로 문제없음
            if (baseStats.TryGetComponent(out NetworkObject networkObj) == true && networkObj.IsOwner == true)
            {
                if (networkObj.NetworkObjectId == targerObjectID)
                {
                    if (duration > 0)
                    {
                        _bufferManager.InitBuff(baseStats, duration, effect, buffIconImagePath);
                    }
                    else
                    {
                        _bufferManager.ImmediatelyBuffStart(baseStats, effect.statType, effect.value);
                    }
                }
            }
        }

        private async UniTask DisconnectFromVivoxAndLobby()
        {
            try
            {
                Lobby currentLobby = await _lobbyManager.GetCurrentLobby();

                if (currentLobby != null)
                {
                    await _lobbyManager.RemoveLobbyAsync(currentLobby);
                }
            }
            catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                UtilDebug.Log("로비가 이미 삭제되어 로비 정리를 건너뜁니다.");
            }
            catch (Exception e)
            {
                UtilDebug.LogError($"[Disconneted NetWorkError] Lobby cleanup error: {e}");
            }

            try
            {
                await _vivoxSession.LogoutOfVivoxAsync();
            }
            catch (Exception e)
            {
                UtilDebug.LogError($"[Disconneted NetWorkError] Vivox logout error: {e}");
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetGameStartLoadingRpc(bool isShow)
        {
            UILoadingProgress loadingProgress = _uiManagerServices.GetOrCreateSceneUI<UILoadingProgress>();

            if (isShow)
            {
                loadingProgress.ShowLoading("여정을 떠날 준비중", "플레이어들이 짐을 챙기고 있습니다.");
            }
            else
            {
                loadingProgress.HideLoading();
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void LoadedPlayerCountRpc()
        {
            if (_uiManagerServices.Try_Get_Scene_UI(out UILoading loading))
            {
                if (loading.TryGetComponent(out GamePlaySceneLoadingProgress loadingProgress))
                {
                    loadingProgress.SetLoadedPlayerCount(LoadedPlayerCount);
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetisAllPlayerLoadedRpc(bool isAllplayerLoaded)
        {
            if (_uiManagerServices.Try_Get_Scene_UI(out UILoading loading))
            {
                if (loading.TryGetComponent(out GamePlaySceneLoadingProgress loadingProgress))
                {
                    loadingProgress.SetisAllPlayerLoaded(isAllplayerLoaded);
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void SubmitSelectedCharactertoServerRpc(ulong clientId, string selectCharacterName)
        {
            //UtilDebug.Log($"{clientId} 유저가 {selectCharacterName}을 등록함");
            Define.PlayerClass selectCharacter =
                (Define.PlayerClass)Enum.Parse(typeof(Define.PlayerClass), selectCharacterName);
            _relayManager.RegisterSelectedCharacterInDict(clientId, selectCharacter);
        }

        public void SpawnLocalObject(Vector3 pos, string objectPath, NetworkParams networkParams)
        {
            FixedList32Bytes<Vector3> list = new FixedList32Bytes<Vector3>();
            list.Add(pos); // 한 개만 담기
            SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), networkParams);
        }

        public void SpawnNonNetworkObject(List<Vector3> pos, string objectPath, NetworkParams networkParams)
        {
            if (pos == null)
                return;

            int posCount = pos.Count;
            switch (posCount)
            {
                case <= 2:
                {
                    FixedList32Bytes<Vector3> list = new FixedList32Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), networkParams);
                    break;
                }
                case <= 5:
                {
                    FixedList64Bytes<Vector3> list = new FixedList64Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), networkParams);
                    break;
                }
                case <= 10:
                {
                    FixedList128Bytes<Vector3> list = new FixedList128Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), networkParams);
                    break;
                }
                case <= 42:
                {
                    FixedList512Bytes<Vector3> list = new FixedList512Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), networkParams);
                    break;
                }
                case <= 340:
                {
                    FixedList4096Bytes<Vector3> list = new FixedList4096Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), networkParams);
                    break;
                }
                default:
                    UtilDebug.LogError("Too many positions! Maximum supported is 340.");
                    break;
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList32Bytes<Vector3>> posList,
            FixedString512Bytes path, NetworkParams networkParams)
        {
            ProcessLocalSpawn(posList.Value, path, networkParams);
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList64Bytes<Vector3>> posList,
            FixedString512Bytes path, NetworkParams networkParams)
        {
            ProcessLocalSpawn(posList.Value, path, networkParams);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList128Bytes<Vector3>> posList,
            FixedString512Bytes path, NetworkParams networkParams)
        {
            ProcessLocalSpawn(posList.Value, path, networkParams);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList512Bytes<Vector3>> posList,
            FixedString512Bytes path, NetworkParams networkParams)
        {
            ProcessLocalSpawn(posList.Value, path, networkParams);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList4096Bytes<Vector3>> posList,
            FixedString512Bytes path, NetworkParams networkParams)
        {
            ProcessLocalSpawn(posList.Value, path, networkParams);
        }

        private void ProcessLocalSpawn<TList>(TList posList, FixedString512Bytes path, NetworkParams networkParams)
            where TList : struct, INativeList<Vector3>
        {
            string objectPath = path.ConvertToString();
            //GameObject spawnGo = _resourcesServices.Load<GameObject>(objectPath);
            //9.1일 수정함 기존에 Load로 호출하니 spawnBehaviour얘가 인젝션이 안돼서
            //spawnBehaviour안에 필요한 의존성을 못긁어옴
            //프리펩을 스폰한뒤 각자 ISpawnBehavior에 재정의한 로직에 맞게 수정
            TList fixedList = posList;
            for (int i = 0; i < fixedList.Length; i++)
            {
                GameObject spawnGo = _resourcesServices.InstantiateByKey(objectPath);
                if (spawnGo.TryGetComponent<ISpawnBehavior>(out var spawnBehaviour))
                {
                    NetworkParams spawnParams = new NetworkParams
                        (
                            argFloat:networkParams.ArgFloat,
                            argInteger:networkParams.ArgInt,
                            argString:networkParams.ArgString,
                            argPosVector3 : fixedList[i],
                            argBoolean:networkParams.ArgBoolean,
                            argUlong: networkParams.ArgUlong
                            ){};
                    spawnBehaviour.SpawnObjectToLocal(spawnParams, objectPath);
                }
                //여기는 인스턴스를 바로 생성하는게 맞음
                //인스턴스를 바로 생성 하고 ISPawnBehaviour에서 초기화단을 구성해줄껏
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void ResetManagersRpc()
        {
            //Managers.Clear();
            UtilDebug.Log("Call RPCCaller in ResetManagersRpc");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestCameraShakeRpc(float intensity, float duration)
        {
            float clampedIntensity = Mathf.Clamp01(intensity);
            float clampedDuration = Mathf.Max(0f, duration);
            if (clampedIntensity <= 0f || clampedDuration <= 0f)
            {
                return;
            }

            BroadcastCameraShakeRpc(clampedIntensity, clampedDuration);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void BroadcastCameraShakeRpc(float intensity, float duration)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            if (mainCamera.TryGetComponent(out CameraShaker shaker))
            {
                shaker.Shake(intensity, duration);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void StartThirdBossFrontCameraRpc(float uiEndingDelay)
        {
            //4.2 하이어라키에서 카메라 찾는방식이 맘에 안들지만,
            //기존에 카메라를 바인딩 하는걸 못씀. 왜냐하면 RPCCaller는 projectContext로 씬 보다 먼저 바인딩이 되기떄문
            GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
            for (int i = 0; i < mainCameras.Length; i++)
            {
                if (mainCameras[i].TryGetComponent(out PlayerFollowingCamera followingCamera))
                {
                    followingCamera.MoveCameraToPlayerFront();
                    break;
                }
            }

            GameObject localPlayer = _gameManagerEx.GetPlayer();
            if (localPlayer != null && localPlayer.TryGetComponent(out PlayerController playerController))
            {
                playerController.PlayVictory();
            }

            ShowUIEndingAfterDelayAsync(uiEndingDelay, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid ShowUIEndingAfterDelayAsync(float delaySeconds, CancellationToken cancellationToken)
        {
            try
            {
                int delayMilliseconds = Mathf.Max(0, Mathf.CeilToInt(delaySeconds * 1000f));
                if (delayMilliseconds > 0)
                {
                    await UniTask.Delay(delayMilliseconds, ignoreTimeScale: true, cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            UIEnding endingUI = _uiManagerServices.GetOrCreateSceneUI<UIEnding>();
            endingUI.gameObject.SetActive(true);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void OnBeforeSceneUnloadRpc()
        {
            //이부분 배열로 변경함. 기존에 네트워크 오브젝트 해쉬셋에서
            //디스폰을 시도하면 해쉬셋의 버전이 도중에 달라져서 에러를 내 뿜음
            var snapshot = _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList.ToArray();

            foreach (NetworkObject ngo in snapshot)
            {
                if (ngo == null)
                    continue;

                if (ngo.TryGetComponent(out ISceneChangeBehaviour behaviour))
                {
                    UtilDebug.Log((behaviour as Component).name + " 초기화 처리 됨");
                    behaviour.OnBeforeSceneUnload();
                }
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void OnBeforeSceneUnloadLocalRpc()
        {
            _sceneManagerEx.InvokeOnBeforeSceneUnloadLocalEvent();
            _sceneManagerEx.SetNormalBootMode(true); //만약 테스트모드에서 실행했다면 이후테스트는 노멀모드로 실행
            DisconnectFromVivoxAndLobby().Forget(); //비복스 및 로비 연결해제
        }


        public void Register(ISpawnController targetManager)
        {
            _spawnController = targetManager;
        }

        public void Unregister(ISpawnController sceneContext)
        {
            if (_spawnController != null)
            {
                _spawnController = null;
            }
        }

        public void SpawnControllerOption(NetworkObject ngo, Action spawnLogic)
        {
            if (_spawnController != null)
            {
                _spawnController.SpawnControllerOption(ngo, spawnLogic);
            }
            else
            {
                spawnLogic.Invoke();
            }
        }
    }
}
