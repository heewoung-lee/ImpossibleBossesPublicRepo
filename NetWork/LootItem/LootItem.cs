using System.Collections;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ItemDataManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using Module.CommonModule;
using Module.PlayerModule;
using NetWork.NGO;
using Player;
using UI.Popup.PopupUI;
using UI.SubItem;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;
using Random = UnityEngine.Random;

namespace NetWork.LootItem
{
    public class LootItem : NetworkBehaviour, IInteraction
    {
        public class LootItemFactory : NgoZenjectFactory<string, LootItem>
        {
            [Inject]
            public LootItemFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService,
                string key) : base(container, registerableFactory, handlerFactory, loadService, key)
            {
             _requestGO = loadService.Load<GameObject>(key);
            }
        }

        private NetworkVariable<IteminfoStruct> _itemInfoStructNetvariable = new NetworkVariable<IteminfoStruct>
            (default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        private IUIManagerServices _uiManagerServices;
        private IPlayerSpawnManager _gameManagerEx;
        private IVFXManagerServices _vfxManager;
        private IItemGetter _itemGetter;
        private RelayManager _relayManager;

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            IPlayerSpawnManager gameManagerEx,
            IItemGetter itemGetter,
            IVFXManagerServices vfxManager,
            RelayManager relayManager)
        {
            _uiManagerServices = uiManagerServices;
            _gameManagerEx = gameManagerEx;
            _itemGetter = itemGetter;
            _vfxManager = vfxManager;
            _relayManager = relayManager;
        }


        private const float AddforceOffset = 5f;
        private const float TorqueForceOffset = 30f;
        private const float DropitemVerticalOffset = 0.2f;
        private const float DropitemRotationOffset = 40f;
        private UIPlayerInventory _uiPlayerInventory;

        private Vector3 _dropPosition;
        private Rigidbody _rigidBody;
        private IItem _iteminfo;

        public bool CanInteraction => _canInteraction;
        public string InteractionName => _iteminfo.ItemName;
        public Color InteractionNameColor => Utill.GetItemGradeColor(_iteminfo.ItemGradeType);

        private bool _canInteraction = false;

        [Rpc(SendTo.Server)]
        public void SetItemInfoStructRpc(IteminfoStruct itemInfoStruct)
        {
            _itemInfoStructNetvariable.Value = itemInfoStruct;
        }

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ApplyItemInfo(default, _itemInfoStructNetvariable.Value);
            _itemInfoStructNetvariable.OnValueChanged += ApplyItemInfo;
            SpawnBehaviour();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _itemInfoStructNetvariable.OnValueChanged -= ApplyItemInfo;
        }

        private void ApplyItemInfo(IteminfoStruct preItemStructInfo, IteminfoStruct newItemStructInfo)
        {
            if (newItemStructInfo.Equals(default(IteminfoStruct)))
                return;

            _iteminfo = _itemGetter.GetItemByItemNumber(newItemStructInfo.ItemNumber)
                .SetIItemEffect(newItemStructInfo);
        }

        public void SpawnBehaviour()
        {
            _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
            _canInteraction = false;

            if (TryGetComponent(out ILootItemBehaviour behaviour) == true)
            {
                behaviour.SpawnBahaviour(_rigidBody);
                return;
            }

            //튀어오르면서 로테이션을 돌린다.
            //바닥에 닿으면 VFX효과를 킨다.
            //아이템을 회전시킨다.
            //상호작용을 하면
            transform.position = _dropPosition + Vector3.up * 1.2f;
            _rigidBody.AddForce(Vector3.up * AddforceOffset, ForceMode.Impulse);
            // 임의의 회전을 위한 랜덤 값 생성
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f), // X축 회전
                Random.Range(-1f, 1f), // Y축 회전
                Random.Range(-1f, 1f) // Z축 회전
            );
            // 회전 힘 추가 (랜덤 값에 강도를 조절)
            _rigidBody.AddTorque(randomTorque * TorqueForceOffset, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return; // 충돌 판정은 서버만
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground") == false || _rigidBody.isKinematic) return;
            LandedLogicRpc(); // 서버 로컬 처리
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
            ItemGradeEffect(_iteminfo);
        }

        public void SetPosition(Vector3 dropPosition)
        {
            _dropPosition = dropPosition;
        }

        public void SetIteminfo(IItem iteminfo)
        {
            _iteminfo = iteminfo;
        }


        IEnumerator RotationDropItem()
        {
            while (true)
            {
                transform.Rotate(new Vector3(0, Time.deltaTime * DropitemRotationOffset, 0));
                yield return null;
            }
        }

        private void ItemGradeEffect(IItem itemInfo)
        {
            string path = itemInfo.ItemGradeType switch
            {
                ItemGradeType.Normal => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Common",
                ItemGradeType.Magic => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Uncommon",
                ItemGradeType.Rare => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Rare",
                ItemGradeType.Unique => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Epic",
                ItemGradeType.Epic => "Prefabs/Particle/LootingItemEffect/Lootbeams_Runic_Legendary",
                _ => null
            };

            if (string.IsNullOrEmpty(path))
                return;

            _vfxManager.InstantiateParticle(path, transform.position, parentTr: transform);
        }


        public void Interaction(ModulePlayerInteraction caller)
        {
            PlayerPickup(caller);
        }

        public void PlayerPickup(ModulePlayerInteraction player)
        {
            PlayerController baseController = player.PlayerController;
            baseController.CurrentStateType = baseController.PickupState; //픽업 애니메이션 실행

            if (baseController.CurrentStateType != baseController.PickupState)
                return;

            UIItemComponentInventory inventoryItem =
                ((IInventoryItemMaker)_iteminfo).MakeItemComponentInventory(_uiManagerServices);
            inventoryItem.transform.SetParent(_uiPlayerInventory.ItemInventoryTr);
            player.DisEnable_Icon_UI(); //상호작용 아이콘 제거
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                DisEnble_Icon_UI_Rpc();
            }
            else
            {
                Call_DisEnable_Icon_UI_Rpc();
            }

            _relayManager.DeSpawn_NetWorkOBJ(gameObject);
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        public void DisEnble_Icon_UI_Rpc()
        {
            ModulePlayerInteraction interaction =
                _gameManagerEx.GetPlayer().GetComponentInChildren<ModulePlayerInteraction>();
            if (interaction.enabled == false)
                return;

            if (ReferenceEquals(interaction.InteractionTarget, this))
            {
                interaction.DisEnable_Icon_UI();
            }
        }

        [Rpc(SendTo.Server)]
        public void Call_DisEnable_Icon_UI_Rpc()
        {
            DisEnble_Icon_UI_Rpc();
        }

        public void OutInteraction()
        {
        }
    }
}