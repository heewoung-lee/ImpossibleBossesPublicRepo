using System.Collections;
using Data.DataType.ItemType.Interface;
using DataType;
using DataType.Item;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.ItamData.Interface;
using GameManagers.ItamDataManager.Interface;
using GameManagers.ResourcesEx;
using Module.CommonModule;
using Module.PlayerModule;
using NetWork.NGO;
using Player;
using UI.Popup.PopupUI;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;
using Random = UnityEngine.Random;

namespace NetWork.Item
{
    public class LootItem : NetworkBehaviour, IInteraction
    {
        public class LootItemFactory : NgoZenjectFactory<LootItem>
        {
            [Inject]
            public LootItemFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService,
                string key) : base(container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>(key);
            }
        }

        private NetworkVariable<int> _itemNumberNetVar = new NetworkVariable<int>
            (-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        
        private IUIManagerServices _uiManagerServices;
        private IPlayerSpawnManager _gameManagerEx;
        private IVFXManagerServices _vfxManager;
        private IResourcesServices _resourcesManager;
        
        private IItemDataManager _itemDataManager; 
        private ItemDataSO _itemData; 

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            IPlayerSpawnManager gameManagerEx,
            IItemDataManager itemDataManager,
            IVFXManagerServices vfxManager,
            IResourcesServices resourcesManager)
        {
            _uiManagerServices = uiManagerServices;
            _gameManagerEx = gameManagerEx;
            _itemDataManager = itemDataManager;
            _vfxManager = vfxManager;
            _resourcesManager = resourcesManager;
        }

        private const float AddforceOffset = 5f;
        private const float TorqueForceOffset = 30f;
        private const float DropitemVerticalOffset = 0.2f;
        private const float DropitemRotationOffset = 40f;
        
        private UIPlayerInventory _uiPlayerInventory;
        private Vector3 _dropPosition;
        private Rigidbody _rigidBody;
        
        
        public bool CanInteraction => _canInteraction;
        public string InteractionName => _itemData != null ? _itemData.dataName : "";
        public Color InteractionNameColor => _itemData != null ? Utill.GetItemGradeColor(_itemData.itemGrade) : Color.white;

        private bool _canInteraction = false;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        public void Initialize(ItemDataSO data)
        {
            _itemData = data;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _itemNumberNetVar.OnValueChanged += OnItemNumberChanged;
            
            if (IsServer && _itemData != null)
            {
                _itemNumberNetVar.Value = _itemData.itemNumber;
                //서버라면 아이템의 값을 할당
            }
            LoadItemData(_itemNumberNetVar.Value);
            SpawnBehaviour(); 
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _itemNumberNetVar.OnValueChanged -= OnItemNumberChanged;
        }

        private void OnItemNumberChanged(int oldVal, int newVal)
        {
            if (newVal == -1) return;
            
            LoadItemData(newVal);
        }

        private void LoadItemData(int itemNumber)
        {
            if (_itemDataManager.TryGetItemData(itemNumber, out var data))
            {
                _itemData = data;
            }
            else
            {
                Debug.LogError($"{itemNumber}의 아이템 정보가 없습니다.");
            }
        }

        public void SpawnBehaviour()
        {
            _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
            _canInteraction = false;

            if (gameObject.TryGetComponent(out ILootItemBehaviour behaviour) == true)
            {
                behaviour.SpawnBahaviour(_rigidBody);
                return;
            }

            transform.position = _dropPosition + Vector3.up * 1.2f;
            _rigidBody.AddForce(Vector3.up * AddforceOffset, ForceMode.Impulse);
            
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) 
            );
            _rigidBody.AddTorque(randomTorque * TorqueForceOffset, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return; 
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground") == false || _rigidBody.isKinematic) return;
            
            LandedLogicRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void LandedLogicRpc()
        {
            _rigidBody.isKinematic = true;
            transform.position += Vector3.up * DropitemVerticalOffset;
            transform.rotation = Quaternion.identity;
            StartCoroutine(RotationDropItem());
            CreateLootingItemEffect();
            _canInteraction = true;
        }

        public void CreateLootingItemEffect()
        {
            if(_itemData != null)
                ItemGradeEffect(_itemData);
        }

        public void SetPosition(Vector3 dropPosition)
        {
            _dropPosition = dropPosition;
        }

        IEnumerator RotationDropItem()
        {
            while (true)
            {
                transform.Rotate(new Vector3(0, Time.deltaTime * DropitemRotationOffset, 0));
                yield return null;
            }
        }

        // [수정] 네임스페이스 경로를 지우고 깔끔하게 수정
        private void ItemGradeEffect(ItemDataSO itemInfo)
        {
            string path = itemInfo.itemGrade switch
            {
                ItemGradeType.Normal => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Common",
                ItemGradeType.Magic => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Uncommon",
                ItemGradeType.Rare => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Rare",
                ItemGradeType.Unique => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Epic",
                ItemGradeType.Epic => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Legendary",
                _ => null
            };

            if (string.IsNullOrEmpty(path)) return;

            _vfxManager.InstantiateParticleInArea(path, transform.position, parentTr: transform);
        }

        public void Interaction(ModulePlayerInteraction caller)
        {
            PlayerPickup(caller);
        }

        public void PlayerPickup(ModulePlayerInteraction player)
        {
            PlayerController baseController = player.PlayerController;
            baseController.CurrentStateType = baseController.PickupState;

            if (baseController.CurrentStateType != baseController.PickupState)
                return;

            if(_uiPlayerInventory != null && _itemData != null)
            {
                // [참고] 아이템을 주울 때 기본 1개 획득
                _uiPlayerInventory.AddItem(_itemData);
            }

            player.DisEnable_Icon_UI();
            RequestDisEnableUI_ServerRpc();
        }

        [Rpc(SendTo.Server)]
        public void RequestDisEnableUI_ServerRpc()
        {
            BroadcastDisEnableUI_Rpc();
            _resourcesManager.DestroyObject(gameObject);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void BroadcastDisEnableUI_Rpc()
        {
            var localPlayer = _gameManagerEx.GetPlayer();
            if (localPlayer == null) return;

            ModulePlayerInteraction interaction = localPlayer.GetComponentInChildren<ModulePlayerInteraction>();
            if (interaction != null && interaction.enabled && ReferenceEquals(interaction.InteractionTarget, this))
            {
                interaction.DisEnable_Icon_UI(); 
            }
        }

        public void OutInteraction()
        {
        }
    }
}