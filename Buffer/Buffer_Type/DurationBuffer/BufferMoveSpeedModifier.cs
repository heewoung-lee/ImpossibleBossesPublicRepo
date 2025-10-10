using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using Stats.BaseStats;
using UnityEngine;
using Zenject;

namespace Buffer.Buffer_Type.DurationBuffer
{
    public class BufferMoveSpeedModifier : DurationBuff
    {
        [Inject]private IResourcesServices _resourcesServices;

        private Sprite _iconImage;

        public override Sprite BuffIconImage
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

        public override string Buffname => "이동속도증가";

        public override StatType StatType => StatType.MoveSpeed;

        public override void ApplyStats(BaseStats stats, float value)
        {
            stats.Plus_MoveSpeed_Abillity((int)value);

        }
        public override void RemoveStats(BaseStats stats, float value)
        {
            stats.Plus_MoveSpeed_Abillity(-(int)value);

        }

        public override void SetBuffIconImage(Sprite buffImageIcon)
        {
            _iconImage = buffImageIcon;
        }
    }
}