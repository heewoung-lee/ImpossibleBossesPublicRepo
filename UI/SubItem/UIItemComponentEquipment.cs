using System.Collections.Generic;
using Data.DataType.ItemType;
using Data.DataType.ItemType.Interface;
using Data.Item;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.LootItem;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;
using Zenject;

namespace UI.SubItem
{
    public class UIItemComponentEquipment : UIItemComponentInventory
    {
        [Inject] private IResourcesServices _instantiate;
        [Inject] private LootItemManager _lootItemManager;
        
        public override void ItemRightClick(PointerEventData eventdata)
        {
            base.ItemRightClick(eventdata);
            //장착중이 아니라면 슬롯에 넣고, 능력치 적용
            //장착중이라면 아이템창에 돌려놓고, 능력치 감소
            EquipItem();
        }


        public void EquipItem()
        {
            if (IsEquipped == false) // 아이템이 장착중이 아니라면 장착하는 로직 수행
            {
                ItemEquipment equipment = _iteminfo as ItemEquipment;
                EquipmentSlotType eqiupSlot = equipment.EquipmentSlotType;
                EquipItemToSlot(eqiupSlot);
            }
            else// 장착중이라면 장착해제
            {
                GetComponentInParent<EquipMentSlot>().ItemUnEquip();
                AttachItemToSlot(gameObject, _contentofInventoryTr);
            }
        }

        public void EquipItemToSlot(EquipmentSlotType eqiupSlot)
        {
            EquipMentSlot slot = null;

            switch (eqiupSlot)
            {
                case EquipmentSlotType.Helmet:
                    slot = _equipSlot.HelmetEquipMent;
                    break;
                case EquipmentSlotType.Gauntlet:
                    slot = _equipSlot.GauntletEquipMent;
                    break;
                case EquipmentSlotType.Shoes:
                    slot = _equipSlot.ShoesEquipMent;
                    break;
                case EquipmentSlotType.Weapon:
                    slot = _equipSlot.WeaponEquipMent;
                    break;
                case EquipmentSlotType.Ring:
                    slot = _equipSlot.RingEquipMent;
                    break;
                case EquipmentSlotType.Armor:
                    slot = _equipSlot.ArmorEquipMent;
                    break;
            }
            slot.ItemEquip(this);
            AttachItemToSlot(gameObject, slot.transform);
        }

        protected override void DropItemOnUI(PointerEventData eventData, List<RaycastResult> uiraycastResult)
        {

            foreach (RaycastResult uiResult in uiraycastResult)
            {
                if (uiResult.gameObject.CompareTag("EquipSlot") && _iteminfo is ItemEquipment)
                {
                    EquipMentSlot slot = uiResult.gameObject.GetComponent<EquipMentSlot>();
                    ItemEquipment equipment = _iteminfo as ItemEquipment;

                    if (slot.slotType == equipment.EquipmentSlotType)
                    {
                        EquipItemToSlot(equipment.EquipmentSlotType);
                    }
                }
                else if (uiResult.gameObject.TryGetComponentInChildren(out InventoryContentCoordinate contentTr) && IsEquipped == true)
                {
                    GetComponentInParent<EquipMentSlot>().ItemUnEquip();
                    AttachItemToSlot(gameObject, contentTr.transform);
                }
            }
        }


        protected override void DropItemOnGround()
        {
            base.DropItemOnGround();
            UnEquipItem();//장비한 아이템을 땅에 떨굴때 장비 벗음 효과 나오도록 수정
        }

        public override GameObject GetLootingItemObejct(IItem iteminfo)
        {
            GameObject lootItem;
            switch (((ItemEquipment)iteminfo).EquipmentSlotType)
            {
                case EquipmentSlotType.Helmet:
                case EquipmentSlotType.Armor:
                    lootItem = _instantiate.InstantiateByKey("Prefabs/LootingItem/Shield", _lootItemManager.ItemRoot);
                    break;
                case EquipmentSlotType.Weapon:
                    lootItem = _instantiate.InstantiateByKey("Prefabs/LootingItem/Sword", _lootItemManager.ItemRoot);
                    break;
                default:
                    lootItem = _instantiate.InstantiateByKey("Prefabs/LootingItem/Bag", _lootItemManager.ItemRoot);
                    break;
            }
            lootItem.GetComponent<LootItem>().SetIteminfo(iteminfo);
            return lootItem;
        }

        protected override void RemoveItemFromInventory()
        {
            _instantiate.DestroyObject(gameObject);
        }

        private void UnEquipItem()
        {
            if (IsEquipped == true)
            {
                GetComponentInParent<EquipMentSlot>().ItemUnEquip();
                SetItemEquipedState(false);
            }
        }
    }
}
