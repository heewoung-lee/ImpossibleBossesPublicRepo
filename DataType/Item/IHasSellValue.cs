namespace DataType.Item
{
    // DropItems 노드에서 전리품 후보를 명시적으로 드롭 가능한 아이템 데이터로 제한할 때 사용
    public interface ICanDrop
    {
    }

    // UIShop.CreateSellValueShopItems에서 상점 목록 생성 시 아이템 판매 가격을 가져올 때 사용
    public interface IHasSellValue
    {
        int SellValue { get; }
    }
}
