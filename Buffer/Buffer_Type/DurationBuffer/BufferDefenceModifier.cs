using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using UnityEngine;
using Zenject;

namespace Buffer.Buffer_Type.DurationBuffer
{
    public class BufferDefenceModifier 
    {
        
        [Inject]private IResourcesServices _resourcesServices;

        private Sprite _iconImage;

        public Sprite BuffIconImage
        {
            get
            {
                if (_iconImage == null)
                {
                    _iconImage = _resourcesServices.Load<Sprite>("Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/IconMisc/IconSet_Equip_Shield");
                }
                return _iconImage;
            }
        }
        
        public string Buffname => "방어력증가";

        public StatType StatType => StatType.Defence;


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