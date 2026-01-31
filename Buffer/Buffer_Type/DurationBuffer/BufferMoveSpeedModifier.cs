using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using UnityEngine;
using Zenject;

namespace Buffer.Buffer_Type.DurationBuffer
{
    public class BufferMoveSpeedModifier
    {
        [Inject]private IResourcesServices _resourcesServices;

        private Sprite _iconImage;

        public Sprite BuffIconImage
        {
            get
            {
                if (_iconImage == null)
                {
                    _iconImage = _resourcesServices.Load<Sprite>("Art/UI/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/IconMisc/IconSet_Equip_Boots");
                }
                return _iconImage;
            }
        }

        public string Buffname => "이동속도증가";

        public StatType StatType => StatType.MoveSpeed;

        public void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_MoveSpeed_Abillity((int)value);

        }
        public void RemoveStats(BaseStats stats, float value)
        {
            stats.Plus_MoveSpeed_Abillity(-(int)value);

        }

        public void SetBuffIconImage(Sprite buffImageIcon)
        {
            _iconImage = buffImageIcon;
        }
    }
}