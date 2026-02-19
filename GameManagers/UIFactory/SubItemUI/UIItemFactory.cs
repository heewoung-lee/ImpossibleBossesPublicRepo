using DataType;
using DataType.Item;
using UI.SubItem;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers.UIFactory.SubItemUI
{
    public interface IUIItemFactory
    {
        UIItemComponentInventory CreateItemUI(ItemDataSO data, Transform parent, int count = 1);
    }

    public class UIItemFactory : IUIItemFactory
    {
        private readonly IUIManagerServices _uiManager;

        private const string PathEquip = "Prefabs/UI/Item/UI_ItemComponent_Equipment";
        private const string PathConsumable = "Prefabs/UI/Item/UI_ItemComponent_Consumable";

        [Inject]
        public UIItemFactory(IUIManagerServices uiManager)
        {
            _uiManager = uiManager;
        }
        // 스위치문을 쓴 이유는 앞으로도 아이템종류가 2개 밖에 안될 것 같아서 Switch문 씀.
        // 타입별로 받고 그 타입이 만들어준 컴포넌트를 뱉는 전략패턴 방식을 생각했지만,
        // 너무 비대해지고, 유지보수보다. 보는게 안좋아질것 같아 스위치문으로 만듦
        public UIItemComponentInventory CreateItemUI(ItemDataSO data, Transform parent, int count = 1)
        {
            
            if (data == null) return null;

            UIItemComponentInventory createdItem = null;

            switch (data.ItemType)
            {
                case ItemType.Equipment:
                    // 장비 생성
                    var equipItem = _uiManager.MakeSubItem<UIItemComponentEquipment>(path: PathEquip, parent: parent);
                    equipItem.InitializeItem(data); // 장비는 개수 불필요
                    createdItem = equipItem;
                    break;

                case ItemType.Consumable:
                    // 소비 아이템 생성
                    var consumeItem = _uiManager.MakeSubItem<UIItemComponentConsumable>(path: PathConsumable, parent: parent);
                    consumeItem.InitializeItem(data, count); // 소비는 개수 필요
                    createdItem = consumeItem;
                    break;

                case ItemType.ETC:
                    UtilDebug.LogWarning("[UIItemFactory] ETC 아이템은 아직 구현되지 않았습니다.");
                    break;
            }

            return createdItem;
        }
    }
}