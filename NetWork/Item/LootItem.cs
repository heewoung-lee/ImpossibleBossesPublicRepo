using System.Collections;
using Controller;
using Data.DataType.ItemType.Interface;
using DataType;
using DataType.Item;
using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.ItemDataManagement.Interface;
using GameManagers.ResourcesExManagement;
using GameManagers.SoundManagement;
using GameManagers.UIManagement;
using GameManagers.VFXManagement;
using Module.CommonModule;
using Module.PlayerModule;
using NetWork.NGO;
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
        private const string ItemDropSoundCueId = "ItemDropSFX";
        private const string PickupSoundCueId = "PickupSFX";

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
        private const float MinimumDropitemVerticalOffset = 0.2f;
        private const float DropitemRotationOffset = 40f;
        private const float GroundRayExtraHeight = 1f;
        private const float GroundRayDistance = 10f;
        private const float GroundSnapPadding = 0.02f;
        private const float LootEffectVerticalOffset = 0.2f;

        private UIPlayerInventory _uiPlayerInventory;
        private Vector3 _dropPosition;
        private Rigidbody _rigidBody;
        private Collider _lootCollider;
        private SoundPlayerBinder _soundPlayerBinder;
        private bool _hasLanded;
        private bool _isPickupRequested;
        
        
        public bool CanInteraction => _canInteraction;
        public string InteractionName => _itemData != null ? _itemData.dataName : "";
        public Color InteractionNameColor => _itemData != null ? Utill.GetItemGradeColor(_itemData.itemGrade) : Color.white;

        private bool _canInteraction = false;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _lootCollider = GetComponent<Collider>();
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public void Initialize(ItemDataSO data)
        {
            _itemData = data;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _itemNumberNetVar.OnValueChanged += OnItemNumberChanged;
            _hasLanded = false;
            _isPickupRequested = false;
            
            if (IsServer && _itemData != null)
            {
                _itemNumberNetVar.Value = _itemData.itemNumber;
                //서버라면 아이템의 값을 할당
            }
            LoadItemData(_itemNumberNetVar.Value);
            SpawnBehaviour();
            _soundPlayerBinder.PlayDetached(ItemDropSoundCueId);
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
                UtilDebug.LogError($"{itemNumber}의 아이템 정보가 없습니다.");
            }
        }

        public void SpawnBehaviour()
        {
            _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
            _canInteraction = false;

            if (gameObject.TryGetComponent(out ILootItemBehaviour behaviour) == true)
            {
                behaviour.SpawnBehaviour(_rigidBody);
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
            if (_hasLanded || other.gameObject.layer == LayerMask.NameToLayer("Ground") == false || _rigidBody.isKinematic) return;

            ResolveLanding(other.gameObject.layer);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void LandedLogicRpc(Vector3 landedPosition)
        {
            if (IsHost)
                return;

            if (_hasLanded)
                return;

            _hasLanded = true;
            ApplyLandedState(landedPosition);
        }

        public bool TryResolveLandingFromTrajectory(int groundLayer)
        {
            if (!IsServer || _hasLanded)
            {
                return false;
            }

            ResolveLanding(groundLayer);
            return true;
        }

        private void ResolveLanding(int groundLayer)
        {
            _hasLanded = true;
            Vector3 landedPosition = GetLandedPosition(groundLayer);
            ApplyLandedState(landedPosition);
            LandedLogicRpc(landedPosition);
        }

        private void ApplyLandedState(Vector3 landedPosition)
        {
            _rigidBody.isKinematic = true;
            transform.position = landedPosition;
            transform.rotation = Quaternion.identity;
            StartCoroutine(RotationDropItem());
            CreateLootingItemEffect(landedPosition);
            _canInteraction = true;
        }

        private Vector3 GetLandedPosition(int groundLayer)
        {
            float halfHeight = _lootCollider != null ? _lootCollider.bounds.extents.y : 0f;
            float verticalOffset = Mathf.Max(halfHeight + GroundSnapPadding, MinimumDropitemVerticalOffset);

            Vector3 rayOrigin = transform.position + Vector3.up * (halfHeight + GroundRayExtraHeight);
            int groundMask = 1 << groundLayer;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, halfHeight + GroundRayDistance, groundMask))
            {
                return hit.point + Vector3.up * verticalOffset;
            }

            return transform.position + Vector3.up * verticalOffset;
        }

        public void CreateLootingItemEffect(Vector3 landedPosition)
        {
            if (_itemData.UseLootGradeEffect  == false)
            {
                return;
            }

            if(_itemData != null)
                ItemGradeEffect(_itemData, landedPosition);
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

        private void ItemGradeEffect(ItemDataSO itemInfo, Vector3 landedPosition)
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

            Vector3 effectPosition = landedPosition;
            if (_lootCollider != null)
            {
                float halfHeight = _lootCollider.bounds.extents.y;
                effectPosition = new Vector3(
                    landedPosition.x,
                    landedPosition.y - halfHeight + LootEffectVerticalOffset,
                    landedPosition.z);
            }

            _vfxManager.InstantiateParticleInArea(path, effectPosition, parentTr: transform);
        }

        public void Interaction(ModulePlayerInteraction caller)
        {
            PlayerPickup(caller);
        }

        public void PlayerPickup(ModulePlayerInteraction player)
        {
            if (_isPickupRequested)
                return;

            PlayerController baseController = player.PlayerController;
            baseController.CurrentStateType = baseController.PickupState;

            if (baseController.CurrentStateType != baseController.PickupState)
                return;

            _isPickupRequested = true;
            _canInteraction = false;

            if (TryGetComponent(out CoinAmountNetwork coinAmountNetwork))
            {
                coinAmountNetwork.ApplyPickupReward(player);
            }
            else if(_uiPlayerInventory != null && _itemData != null)
            {
                // [참고] 아이템을 주울 때 기본 1개 획득
                _uiPlayerInventory.AddItem(_itemData);
            }

            if (player.transform.TryGetComponentInParents(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(PickupSoundCueId);
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
