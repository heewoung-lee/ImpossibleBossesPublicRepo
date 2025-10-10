using System.Collections.Generic;
using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.SubItem;
using UnityEngine;
using Zenject;

namespace Data.Item
{
    public class InventoryItemSaveAndLoader : MonoBehaviour
    {
        //아이템 정보 가져오기
        List<IteminfoStruct> _inventoryItemList = new List<IteminfoStruct>();
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private SceneDataSaveAndLoader _sceneDataSaveAndLoader;

        private void OnDestroy()
        {
            foreach (UIItemComponentInventory item in GetComponentsInChildren<UIItemComponentInventory>())
            {
                _inventoryItemList.Add(new IteminfoStruct(item));
            }
            _sceneDataSaveAndLoader.SaveInventoryItem(_inventoryItemList);
        }

        private void Start()
        {
            if(_sceneDataSaveAndLoader.TryGetLoadInventoryItem(_uiManagerServices,out List<UIItemComponentInventory> loaditemList))
            {
                //씬 전환후 가져온 아이템들에 대한 후처리는 여기에 할것 
            }
        }

    }
}
