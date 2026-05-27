using System.Collections.Generic;
using System.Linq;
using Data.DataType.StatType;
using GameManagers.DataManagement;
using GameManagers.SoundManagement;
using Stats;
using UnityEngine;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModuleMonkClass : ModulePlayerClass
    {
        private static readonly int MonkVictoryAnimHash = Animator.StringToHash("Victory");
        private const string MonkAttackCueId = "MonkAttack";
        private const string HolyShieldCueId = "HolyShieldSFX";
        private const string KnockBackCueId = "KnockBackSFX";

        private IAllData _allData;
        private Dictionary<int, MonkStat> _originData;
        private SoundPlayerBinder _soundPlayerBinder;

        [Inject]
        public void Construct(IAllData allData)
        {
            _allData = allData;
            _originData = _allData.GetData(typeof(MonkStat)) as Dictionary<int, MonkStat>;
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Monk;
        public override int VictoryAnimHash => MonkVictoryAnimHash;

        protected override void InitOnAwake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }

        public void HolyShieldSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(HolyShieldCueId);
        }

        public void MonkAttackSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(MonkAttackCueId);
        }

        public void KnockBackSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(KnockBackCueId);
        }
    }
}
