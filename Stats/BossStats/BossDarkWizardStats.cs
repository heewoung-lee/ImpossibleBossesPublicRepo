using System;
using BehaviorDesigner.Runtime;
using Controller.BossState.BossDarkWizard;
using Controller.ControllerStats;
using Controller.ControllerStats.BaseStates;
using Data.DataType.StatType;
using GameManagers.SoundManagement;
using Util;

namespace Stats.BossStats
{
    
    public class BossDarkWizardStats : BossStats
    {
        private const string DarkWizardDeadCueId = "DarkWizardDeadSFX";
        private int _bossID;
        private BossDarkWizardController _controller;
        protected override void StartInit()
        {
            base.StartInit();
            _controller = GetComponent<BossDarkWizardController>();
            UpdateStat();
        }

        private void OnEnable()
        {
            EventAttacked += OnHitEvent;
        }

        private void OnDisable()
        {
            EventAttacked -= OnHitEvent;
        }

        protected override void SetStats()
        {
            _bossID = (int)Define.BossID.DarkWizard;
            BossStat stat = _statDict[_bossID];
            MaxHp = stat.hp;
            Hp = stat.hp;
            Attack = stat.attack;
            Defence = stat.defence;
            MoveSpeed = stat.speed;
            _viewAngle = stat.viewAngle;
            _viewDistance = stat.viewDistance;
        }

        private void OnHitEvent(int damage, int currentHp) //피격 애니메이션 실행
        {
            _controller.UpdateStateHit();
        }
        
        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(DarkWizardDeadCueId);
            }

            _controller.CurrentStateType = _controller.BaseDieState;
            GetComponent<BehaviorTree>().SendEvent("BossDeadEvent");
        }
        
        
    }
}
