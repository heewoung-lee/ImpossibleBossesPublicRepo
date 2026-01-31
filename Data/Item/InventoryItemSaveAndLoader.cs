using System;
using System.Collections.Generic;
using DataType.Item;
using GameManagers.ItamData.Interface;
using GameManagers.ItamDataManager.Interface;
using GameManagers.Scene;
using UI.Popup.PopupUI;
using UI.SubItem;
using UnityEngine;
using Zenject;

namespace Data.Item
{
    public class InventoryItemSaveAndLoader : MonoBehaviour
    {
         private SceneDataSaveAndLoader _sceneDataSaveAndLoader;
         private IItemDataManager _itemDataManager;
        
        private UIPlayerInventory _uiPlayerInventory;
        [Inject]
        private void Construct(SceneDataSaveAndLoader sceneDataSaveAndLoader, IItemDataManager itemDataManager)
        {
            _sceneDataSaveAndLoader = sceneDataSaveAndLoader;
            _itemDataManager = itemDataManager;
        }


        private void Awake()
        {
            _uiPlayerInventory = GetComponent<UIPlayerInventory>();
            
            if (_uiPlayerInventory == null)
            {
                _uiPlayerInventory = GetComponentInParent<UIPlayerInventory>();
            }
        }

        private void OnDestroy()
        {
            List<IteminfoStruct> saveList = new List<IteminfoStruct>();
            
            foreach (UIItemComponentInventory item in GetComponentsInChildren<UIItemComponentInventory>())
            {
                saveList.Add(new IteminfoStruct(item.ItemNumber));
            }
            
            _sceneDataSaveAndLoader.SaveInventoryItem(saveList);
        }
        // 새 씬이 시작될 때 로드
        private void Start()
        {
            //장착된 데이터(ID 리스트) 가져오기
            if (_sceneDataSaveAndLoader.TryGetLoadInventoryItem(out List<IteminfoStruct> savedList))
            {
                //각 ID를 순회하며 복구
                foreach (var info in savedList)
                {
                    // ID로 원본 데이터(SO) 찾기
                    if (_itemDataManager.TryGetItemData(info.ItemNumber, out ItemDataSO data))
                    {
                        // UIPlayerInventory에게 생성 요청
                        _uiPlayerInventory.AddItem(data);
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveLoader] 저장된 아이템 ID {info.ItemNumber}에 해당하는 데이터를 찾을 수 없습니다.");
                    }
                }
            }
        }
    }
}