using System.Collections.Generic;
using Data.DataType.ItemType;
using Data.Item;
using DataType.Item.Equipment;
using UnityEngine.EventSystems;
using Zenject;
using Data.Item.EquipSlot; 
using UnityEngine;
using Util; // GameObject

namespace UI.SubItem
{
    public class UIItemComponentEquipment : UIItemComponentInventory
    {
        private const string EquipSoundCueId = "EquipSFX";
        private const string WearingOffSoundCueId = "WearingOffSFX";

        public override void ItemRightClick(PointerEventData eventdata)
        {
            base.ItemRightClick(eventdata);
            EquipItem();
        }

        public void EquipItem()
        {
            if (!(_itemData is EquipmentItemSO equipData)) return;

            if (IsEquipped == false) 
            {
                EquipItemToSlot(equipData.slotType);
            }
            else
            {
                MoveToInventoryAfterUnEquip(_contentofInventoryTr);
            }
        }

        public void EquipItemToSlot(EquipmentSlotType eqiupSlot)
        {
            EquipMentSlot slot = null;
            
            // _equipSlot은 부모(UIItemComponentInventory)에 정의된 _inventoryUI에서 가져와야 함.
            // 만약 _equipSlot 참조가 없다면 아래 로직을 통해 가져오세요.
            // (보통 UIPlayerInventory가 가지고 있음)
            if (_equipSlot == null)
            {
                // UIPlayerInventory를 찾아 _equipSlot 접근
                // (기존 코드에 _equipSlot이 protected로 선언되어 있다면 접근 가능)
            }

            switch (eqiupSlot)
            {
                case EquipmentSlotType.Helmet: slot = _equipSlot.HelmetEquipMent; break;
                case EquipmentSlotType.Gauntlet: slot = _equipSlot.GauntletEquipMent; break;
                case EquipmentSlotType.Shoes: slot = _equipSlot.ShoesEquipMent; break;
                case EquipmentSlotType.Weapon: slot = _equipSlot.WeaponEquipMent; break;
                case EquipmentSlotType.Ring: slot = _equipSlot.RingEquipMent; break;
                case EquipmentSlotType.Armor: slot = _equipSlot.ArmorEquipMent; break;
            }

            if (slot != null)
            {
                slot.ItemEquip(this);
                AttachItemToSlot(gameObject, slot.transform);

                if (IsEquipped)
                {
                    _soundManagerServices.PlayUiSfx(gameObject, EquipSoundCueId);
                }
            }
        }

        protected override void DropItemOnUI(PointerEventData eventData, List<RaycastResult> uiraycastResult)
        {
            foreach (RaycastResult uiResult in uiraycastResult)
            {
                // [변경] SO 캐스팅
                if (uiResult.gameObject.CompareTag("EquipSlot") && _itemData is EquipmentItemSO equipData)
                {
                    EquipMentSlot slot = uiResult.gameObject.GetComponent<EquipMentSlot>();
                    if (slot.slotType == equipData.slotType)
                    {
                        EquipItemToSlot(equipData.slotType);
                    }
                }
                else if (uiResult.gameObject.TryGetComponentInChildren(out InventoryContentCoordinate contentTr) && IsEquipped == true)
                {
                    MoveToInventoryAfterUnEquip(contentTr.transform);
                }
            }
        }

        protected override void DropItemOnGround()
        {
            UnEquipItem();
            base.DropItemOnGround(); 
        }

        protected override void RemoveItemFromInventory()
        {
            _resourcesServices.DestroyObject(gameObject);
        }

        private void UnEquipItem(bool playSound = true)
        {
            if (IsEquipped == true)
            {
                GetComponentInParent<EquipMentSlot>().ItemUnEquip();
                SetItemEquipedState(false);

                if (playSound)
                {
                    _soundManagerServices.PlayUiSfx(gameObject, WearingOffSoundCueId);
                }
            }
        }

        public void MoveToInventoryAfterUnEquip(Transform inventoryTarget, bool playSound = true)
        {
            UnEquipItem(playSound);
            AttachItemToSlot(gameObject, inventoryTarget);
        }
    }
}
