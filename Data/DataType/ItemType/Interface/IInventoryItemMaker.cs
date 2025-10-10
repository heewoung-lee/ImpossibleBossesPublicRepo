using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.SubItem;
using UnityEngine;

namespace Data.DataType.ItemType.Interface
{
    public interface IInventoryItemMaker
    {
        public UIItemComponentInventory MakeItemComponentInventory(IUIManagerServices uiManagerServices,Transform parent = null, int itemCount = 1, string name = null, string path = null);
    }
}
