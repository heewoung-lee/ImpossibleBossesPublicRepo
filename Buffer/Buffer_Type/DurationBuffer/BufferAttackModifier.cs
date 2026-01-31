using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using UnityEngine;
using Zenject;

namespace Buffer.Buffer_Type.DurationBuffer
{
    public class BufferAttackModifier
    {
        
        [Inject]private IResourcesServices _resourcesServices;

        private Sprite _iconImage;

        public Sprite BuffIconImage
        {
            get
            {
                if (_iconImage == null)
                {
                    _iconImage = _resourcesServices.Load<Sprite>("Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/IconMisc/IconSet_Equip_Sword");
                }
                return _iconImage;
            }
        }
        
        public string Buffname => "공격력증가";
        public StatType StatType => StatType.Attack;

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