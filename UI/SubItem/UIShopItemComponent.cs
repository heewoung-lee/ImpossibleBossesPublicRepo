using System.Collections.Generic;
using Data.Item;
using DataType;
using DataType.Item;
using DataType.Item.Consumable;
using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.ItemDataManagement.Interface;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using Stats;
using TMPro;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.SubItem
{
    public class UIShopItemComponent : UIItemComponent
    {
        private const string BuyItemCueId = "BuyItem";

        [Inject] private IResourcesServices _destroyer;
        [Inject] private IItemGradeBorder _itemGradeBorder;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        [Inject] private IUIManagerServices _uiManagerServices;

        enum ItemICons
        {
            ItemIconImage,
            ItemGradeBorderImage
        }

        enum ItemTexts
        {
            ItemNameText,
            ItemPriceText,
            ItemCountText
        }

        private UIShop _uiShop;

        private TMP_Text _itemNameText;
        private TMP_Text _itemgPriceText;
        private TMP_Text _itemCountText;
        private int _itemPrice;
        private int _itemCount;
        private UIPlayerInventory _uiPlayerInventory;

        private RectTransform _itemRectTr;
        private PlayerStats _playerStats;
        private Image _itemGradeBorderImage;

        protected GraphicRaycaster _uiRaycaster;
        protected EventSystem _eventSystem;

        private bool HasLimitedStock => _itemData != null && _itemData.ItemType == ItemType.Equipment;

        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;

                if (_itemCount <= 1)
                {
                    _itemCountText.text = "";
                }
                else
                {
                    _itemCountText.text = _itemCount.ToString();
                }
            }
        }

        public override RectTransform ItemRectTr => _itemRectTr;

        public int ItemPrice
        {
            get => _itemPrice;
            set
            {
                _itemPrice = value;
                _itemgPriceText.text = _itemPrice.ToString();
            }
        }

        public ItemType ItemType => _itemData != null ? _itemData.ItemType : ItemType.ETC;

        protected override void AwakeInit()
        {
            Bind<Image>(typeof(ItemICons));
            Bind<TMP_Text>(typeof(ItemTexts));
            _itemIconSourceImage = GetImage((int)ItemICons.ItemIconImage);
            _itemGradeBorderImage = GetImage((int)ItemICons.ItemGradeBorderImage);
            _itemNameText = Get<TMP_Text>((int)ItemTexts.ItemNameText);
            _itemCountText = Get<TMP_Text>((int)ItemTexts.ItemCountText);
            _itemgPriceText = Get<TMP_Text>((int)ItemTexts.ItemPriceText);
            _itemRectTr = GetComponent<RectTransform>();
        }

        protected override void StartInit()
        {
            base.StartInit();

            if (_itemData != null)
            {
                _itemNameText.text = _itemData.dataName;
                _itemGradeBorderImage.sprite = _itemGradeBorder.GetGradeBorder(_itemData.itemGrade);
            }

            var player = _gameManagerEx.GetPlayer();
            if (player != null) _playerStats = player.GetComponent<PlayerStats>();

            _uiPlayerInventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
            _uiShop = _uiManagerServices.GetImportant_Popup_UI<UIShop>();

            if (_uiShop != null)
            {
                _uiRaycaster = _uiShop.UIShopRayCaster;
                _eventSystem = _uiShop.EventSystem;
            }
        }

        public void InitializeItem(ItemDataSO data, int count, int price)
        {
            base.InitializeItem(data);

            ItemPrice = price;

            // 장비는 1개, 소비는 여러 개
            ItemCount = HasLimitedStock ? count : 1;
        }

        public override void ItemRightClick(PointerEventData eventdata)
        {
            base.ItemRightClick(eventdata);
            BuyItem();
        }

        private void BuyItem()
        {
            if (_playerStats == null)
                return;

            if (_itemData is ExperienceBookItemSO && _playerStats.IsMaxLevel)
            {
                _uiManagerServices.GetMessageErrorToast().Show("이미 최대 레벨입니다");
                return;
            }

            if (_playerStats.TrySpendMoney(_itemPrice) == true)
            { //살 돈이 있다면
                if (_itemData is ExperienceBookItemSO experienceBookItem) // 경험치책은 즉시 적용
                {
                    _playerStats.Exp += experienceBookItem.experienceAmount;
                }
                else if (_uiPlayerInventory != null)
                {
                    _uiPlayerInventory.AddItem(_itemData);
                }

                if (HasLimitedStock)
                {
                    ItemCount--;
                    if (_itemCount <= 0)
                        _destroyer.DestroyObject(gameObject);
                }

                _soundManagerServices.PlayUiSfx(gameObject, BuyItemCueId);
            }
            else
            {
                _uiManagerServices.GetMessageErrorToast().Show("돈이 부족합니다");
            }
        }

        public override void GetDragEnd(PointerEventData eventData)
        {
            // 드래그 종료 시 이미지 복구
            RevertImage();

            List<RaycastResult> uiResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, uiResults);

            foreach (RaycastResult uiResult in uiResults)
            {
                // 인벤토리 영역에 드랍하면 구매
                if (uiResult.gameObject.TryGetComponentInChildren(out InventoryContentCoordinate contextTr))
                {
                    BuyItem();
                    break; // 한 번만 구매
                }
            }
        }
    }
}
