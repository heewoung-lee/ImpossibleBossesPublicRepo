using Data.DataType.ItemType.Interface;
using Stats.BaseStats;

namespace Buffer.Buffer_Type.ImmediatelyBuffer
{
    public class HpPosition
    {
        public string Buffname => "체력회복";

        public StatType StatType => StatType.CurrentHp;

        public void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_Current_Hp_Abillity((int)value);
        }
    }
}