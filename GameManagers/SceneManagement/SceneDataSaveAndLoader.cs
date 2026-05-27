using System.Collections.Generic;
using DataType.Item.Equipment;
using UI.SubItem;

namespace GameManagers.SceneManagement
{
    public class SceneDataSaveAndLoader 
    {
        private List<IteminfoStruct> _inventoryItemList = new List<IteminfoStruct>();
        private List<(int count,IteminfoStruct iteminfo)> _consumableItemList = new List<(int,IteminfoStruct)>();
        private Dictionary<EquipmentSlotType, IteminfoStruct> _equipmentSlotData = new Dictionary<EquipmentSlotType, IteminfoStruct>();

        public void Clear()
        {
            // 로비로 돌아온 뒤 새 방을 만들면 이전 플레이 세션의 임시 아이템 저장값이 이어지면 안 되므로 비운다.
            _inventoryItemList.Clear();
            _consumableItemList.Clear();
            _equipmentSlotData.Clear();
        }

        public void SaveConsumableItem(List<(int count,IteminfoStruct iteminfo)> saveItemList)
        {
            _consumableItemList.Clear();
            _consumableItemList = saveItemList;
        }
        
        public void SaveInventoryItem(List<IteminfoStruct> saveItemlist)
        {
            _inventoryItemList.Clear();
            _inventoryItemList.AddRange(saveItemlist);
        }

        public bool TryGetLoadInventoryItem(out List<IteminfoStruct> savedList)
        {
            savedList = null;
            if (_inventoryItemList == null || _inventoryItemList.Count <= 0)
                return false;

            savedList = new List<IteminfoStruct>(_inventoryItemList);
            _inventoryItemList.Clear(); 
            return true;
        }

        public bool TryGetLoadConsumableItem(out List<(int count,IteminfoStruct iteminfo)> savedConsumableList)
        {
            savedConsumableList = null;
            if (_consumableItemList == null || _consumableItemList.Count <= 0)
                return false;

            savedConsumableList = new List<(int count,IteminfoStruct iteminfo)>(_consumableItemList);
            _consumableItemList.Clear();
            return true;
        }
        

        public void SaveEquipMentData(EquipmentSlotType slotType, UIItemComponentInventory uiItem)
        {
            if(uiItem == null) return;
            IteminfoStruct info = new IteminfoStruct(uiItem.ItemNumber);
             _equipmentSlotData.Add(slotType, info);
        }

        public bool TryGetLoadEquipMentData(EquipmentSlotType equipMentType, out IteminfoStruct equipItem)
        {
            equipItem = default;
            if (_equipmentSlotData.TryGetValue(equipMentType, out IteminfoStruct iteminfo))
            {
                equipItem = iteminfo;
                _equipmentSlotData.Remove(equipMentType);
                return true;
            }
            return false;
        }
    }
}
