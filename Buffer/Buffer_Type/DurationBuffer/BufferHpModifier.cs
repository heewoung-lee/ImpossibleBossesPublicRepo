using Data.DataType.ItemType.Interface;
using Stats.BaseStats;
using UnityEngine;

namespace Buffer.Buffer_Type.DurationBuffer
{
    public class BufferHpModifier
    {
        private Sprite _iconImage = null;
        public Sprite BuffIconImage => _iconImage;
        public string Buffname => "체력증가";
        public StatType StatType => StatType.CurrentHp;
        public void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_Current_Hp_Abillity((int)value);
        }
        public void RemoveStats(BaseStats stats, float value)
        {
            stats.Plus_Current_Hp_Abillity(-(int)value);
        }

        public void SetBuffIconImage(Sprite buffImageIcon)
        {
            _iconImage = buffImageIcon;
        }
    }
}