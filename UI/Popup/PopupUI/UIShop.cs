using System;
using System.Collections;
using System.Collections.Generic;
using Data.Item;
using DataType; 
using DataType.Item;
using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.ItemDataManagement.Interface;
using GameManagers.SoundManagement;
using GameManagers.UIManagement;
using Stats;
using TMPro;
using UI.Scene.SceneUI;
using UI.SubItem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UIShop : UIPopup
    {
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IItemDataManager _itemDataManager; 

        private const string ShopItemPrefabPath = "Prefabs/UI/Item/UIShopItemComponent";
        private const int EquipmentShopItemCount = 5;
        private const int NormalEquipmentWeight = 50;
        private const int MagicEquipmentWeight = 30;
        private const int RareEquipmentWeight = 13;
        private const int UniqueEquipmentWeight = 5;
        private const int EpicEquipmentWeight = 2;

        enum ItemShopText { EquipItemTapText, ConsumableItemTapText, ETCItemTapText, PlayerHasGoldText }
        enum IconImages { EquipItemTap, ConsumableItemTap, ETCItemTap, TabFocusLine }

        Dictionary<GameObject, (TMP_Text, ItemType)> _findGameObjectToTMPTextDict;
        private TMP_Text _equipItemTapText;
        private TMP_Text _consumableItemTapText;
        private TMP_Text _etcItemTapText;
        private TMP_Text _playerHasGoldText;
        private TMP_Text _currentFocusText;
        private Image _equipItemIcon;
        private Image _consumableItemIcon;
        private Image _etcItemIcon;
        private Image _tabFocusLine;
        private Transform _itemCoordinate;
        private PlayerStats _playerStats;
        private Coroutine _moveCoroutine;
        private Color _focusColor;
        private Color _nonFocusColor;
        private GraphicRaycaster _uiShopRaycaster;
        private EventSystem _eventSystem;

        public GraphicRaycaster UIShopRayCaster => _uiShopRaycaster;
        public EventSystem EventSystem => _eventSystem;
        public Transform ItemCoordinate => _itemCoordinate;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_Text>(typeof(ItemShopText));
            Bind<Image>(typeof(IconImages));
            _equipItemTapText = GetText((int)ItemShopText.EquipItemTapText);
            _consumableItemTapText = GetText((int)ItemShopText.ConsumableItemTapText);
            _etcItemTapText = GetText((int)ItemShopText.ETCItemTapText);
            _playerHasGoldText = GetText((int)ItemShopText.PlayerHasGoldText);
            
            _equipItemIcon = GetImage((int)IconImages.EquipItemTap);
            _consumableItemIcon = GetImage((int)IconImages.ConsumableItemTap);
            _etcItemIcon = GetImage((int)IconImages.ETCItemTap);
            _tabFocusLine = GetImage((int)IconImages.TabFocusLine);

            _findGameObjectToTMPTextDict = new Dictionary<GameObject, (TMP_Text, ItemType)>()
            {
                {_equipItemTapText.transform.parent.gameObject,(_equipItemTapText,ItemType.Equipment)},
                {_consumableItemTapText.transform.parent.gameObject,(_consumableItemTapText,ItemType.Consumable)},
                {_etcItemTapText.transform.parent.gameObject,(_etcItemTapText,ItemType.ETC)}, 
            };

            _focusColor = "#F6E19C".HexCodetoConvertColor();
            _nonFocusColor = "#BEB5B6".HexCodetoConvertColor();
            _currentFocusText = _equipItemTapText;
            _itemCoordinate = gameObject.FindChild<ItemShopContentCoordinate>(null, true).transform;

            _uiShopRaycaster = GetComponent<GraphicRaycaster>();
            _eventSystem = FindAnyObjectByType<EventSystem>();
        }

        public void InitializePlayerStatEvent(PlayerStats playerstats)
        {
            _playerStats = playerstats;
            ZenjectEnable(); 
            UpdateHasGoldChanged(_playerStats.Gold);
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _uiManagerServices.AddImportant_Popup_UI(this);
            
            var player = _gameManagerEx.GetPlayer();
            if (player != null && player.TryGetComponent(out PlayerStats stats))
            {
                _playerStats = stats;
            }
            else
            {
                _gameManagerEx.OnPlayerSpawnEvent += InitializePlayerStatEvent;
            }
        }
        private void OnDestroy() //파괴 되면 구독해제
        {
            if (_gameManagerEx != null)
            {
                _gameManagerEx.OnPlayerSpawnEvent -= InitializePlayerStatEvent;
            }
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            
            if (_playerStats == null) return;
            
            _closePopupUI.performed += CloseDecriptionWindow;
            _playerStats.PlayerHasGoldChangeEvent += UpdateHasGoldChanged;
            BindEvent(_equipItemIcon.gameObject, ClickToTab);
            BindEvent(_consumableItemIcon.gameObject, ClickToTab);
            BindEvent(_etcItemIcon.gameObject, ClickToTab);
            UpdateHasGoldChanged(_playerStats.Gold);
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            if (_playerStats == null) return;
            
            _closePopupUI.performed -= CloseDecriptionWindow;
            _playerStats.PlayerHasGoldChangeEvent -= UpdateHasGoldChanged;
            UnBindEvent(_equipItemIcon.gameObject, ClickToTab);
            UnBindEvent(_consumableItemIcon.gameObject, ClickToTab);
            UnBindEvent(_etcItemIcon.gameObject, ClickToTab);
            CloseDecriptionWindow();
        }

        public void UpdateHasGoldChanged(int gold)
        {
            _playerHasGoldText.text = gold.ToString();
        }

        public void CloseDecriptionWindow(InputAction.CallbackContext context)
        {
            CloseDecriptionWindow();
        }

        public void CloseDecriptionWindow()
        {
            if (_uiManagerServices.Try_Get_Scene_UI(out UIDescription description))
            {
                description.UI_DescriptionDisable();
                description.SetdecriptionOriginPos();
            }
        }

        public void ClickToTab(PointerEventData eventData)
        {
            _soundManagerServices.PlayUiSfx(gameObject, UICommonSoundCueId.Hover);
            _currentFocusText.color = _nonFocusColor;

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
            _moveCoroutine = StartCoroutine(MovetoFocusTab(_tabFocusLine.transform, eventData.pointerPress.transform));
            if (_findGameObjectToTMPTextDict.TryGetValue(eventData.pointerPress, out (TMP_Text, ItemType) value))
            {
                (TMP_Text focusText, ItemType type) = value;
                focusText.color = _focusColor;
                _currentFocusText = focusText;

                ShowItemTypeForSelectedTab(type);
            }
        }

        public void ShowItemTypeForSelectedTab(ItemType type)
        {
            foreach (Transform childTr in _itemCoordinate)
            {
                if (childTr.TryGetComponent(out UIShopItemComponent shopItem))
                {
                    if (shopItem.ItemType == type)
                        childTr.gameObject.SetActive(true);
                    else
                        childTr.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator MovetoFocusTab(Transform originTr, Transform tarGetTr)
        {
            float elapsedTime = 0f;
            float duration = 0.4f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float ratio = Mathf.Clamp01(elapsedTime / duration);
                _tabFocusLine.transform.position = Vector3.Lerp(originTr.position, tarGetTr.position, ratio);
                yield return null;
            }
        }

        protected override void StartInit()
        {
            if (_playerStats != null)
            {
                UpdateHasGoldChanged(_playerStats.Gold);
            }
            
            RespawnShopItems();
            ShowItemTypeForSelectedTab(ItemType.Equipment);
        }

        public void RespawnShopItems()
        {
            CreateSellValueShopItems(ItemType.Consumable);
            CreateSellValueShopItems(ItemType.ETC);

            for (int i = 0; i < EquipmentShopItemCount; i++)
            {
                ItemGradeType grade = GetWeightedEquipmentGrade();
                ItemDataSO equipData = _itemDataManager.GetRandomItemData(ItemType.Equipment, grade);
                CreateShopItem(equipData, GetEquipmentPrice(grade), 1);
            }
        }

        /// <summary>
        /// 가중치 기반 랜덤 선택.
        /// 전체 가중치 합 안에서 난수 하나를 뽑고, 앞 등급의 가중치를 차례로 빼면서
        /// 현재 난수가 어느 등급 구간에 들어가는지 확인한다.
        /// 예: 50/30/13/5/2이면 0~49 Normal, 50~79 Magic, 80~92 Rare,
        /// 93~97 Unique, 98~99 Epic 구간이 된다.
        /// </summary>
        private ItemGradeType GetWeightedEquipmentGrade()
        {
            int totalWeight =
                NormalEquipmentWeight +
                MagicEquipmentWeight +
                RareEquipmentWeight +
                UniqueEquipmentWeight +
                EpicEquipmentWeight;

            if (totalWeight <= 0)
                return ItemGradeType.Normal;

            int roll = UnityEngine.Random.Range(0, totalWeight);
            if (roll < NormalEquipmentWeight)
                return ItemGradeType.Normal;

            roll -= NormalEquipmentWeight;
            if (roll < MagicEquipmentWeight)
                return ItemGradeType.Magic;

            roll -= MagicEquipmentWeight;
            if (roll < RareEquipmentWeight)
                return ItemGradeType.Rare;

            roll -= RareEquipmentWeight;
            if (roll < UniqueEquipmentWeight)
                return ItemGradeType.Unique;

            return ItemGradeType.Epic;
        }

        private int GetEquipmentPrice(ItemGradeType grade)
        {
            switch (grade)
            {
                case ItemGradeType.Normal:
                    return UnityEngine.Random.Range(5, 11);
                case ItemGradeType.Magic:
                    return UnityEngine.Random.Range(8, 13);
                case ItemGradeType.Rare:
                    return UnityEngine.Random.Range(10, 26);
                case ItemGradeType.Unique:
                    return UnityEngine.Random.Range(15, 36);
                case ItemGradeType.Epic:
                    return UnityEngine.Random.Range(20, 51);
                default:
                    return UnityEngine.Random.Range(5, 11);
            }
        }

        private void CreateSellValueShopItems(ItemType type)
        {
            List<ItemDataSO> sellValueItems = _itemDataManager.GetItemDataList(type);

            foreach (ItemDataSO itemData in sellValueItems)
            {
                IHasSellValue sellValueItem = itemData as IHasSellValue;
                if (sellValueItem == null)
                    continue;

                CreateShopItem(itemData, sellValueItem.SellValue, 1);
            }
        }

        private void CreateShopItem(ItemDataSO data, int price, int count)
        {
            if (data == null) return;
            
            UIShopItemComponent itemObj =_uiManagerServices.MakeSubItem<UIShopItemComponent>(_itemCoordinate,path: ShopItemPrefabPath);
            if (itemObj.TryGetComponent(out UIShopItemComponent shopItem))
            {
                shopItem.InitializeItem(data, count, price);
            }
        }
    }
}
