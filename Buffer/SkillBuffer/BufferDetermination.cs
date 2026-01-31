using Data.DataType.ItemType.Interface;
using Stats.BaseStats;
using UnityEngine;

namespace Buffer.SkillBuffer
{
    public class BufferDetermination
    {
        public Sprite BuffIconImage => _iconImage;
        public string Buffname => "방어력증가";
        public StatType StatType => StatType.Defence;
        private Sprite _iconImage = null;

        public void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_Defence_Abillity((int)value);
        }
        public void RemoveStats(BaseStats stats, float value)
        {
            stats.Plus_Defence_Abillity(-(int)value);
        }

        public void SetBuffIconImage(Sprite buffImageIcon)
        {
            _iconImage = buffImageIcon;
        }
    }
}