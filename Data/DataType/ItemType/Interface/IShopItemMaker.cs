using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.SubItem;
using UnityEngine;

namespace Data.DataType.ItemType.Interface
{
    public interface IShopItemMaker
    {
        public UIShopItemComponent MakeShopItemComponent(IUIManagerServices uiManagerServices,int itemPrice, Transform parent = null, int itemCount = 1,
            string name = null, string path = null);
    }
}