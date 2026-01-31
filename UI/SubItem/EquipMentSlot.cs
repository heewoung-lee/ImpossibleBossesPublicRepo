using System;
using Controller;
using Data.Item;
using DataType.Item; // ItemDataSO
using DataType.Item.Equipment;
using DataType.Skill.Factory;
using DataType.Skill.Factory.Effect;
using DataType.Strategies; // EquipmentSlotType
using GameManagers; // SceneDataSaveAndLoader
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ItamData.Interface;
using GameManagers.Scene;
using GameManagers.UIFactory.SubItemUI;
using Skill;
using Stats.BaseStats;
using UI.Popup.PopupUI;
using UnityEngine;
using Zenject;

namespace UI.SubItem
{
    public class EquipMentSlot : MonoBehaviour
    {
        public EquipmentSlotType slotType;

        private IUIManagerServices _uiManagerServices;
        private IItemDataManager _itemDataManager;
        private IPlayerSpawnManager _gameManagerEx;
        private SceneDataSaveAndLoader _sceneDataSaveAndLoader;
        private IUIItemFactory _uiItemFactory;
        private IStrategyFactory _strategyFactory;
        
        [Inject]
        public void Construct(IUIManagerServices uiManagerServices, IItemDataManager itemDataManager,  
            IPlayerSpawnManager gameManagerEx, SceneDataSaveAndLoader sceneDataSaveAndLoader, 
            IUIItemFactory uiItemFactory,IStrategyFactory effectFactory)
        {
            _uiManagerServices = uiManagerServices;
            _itemDataManager = itemDataManager;
            _gameManagerEx = gameManagerEx;
            _sceneDataSaveAndLoader = sceneDataSaveAndLoader;
            _uiItemFactory = uiItemFactory;
            _strategyFactory = effectFactory;
        }
        
        private UIPlayerInventory _uiPlayerInventory;
        private Transform _contentofInventoryTr;
        private BaseStats _playerStats;
        private UIItemComponentInventory _equipedItem;

        // 장비 UI 프리팹 경로 (UIPlayerInventory와 동일하게 맞춤)

        public BaseStats PlayerStats
        {
            get
            {
                if (_playerStats == null)
                {
                    var player = _gameManagerEx.GetPlayer();
                    if (player != null && player.TryGetComponent(out BaseStats stats))
                    {
                        _playerStats = stats;
                    }
                }

                return _playerStats;
            }
        }
        public bool IsEquipped { get; private set; } = false;

        private void OnDestroy()
        {
            // 씬 이동 등으로 파괴될 때 데이터 저장
            SaveDataFromEquipment();
        }

        private void SaveDataFromEquipment()
        {
            if (IsEquipped && _equipedItem != null)
            {
                _sceneDataSaveAndLoader.SaveEquipMentData(slotType, _equipedItem);

                ProcessStrategy(_equipedItem.ItemNumber, false);

                IsEquipped = false;
                _equipedItem.transform.SetParent(null);
                _equipedItem.SetItemEquipedState(false);
            }
        }

        void Start()
        {
            string slotTypeName = transform.gameObject.name.Replace("_Item_Slot", "");
            slotType = (EquipmentSlotType)Enum.Parse(typeof(EquipmentSlotType), slotTypeName);

            _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
            if (_uiPlayerInventory != null)
            {
                var contentCoord = _uiPlayerInventory.GetComponentInChildren<InventoryContentCoordinate>();
                if (contentCoord != null) _contentofInventoryTr = contentCoord.transform;
            }

            LoadSavedEquipment();
        }

        private void LoadSavedEquipment()
        {
            if (_sceneDataSaveAndLoader.TryGetLoadEquipMentData(slotType, out IteminfoStruct iteminfo))
            {
                if (_itemDataManager.TryGetItemData(iteminfo.ItemNumber, out ItemDataSO data))
                {
                    UIItemComponentInventory equipItem = _uiItemFactory.CreateItemUI(data, null);
                
                    // 슬롯에 등록
                    ItemEquip(equipItem);
                    equipItem.SetItemEquipedState(true); 
                    equipItem.transform.SetParent(transform,false);
                    //false로 해놓은 이유는 현재 장비슬롯이 해당 장비의 부모가 될때,
                    //프리펩 로컬 크기와 위치를 유지하며 슬롯에 맞게 들어가기 위함.
                    //true로 해놓으면 아이템이 갖는 본래 월드좌표와 크기가 되니 오류가 생김.
                }
            }
        }

        public void ItemEquip(UIItemComponentInventory itemComponent)
        {
            if (IsEquipped && _equipedItem != null)
            {
                ProcessStrategy(_equipedItem.ItemNumber, false); 
                UIItemComponentInventory oldItem = _equipedItem;
                oldItem.transform.SetParent(_contentofInventoryTr, false); 
                oldItem.SetItemEquipedState(false);
            }

            _equipedItem = itemComponent;
            IsEquipped = true;

            ProcessStrategy(_equipedItem.ItemNumber, true); 
        }
        public void ItemUnEquip()
        {
            if (IsEquipped && _equipedItem != null)
            {
                ProcessStrategy(_equipedItem.ItemNumber, false); // false = 해제
            }
            IsEquipped = false;
            _equipedItem = null;
        }
        private void ProcessStrategy(int itemNumber, bool isEquip)
        {
            if (_itemDataManager.TryGetItemData(itemNumber, out ItemDataSO data))
            {
                var strategy = _strategyFactory.GetStrategy(data);
                if (strategy is IEquippable equipStrategy && data is EquipmentItemSO equipData)
                {
                    if (isEquip)
                        equipStrategy.Equip(PlayerStats, equipData);
                    else 
                        equipStrategy.UnEquip(PlayerStats, equipData);
                }
            }
        }
    }
}