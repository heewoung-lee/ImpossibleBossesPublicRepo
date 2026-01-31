using System.Collections.Generic;
using Data.Item;
using DataType.Item;
using DataType.Item.Consumable;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using TMPro;
using UI.Scene.SceneUI;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;
using Zenject;

namespace UI.SubItem
{
    public class UIItemComponentConsumable : UIItemComponentInventory
    {
        enum Texts { ItemCountText }
        private TMP_Text _itemCountText;
        private string _itemGuid;
        private UIConsumableBar _consumableBar;
        private int _itemCount;
        private float _duringbuff;

        public float DuringBuffTime => _duringbuff;
        public string ItemGuid => _itemGuid;
        
        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                if(_itemCountText != null) _itemCountText.text = _itemCount.ToString();
            }
        }

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_Text>(typeof(Texts));
            _itemCountText = Get<TMP_Text>((int)Texts.ItemCountText);
            _itemGuid = System.Guid.NewGuid().ToString(); 
        }

        protected override void StartInit()
        {
            base.StartInit();
            _consumableBar = _uiManagerServices.Get_Scene_UI<UIConsumableBar>();
            _itemCountText.text = $"{_itemCount}";
            CombineConsumableItems();
        }

        public override void InitializeItem(ItemDataSO data)
        {
            InitializeItem(data, 1);
        }

        public void InitializeItem(ItemDataSO data, int count)
        {
            base.InitializeItem(data); 
            ItemCount = count; 
            // 소비 아이템 데이터라면 지속시간 설정
            if (data is ConsumableItemSO consumableData)
            {
                _duringbuff = consumableData.duration;
            }
        }

        public bool CombineConsumableItems(Transform parentTr = null)
        {
            Transform searchingTr = parentTr;
            if (parentTr == null) searchingTr = gameObject.transform.parent;
            if (searchingTr == null) return false;

            foreach (Transform itemInInventory in searchingTr)
            {
                if (itemInInventory == transform) continue;

                if (itemInInventory.TryGetComponent(out UIItemComponentConsumable item))
                {
                    if (item.ItemGuid == _itemGuid) continue;

                    if (item.ItemNumber == ItemNumber)
                    {
                        item.ItemCount += _itemCount;
                        _resourcesServices.DestroyObject(gameObject);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void ItemRightClick(PointerEventData eventdata)
        {
            base.ItemRightClick(eventdata);
            if (IsEquipped == false) ConsumableItemEquip(this);
            else 
            {
                AttachItemToSlot(gameObject, _contentofInventoryTr);
                CombineConsumableItems();
            }
        }

        public void LoadToSlot(Transform parentTr)
        {
            AttachItemToSlot(gameObject, parentTr);
        }

        private void ConsumableItemEquip(UIItemComponentConsumable itemcomponent)
        {
            CloseDescription();
            if (_consumableBar == null) return;

            foreach (Transform parentTr in _consumableBar.FrameTrs)
            {
                if (CombineConsumableItems(parentTr)) return;
            }

            for (int i = 0; i < _consumableBar.FrameTrs.Length; i++)
            {
                if (_consumableBar.FrameTrs[i].childCount < 1)
                {
                    AttachItemToSlot(itemcomponent.gameObject, _consumableBar.FrameTrs[i].transform);
                    break;
                }
            }
        }

        protected override void DropItemOnUI(PointerEventData eventData, List<RaycastResult> uiraycastResult)
        {
            foreach (RaycastResult uiResult in uiraycastResult)
            {
                // [변경] IItem -> SO 타입 체크
                if (uiResult.gameObject.CompareTag("ConsumableSlot") && _itemData is ConsumableItemSO)
                {
                    foreach (Transform frameTr in _consumableBar.FrameTrs)
                    {
                        if (frameTr.gameObject.TryGetComponentInChildren(out UIItemComponentConsumable uiConsumableItem))
                        {
                            if (uiConsumableItem.ItemNumber != ItemNumber) continue;
                            CombineConsumableItems(frameTr); 
                            return; 
                        }
                    }

                    if (uiResult.gameObject.TryGetComponentInChildren(out UIItemComponentConsumable uiAlreadyitem)
                        && uiAlreadyitem.ItemNumber != ItemNumber)
                    {
                        AttachItemToSlot(uiAlreadyitem.gameObject, transform.parent);
                    }

                    AttachItemToSlot(gameObject, uiResult.gameObject.transform);
                    break;
                }
                else if (uiResult.gameObject.TryGetComponentInChildren(out InventoryContentCoordinate contextTr))
                {
                    AttachItemToSlot(gameObject, contextTr.transform);
                    CombineConsumableItems(); 
                }
            }
        }

        protected override void RemoveItemFromInventory()
        {
            ItemCount--;
            if (ItemCount <= 0) _resourcesServices.DestroyObject(gameObject);
        }
    }
}