using Data.DataType.ItemType.Interface;
using Stats.BaseStats;
using UnityEngine;

namespace Buffer.SkillBuffer
{
    public class BufferRoarModifier
    {
        public Sprite BuffIconImage => _iconImage;
        public string Buffname => "공격력증가";
        public StatType StatType => StatType.Attack;
        private Sprite _iconImage = null;

        public void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_Attack_Ability((int)value);
        }
        public void RemoveStats(BaseStats stats, float value)
        {
            stats.Plus_Attack_Ability(-(int)value);
        }

        public void SetBuffIconImage(Sprite buffImageIcon)
        {
            _iconImage = buffImageIcon;
        }
    }
}