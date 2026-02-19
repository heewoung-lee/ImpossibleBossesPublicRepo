using System.Collections.Generic;
using System.Linq;
using DataType;
using DataType.Item;
using GameManagers.ItamData.Interface;
using GameManagers.ItamDataManager.Interface;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.ItamData
{
    public class ItemDataManager : IItemDataManager
    {
        // 검색 속도를 위해 Dictionary에 캐싱
        private Dictionary<int, ItemDataSO> _itemDatabase = new Dictionary<int, ItemDataSO>();

        [Inject]
        public ItemDataManager()
        {
            LoadAllItemData();
        }

        private void LoadAllItemData()
        {
            // Resources/Data/Item 폴더의 모든 SO 로드
            ItemDataSO[] loadedItems = UnityEngine.Resources.LoadAll<ItemDataSO>("SOData");

            foreach (var item in loadedItems)
            {
                if (_itemDatabase.ContainsKey(item.itemNumber))
                {
                    UtilDebug.LogWarning($"중복 ID 발견: {item.itemNumber} ({item.name})");
                    continue;
                }
                _itemDatabase.Add(item.itemNumber, item);
            }
            UtilDebug.Log($"[ItemDataManager] SO 데이터 {loadedItems.Length}개 로드 완료.");
        }

        public bool TryGetItemData(int itemNumber, out ItemDataSO itemData)
        {
            return _itemDatabase.TryGetValue(itemNumber, out itemData);
        }
        
        public ItemDataSO GetRandomItemData()
        {
            if (_itemDatabase == null || _itemDatabase.Count == 0)
            {
                UtilDebug.LogWarning("[ItemDataManager] 등록된 아이템 데이터가 없습니다.");
                return null;
            }

            List<ItemDataSO> allItems = _itemDatabase.Values.ToList();
            int randomIndex = Random.Range(0, allItems.Count);
            
            return allItems[randomIndex];
        }

        public ItemDataSO GetRandomItemData(ItemType type)
        {
            List<ItemDataSO> filteredList = _itemDatabase.Values.Where(x => x.ItemType == type).ToList();
    
            if (filteredList.Count == 0) return null;
    
            return filteredList[Random.Range(0, filteredList.Count)];
        }
    }
}