using Stats.BaseStats;

namespace DataType.Item.Equipment
{
    public interface IEquippable
    {
        public void Equip(BaseStats stats, BaseDataSO data);
        public void UnEquip(BaseStats stats, BaseDataSO data);
    }
}