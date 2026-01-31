using Data.DataType.ItemType.Interface;
using Stats.BaseStats;
using UnityEngine;

namespace Buffer.Buffer_Type.DurationBuffer
{
    public class BufferMaxHpModifier 
    {

        private Sprite _iconImage = null;

        public Sprite BuffIconImage => _iconImage;

        public string Buffname => "최대체력증가";

        public StatType StatType => StatType.MaxHP;


        public void RemoveStats(BaseStats stats, float value)
        {
            stats.Plus_MaxHp_Abillity(-(int)value);
        }

        public void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_MaxHp_Abillity((int)value);
        }

        public void SetBuffIconImage(Sprite buffImageIcon)
        {
            _iconImage = buffImageIcon;
        }
    }
}