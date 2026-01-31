using System;
using System.Collections.Generic;
using Data.Item;
using Data.Item.EquipSlot;
using DataType.Item;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.ItamDataManager.Interface;
using GameManagers.RelayManager;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.SubItem
{
    public abstract class UIItemComponentInventory : UIItemComponent
    {
        [Inject] protected IUIManagerServices _uiManagerServices;
        [Inject] protected IItemGradeBorder _itemGradeBorderManager;
        [Inject] protected IPlayerSpawnManager _gameManagerEx;
        [Inject] protected RelayManager _relayManager;

        public enum Images
        {
            BackGroundImage,
            ItemIconSourceImage, // 프리팹의 이미지 이름과 정확히 같아야 함
            ItemGradeBorder
        }

        protected Image _backGroundImage;
        protected Image _itemGradeBorder;
        protected RectTransform _itemRectTr;

        protected Transform _contentofInventoryTr;
        protected UIPlayerInventory _inventoryUI;
        protected EquipSlotTrInfo _equipSlot;

        protected GraphicRaycaster _uiRaycaster;
        protected EventSystem _eventSystem;

        private bool _isEquipped = false;
        private Action _onAfterStart;

        public event Action OnAfterStart
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onAfterStart, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onAfterStart, value); }
        }

        public bool IsEquipped => _isEquipped;
        public override RectTransform ItemRectTr => _itemRectTr;

        protected override void AwakeInit()
        {
            Bind<Image>(typeof(Images));
            _backGroundImage = Get<Image>((int)Images.BackGroundImage);
            _itemIconSourceImage = Get<Image>((int)Images.ItemIconSourceImage);
            _itemGradeBorder = Get<Image>((int)Images.ItemGradeBorder);
            _itemRectTr = GetComponent<RectTransform>();
        }

        protected override void StartInit()
        {
            base.StartInit();

            _inventoryUI = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();

            if (_inventoryUI != null)
            {
                var contentCoord = _inventoryUI.GetComponentInChildren<InventoryContentCoordinate>(true);
                if (contentCoord != null)
                    _contentofInventoryTr = contentCoord.transform;

                _equipSlot = _inventoryUI.GetComponentInChildren<EquipSlotTrInfo>(true);

                _uiRaycaster = _inventoryUI.UIInventoryRayCaster;
                _eventSystem = _inventoryUI.EventSystem;
            }

            _onAfterStart?.Invoke();
            _onAfterStart = null;
        }

        public override void InitializeItem(ItemDataSO data)
        {
            // 1. 부모 호출 -> UIItemComponent.InitializeItem 실행
            // 부모 쪽에서 _itemIconSourceImage.sprite = data.icon 을 수행함 (이제 변수가 연결돼서 보임!)
            base.InitializeItem(data);

            if (data == null) return;

            // 2. 인벤토리 전용 로직 (등급 테두리)
            if (_itemGradeBorder != null && _itemGradeBorderManager != null)
            {
                _itemGradeBorder.sprite = _itemGradeBorderManager.GetGradeBorder(data.itemGrade);
                _itemGradeBorder.enabled = true;
            }
            SetItemEquipedState(false);
        }

        public sealed override void GetDragEnd(PointerEventData eventData)
        {
            if (_isDragging)
            {
                DropItem(eventData);
                RevertImage();
                _isDragging = false;
            }
        }

        private void DropItem(PointerEventData eventData)
        {
            if (IsPointerOverUI(eventData, out List<RaycastResult> uiraycastResult))
            {
                DropItemOnUI(eventData, uiraycastResult);
                return;
            }

            DropItemOnGround();
        }

        protected abstract void DropItemOnUI(PointerEventData eventData, List<RaycastResult> uiraycastResult);
        protected abstract void RemoveItemFromInventory();

        private bool IsPointerOverUI(PointerEventData eventData, out List<RaycastResult> uiraycastResult)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            uiraycastResult = results;
            return results.Count > 0;
        }

        protected virtual void DropItemOnGround()
        {
            RemoveItemFromInventory();
            if (_relayManager != null && _itemData != null)
            {
                IteminfoStruct itemStruct = new IteminfoStruct(_itemData.itemNumber);
                Vector3 dropPos = _gameManagerEx.GetPlayer().transform.position;
                _relayManager.NgoRPCCaller.Spawn_Loot_ItemRpc(itemStruct, dropPos);
            }
        }

        public void SetItemEquipedState(bool isEquiped) => _isEquipped = isEquiped;

        protected void AttachItemToSlot(GameObject go, Transform slot)
        {
            go.transform.SetParent(slot);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (slot.GetComponent<InventoryContentCoordinate>() != null)
                SetItemEquipedState(false);
            else
                SetItemEquipedState(true);
        }
    }
}