using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buffer;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ItemDataManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.Interface.VivoxManager;
using NetWork.BaseNGO;
using NetWork.NGO.Interface;
using NUnit.Framework;
using Scene.CommonInstaller;
using Scene.GamePlayScene;
using Stats;
using UI.Scene.Interface;
using UI.Scene.SceneUI;
using UI.SubItem;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;
using LootItem = NetWork.LootItem.LootItem;

namespace NetWork.NGO
{
    public class NgoRPCCaller : NetworkBehaviour,IRegistrar<ISpawnController>
    {
        public class NgoRPCCallerFactory : NgoZenjectFactory<NgoRPCCaller>
        {
            public NgoRPCCallerFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NgoRPCCaller");
            }
        }

        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IResourcesServices _resourcesServices;
        [Inject] private IItemGetter _itemGetter;
        [Inject] private ILootItemGetter _lootItemGetter;
        [Inject] private IBufferManager _bufferManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private IVivoxSession _vivoxSession;
        [Inject] private SceneManagerEx _sceneManagerEx;
        [Inject] private RelayManager _relayManager;
        [Inject] private NgoPoolManager _poolManager;
        [Inject] private IVFXManagerServices _vfxManager;
        [Inject] private LootItemManager _lootItemManager;
        [Inject] private ICoroutineRunner _coroutineRunner;
        
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

        NetworkManager _networkManager;

        NetworkManager RelayNetworkManager
        {
            get
            {
                if (_networkManager == null)
                {
                    _networkManager = _relayManager.NetworkManagerEx;
                }

                return _networkManager;
            }
        }

        [Rpc(SendTo.Server)]
        public void GetPlayerChoiceCharacterRpc(ulong clientId,RpcParams rpcParams= default)
        {
 
            string choiceCharacterName = _relayManager.ChoicePlayerCharactersDict[clientId].ToString();
            Vector3 targetPosition = new Vector3(1 * clientId, 0, 1);
            
            _relayManager.SpawnNetworkObjInjectionOwner(clientId,
                $"Prefabs/Player/SpawnCharacter/{choiceCharacterName}Base",
                targetPosition, _relayManager.NgoRoot.transform, false);
        }
         

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _relayManager.SetRPCCaller(gameObject);
            _relayManager.Invoke_Spawn_RPCCaller_Event();

            SpawnRpcCallerTools();
            
            _loadedPlayerCount.OnValueChanged += LoadedPlayerCountValueChanged;
            _isAllPlayerLoaded.OnValueChanged += IsAllPlayerLoadedValueChanged;
        }


        private void SpawnRpcCallerTools()
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
                return;
                
            _relayManager.SpawnNetworkObj("Prefabs/NGO/NgoRPCSpawnController",transform);
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _loadedPlayerCount.OnValueChanged -= LoadedPlayerCountValueChanged;
            _isAllPlayerLoaded.OnValueChanged -= IsAllPlayerLoadedValueChanged;
        }


        public void IsAllPlayerLoadedValueChanged(bool previousValue, bool newValue)
        {
            SetisAllPlayerLoadedRpc(newValue);
        }

        private void LoadedPlayerCountValueChanged(int previousValue, int newValue)
        {
            //Debug.Log($"이전값{previousValue} 이후값{newValue}");
            LoadedPlayerCountRpc();
        }

        [Rpc(SendTo.Server)]
        public void DeSpawnByIDServerRpc(ulong networkID, RpcParams rpcParams = default)
        {
            RelayNetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkID, out NetworkObject ngo);
            ngo.Despawn(true);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
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
            //여기에서 itemStruct를 IItem으로 변환
            GameObject networkLootItem = null;
            IItem iteminfo = _itemGetter.GetItemByItemNumber(itemStruct.ItemNumber);
            switch (itemStruct.ItemType)
            {
                case ItemType.Equipment:
                    networkLootItem = _lootItemGetter.GetEquipLootItem(iteminfo);
                    break;
                case ItemType.Consumable:
                    networkLootItem = _lootItemGetter.GetConsumableLootItem(iteminfo);
                    break;
                case ItemType.ETC:
                    break;
            }

            //여기에서는 어떤 아이템을 스폰할껀지 아이템의 형상만 가져올 것.
            LootItem.LootItem lootitem = networkLootItem.GetComponent<LootItem.LootItem>();
            lootitem.SetPosition(dropPosition);
            _relayManager.SpawnNetworkObj(lootitem.gameObject, _lootItemManager.ItemRoot, dropPosition);
            lootitem.SetItemInfoStructRpc(itemStruct);
        }
        

        [Rpc(SendTo.Server)]
        public void SpawnPrefabNeedToInitializeRpc(string path)
        {
            NetworkObject networkObj = SpawnObjectToResources(path);
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

        private NetworkObject SpawnVFXObjectToResources(string path, Vector3 position = default)
        {
            if (_poolManager.PooledObjects.ContainsKey(path) || _resourcesServices.Load<NgoPoolingInitializeBase>(path) != null)
            {
                //0908 수정 VFX가 풀 오브젝트이면 해당 함수를 실행하도록 했는데 문제는, 풀 오브젝트가 처음 생성될때 풀 오브젝트에 자신을 등록하도록
                //하는 동적방식을 채택한 이후로 _poolManager.PooledObjects.ContainsKey(path) 이부분이 fasle가 되는바람에 아랫부분이 실행됨.
                //그래서 처음에는 Load된 객체에 NgoPoolingInitializeBase이 있는 지를 처음만 체크 해두게 납둠.
                //이방식이 맘에 들진 않지만, 나중에 문제가 생기면 수정할 것 
                return SpawnObjectToResources(path, position);
            }
            
            //4.28일 NGO_CALLER가 부모까지 지정하는건 책임소재에서 문제가 될 수 있어서 이부분은 각자 풀 오브젝트 초기화 부분에서 부모를 지정하도록 함
            return SpawnObjectToResources(path, position, _vfxManager.VFXRootNgo);
            
        }


        private NetworkObject SpawnObjectToResources(string path, Vector3 position = default, Transform parentTr = null)
        {
            GameObject obj = _resourcesServices.InstantiateByKey(path);
            obj.transform.position = position;
            NetworkObject networkObj = _relayManager.SpawnNetworkObj(obj, parentTr, position).GetComponent<NetworkObject>();
            return networkObj;
        }

        [Rpc(SendTo.Server)]
        public void SpawnVFXPrefabServerRpc(string path, float duration, ulong targerObjectID = Invalidobjectid)
        {
            Vector3 pariclePos = Vector3.zero;
            Assert.AreEqual(targerObjectID != Invalidobjectid, true, $"targerObject is not spawn");
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(targerObjectID,
                    out NetworkObject targetNgo)) //쫒아가는 타겟 오브젝트 꺼네기
            {
                pariclePos = targetNgo.transform.position;
            }

            NetworkObject vfxObj = SpawnVFXObjectToResources(path, position: pariclePos);
            SpawnVFXPrefabClientRpc(vfxObj.NetworkObjectId, path, duration, targerObjectID);
        }

        [Rpc(SendTo.Server)]
        public void SpawnVFXPrefabServerRpc(string path, float duration, Vector3 spawnPosition = default)
        {
            Vector3 pariclePos = spawnPosition;
            NetworkObject vfxObj = SpawnVFXObjectToResources(path, position: pariclePos);
            SpawnVFXPrefabClientRpc(vfxObj.NetworkObjectId, path, duration);
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void SpawnVFXPrefabClientRpc(ulong particleNgoid, string path,
            float duration, ulong targetNGOID = Invalidobjectid)
        {
            Action<GameObject> positionAndBehaviorSetterEvent = null;
            if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(particleNgoid,
                    out NetworkObject paricleNgo))
            {
                if (paricleNgo.TryGetComponent(out NgoParticleInitializeBase skillInitailze))
                {
                    skillInitailze.SetInitialize(paricleNgo);
                    if (_relayManager.NetworkManagerEx.SpawnManager.SpawnedObjects.TryGetValue(targetNGOID,
                            out NetworkObject targetNgo))
                    {
                        skillInitailze.SetTargetInitialize(targetNgo);
                        positionAndBehaviorSetterEvent += (particleGameObject) =>
                        {
                            _vfxManager.FollowParticleRoutine(targetNgo.transform, particleGameObject, path,
                                duration);
                        };
                    }

                    skillInitailze.StartParticleOption(positionAndBehaviorSetterEvent);
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void Call_InitBuffer_ServerRpc(StatEffect effect, string buffIconImagePath = null, float duration = -1)
        {
            Call_InitBuffer_ClientRpc(effect, buffIconImagePath, duration);
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void Call_InitBuffer_ClientRpc(StatEffect effect, string buffIconImagePath = null, float duration = -1)
        {
            PlayerStats playerstats = _gameManagerEx.GetPlayer().GetComponent<PlayerStats>();

            if (_bufferManager.GetModifier(effect) is DurationBuff durationbuff)
            {
                Sprite buffImageIcon = _resourcesServices.Load<Sprite>(buffIconImagePath);
                durationbuff.SetBuffIconImage(buffImageIcon);
                _bufferManager.InitBuff(playerstats, duration, durationbuff, effect.value);
            }
            else
            {
                _bufferManager.InitBuff(playerstats, duration, effect);
            }
        }

        private async Task DisconnectFromVivoxAndLobby()
        {
            try
            {
                Lobby currentLobby = await _lobbyManager.GetCurrentLobby();

                if (currentLobby == null)
                {
                    return;
                }

                await _lobbyManager.RemoveLobbyAsync(currentLobby);
                await _vivoxSession.LogoutOfVivoxAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Disconneted NetWorkError] Error: {e}");
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
            //Debug.Log($"{clientId} 유저가 {selectCharacterName}을 등록함");
            Define.PlayerClass selectCharacter =
                (Define.PlayerClass)Enum.Parse(typeof(Define.PlayerClass), selectCharacterName);
            _relayManager.RegisterSelectedCharacterInDict(clientId, selectCharacter);
        }

        public void SpawnLocalObject(Vector3 pos, string objectPath, SpawnParamBase spawnParamBase)
        {
            FixedList32Bytes<Vector3> list = new FixedList32Bytes<Vector3>();
            list.Add(pos); // 한 개만 담기
            SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), spawnParamBase);
        }

        public void SpawnNonNetworkObject(List<Vector3> pos, string objectPath, SpawnParamBase spawnParamBase)
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
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), spawnParamBase);
                    break;
                }
                case <= 5:
                {
                    FixedList64Bytes<Vector3> list = new FixedList64Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), spawnParamBase);
                    break;
                }
                case <= 10:
                {
                    FixedList128Bytes<Vector3> list = new FixedList128Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), spawnParamBase);
                    break;
                }
                case <= 42:
                {
                    FixedList512Bytes<Vector3> list = new FixedList512Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), spawnParamBase);
                    break;
                }
                case <= 340:
                {
                    FixedList4096Bytes<Vector3> list = new FixedList4096Bytes<Vector3>();
                    foreach (var p in pos) list.Add(p);
                    SpwanLocalObjectRpc(list, new FixedString512Bytes(objectPath), spawnParamBase);
                    break;
                }
                default:
                    Debug.LogError("Too many positions! Maximum supported is 340.");
                    break;
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList32Bytes<Vector3>> posList,
            FixedString512Bytes path, SpawnParamBase spawnParamBase)
        {
            ProcessLocalSpawn(posList.Value, path, spawnParamBase);
        }
        

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList64Bytes<Vector3>> posList,
            FixedString512Bytes path, SpawnParamBase spawnParamBase)
        {
            ProcessLocalSpawn(posList.Value, path, spawnParamBase);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList128Bytes<Vector3>> posList,
            FixedString512Bytes path, SpawnParamBase spawnParamBase)
        {
            ProcessLocalSpawn(posList.Value, path, spawnParamBase);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList512Bytes<Vector3>> posList,
            FixedString512Bytes path, SpawnParamBase spawnParamBase)
        {
            ProcessLocalSpawn(posList.Value, path, spawnParamBase);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpwanLocalObjectRpc(ForceNetworkSerializeByMemcpy<FixedList4096Bytes<Vector3>> posList,
            FixedString512Bytes path, SpawnParamBase spawnParamBase)
        {
            ProcessLocalSpawn(posList.Value, path, spawnParamBase);
        }

        private void ProcessLocalSpawn<TList>(TList posList, FixedString512Bytes path, SpawnParamBase spawnParamBase)
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
                    SpawnParamBase spawnParams = spawnParamBase;
                    spawnParams.ArgPosVector3 = fixedList[i];
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
                Debug.Log("Call RPCCaller in ResetManagersRpc");
            }

            [Rpc(SendTo.Server, RequireOwnership = false)]
            public void OnBeforeSceneUnloadRpc()
            {
                foreach (NetworkObject ngo in _relayManager.NetworkManagerEx.SpawnManager.SpawnedObjectsList)
                {
                    if (ngo.TryGetComponent(out ISceneChangeBehaviour behaviour))
                    {
                        Debug.Log((behaviour as Component).name + "초기화 처리 됨");
                        behaviour.OnBeforeSceneUnload();
                    }
                }
            }


            [Rpc(SendTo.ClientsAndHost)]
            public void OnBeforeSceneUnloadLocalRpc()
            {
                _sceneManagerEx.InvokeOnBeforeSceneUnloadLocalEvent();

                _ = DisconnectFromVivoxAndLobby(); //비복스 및 로비 연결해제
            }


            public void Register(ISpawnController spawnController)
            {
               _spawnController = spawnController;
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