using System;
using System.Collections.Generic;
using System.Linq;
using Controller.PlayerState.FighterState;
using Data.DataType.StatType;
using GameManagers;
using GameManagers.DataManagement;
using GameManagers.SoundManagement;
using Stats;
using Stats.BaseStats;
using UnityEngine;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{


    public class ModuleFighterClass : ModulePlayerClass
    {
        private static readonly int FighterVictoryAnimHash = Animator.StringToHash("Victory");
        private const string FighterAttackSoundCueId = "FigherAttackSFX";
        private const string BuffSoundCueId = "BuffSFX";
        private const string SlashSfx1CueId = "SlashSFX1";
        private const string SlashSfx2CueId = "SlashSFX2";
        private const string SlashSfx3CueId = "SlashSFX3";
        private const string TauntSfxCueId = "TauntSFX";

        private IAllData _allData;
        private Dictionary<int, FighterStat> _originData;
        private SoundPlayerBinder _soundPlayerBinder;

        [Inject]
        public void Construct(IAllData allData)
        {
            _allData = allData;
            _originData = _allData.GetData(typeof(FighterStat)) as Dictionary<int, FighterStat>;
            //이 모듈이 파이터 클래스에 대한 스탯을 가져오도록 정의
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Fighter;
        public override int VictoryAnimHash => FighterVictoryAnimHash;

        protected override void InitOnAwake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }


        #region AnimationClipMethod

        public void AttackEvent()
        {
            _soundPlayerBinder.PlayDetached(FighterAttackSoundCueId);
            
            
            if (IsOwner == false) return;
            TargetInSight.AttackTargetInSector(Stats);
        }

        public void BuffSkillSfxEvent()
        {
            PlayDetachedSfx(BuffSoundCueId);
        }

        public void SlashSfx1Event()
        {
            PlayDetachedSfx(SlashSfx1CueId);
        }

        public void SlashSfx2Event()
        {
            PlayDetachedSfx(SlashSfx2CueId);
        }

        public void SlashSfx3Event()
        {
            PlayDetachedSfx(SlashSfx3CueId);
        }

        public void TauntSfxEvent()
        {
            PlayDetachedSfx(TauntSfxCueId);
        }

        #endregion

        private void PlayDetachedSfx(string cueId)
        {
            _soundPlayerBinder.PlayDetached(cueId);
        }
    }
}
