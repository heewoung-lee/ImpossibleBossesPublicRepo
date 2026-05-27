using DataType.Item;
using DataType.Item.Consumable;
using DataType.Item.Equipment;
using GameManagers.ResourcesExManagement;
using NetWork.Item;
using UnityEngine;
using Zenject;

namespace GameManagers.ItemDataManagement
{
    public class LootItemFactory
    {
        private IResourcesServices _resourcesServices;

        [Inject]
        public LootItemFactory(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        public GameObject CreateLootItem(ItemDataSO data, Vector3 position)
        {
            string prefabPath = GetPrefabPathByData(data);
            GameObject lootObj = _resourcesServices.InstantiateByKey(prefabPath);

            if (lootObj.TryGetComponent(out LootItem lootItemScript))
            {
                lootItemScript.Initialize(data); 
            }

            return lootObj;
        }

        private string GetPrefabPathByData(ItemDataSO data)
        {
            if (data is CoinItemSO)
            {
                return "Prefabs/NGO/LootingItem/Coin";
            }

            // 장비 아이템인 경우
            if (data is EquipmentItemSO equipData)
            {
                switch (equipData.slotType)
                {
                    case EquipmentSlotType.Weapon:
                        return "Prefabs/NGO/LootingItem/Sword";
                    default:
                        return "Prefabs/NGO/LootingItem/Shield";
                }
            }
            // 소비 아이템인 경우
            else if (data is ConsumableItemSO)
            {
                return "Prefabs/NGO/LootingItem/Potion";
            }
            return "Prefabs/NGO/LootingItem/Bag";
        }
    }
}
