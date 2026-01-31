using DataType;
using DataType.Item;


namespace GameManagers.ItamData.Interface
{
    public interface IItemDataManager
    {
        /// <summary>
        /// 아이템 ID(Number)를 통해 SO 데이터를 찾습니다.
        /// </summary>
        /// <param name="itemNumber">찾을 아이템의 ID</param>
        /// <param name="itemData">결과를 담을 변수 (실패 시 null)</param>
        /// <returns>성공 여부</returns>
        bool TryGetItemData(int itemNumber, out ItemDataSO itemData);
        ItemDataSO GetRandomItemData();
        ItemDataSO GetRandomItemData(ItemType type);
    }
}