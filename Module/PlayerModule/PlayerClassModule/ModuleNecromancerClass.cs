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
    public class ModuleNecromancerClass : ModulePlayerClass
    {
        private static readonly int NecromancerVictoryAnimHash = Animator.StringToHash("Victory");
        private const string NecromancerAttackCueId = "NecromancerAttack";
        private const string NecromancerCastingCueId = "NecromancerCastingSFX";
        private const string NecromancerSkillScratch1CueId = "NecromancerSkillScratch1SFX";
        private const string NecromancerSkillScratch2CueId = "NecromancerSkillScratch2SFX";
        private const string NecromancerSkillScratch3CueId = "NecromancerSkillScratch3SFX";
        private IAllData _allData;
        private Dictionary<int, NecromancerStat> _originData;
        private const float BackAttackAngle = 120f;
        private const float BackAttackMultiplier = 1.5f;
        private SoundPlayerBinder _soundPlayerBinder;
            
        [Inject]
        public void Construct(IAllData allData)
        {
            _allData = allData;
            _originData = _allData.GetData(typeof(NecromancerStat)) as Dictionary<int, NecromancerStat>;
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Necromancer;
        public override int VictoryAnimHash => NecromancerVictoryAnimHash;

        protected override void InitOnAwake()
        {
            _soundPlayerBinder = GetComponent<SoundPlayerBinder>();
        }
        
        
        #region AnimationClipMethod

        public void AttackEvent()
        {
            _soundPlayerBinder.PlayDetached(NecromancerAttackCueId);

            if (IsOwner == false) return;
            TargetInSight.AttackTargetInSector(Stats, -1, CalculateBackAttackMultiplier);
        }

        public void NecromancerCastingSfxEvent()
        {
            _soundPlayerBinder.PlayDetached(NecromancerCastingCueId);
        }

        public void NecromancerSkillScratch1SfxEvent()
        {
            _soundPlayerBinder.PlayDetached(NecromancerSkillScratch1CueId);
        }

        public void NecromancerSkillScratch2SfxEvent()
        {
            _soundPlayerBinder.PlayDetached(NecromancerSkillScratch2CueId);
        }

        public void NecromancerSkillScratch3SfxEvent()
        {
            _soundPlayerBinder.PlayDetached(NecromancerSkillScratch3CueId);
        }

        private float CalculateBackAttackMultiplier(Transform attacker, Transform victim)
        {
            if (TargetInSight.IsBackAttack(attacker, victim, BackAttackAngle))
            {
                return BackAttackMultiplier;
            }
            return 1.0f;
        }

        #endregion
    }
}
