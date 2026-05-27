using System.Collections.Generic;
using Data.DataType.StatType;
using GameManagers.DataManagement;
using GameManagers.SoundManagement;
using GameManagers.VFXManagement;
using UnityEngine;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModuleMageClass : ModulePlayerClass
    {
        private static readonly int MageVictoryAnimHash = Animator.StringToHash("Victory");
        private const string MageAttackCueId = "MageAttack";
        private const string MageCastingCueId = "MageCastingSFX";

        private IAllData _allData;
        private Dictionary<int, MageStat> _originData;
        private IVFXManagerServices _vfxManagerServices;
        private SoundPlayerBinder _soundPlayerBinder;

        [Inject]
        public void Construct(IAllData allData, IVFXManagerServices vfxManagerServices)
        {
            _allData = allData;
            _vfxManagerServices = vfxManagerServices;
            _originData = _allData.GetData(typeof(MageStat)) as Dictionary<int, MageStat>;
            InitializeStatTable(_originData);
        }

        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Mage;
        public override int VictoryAnimHash => MageVictoryAnimHash;

        protected override void InitOnAwake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public void MageAttack()
        {
            _soundPlayerBinder.PlayDetached(MageAttackCueId);

            if (IsOwner == false) return;

            _vfxManagerServices.InstantiateParticleWithTarget("Prefabs/Player/VFX/Mage/MageAttack", transform);
            _vfxManagerServices.InstantiateParticleWithTarget("Prefabs/Player/VFX/Mage/MageAttackMuzzle", transform);
        }

        public void MageCastingSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(MageCastingCueId);
        }
    }
}
