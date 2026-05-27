using BehaviorDesigner.Runtime.Tasks;
using BehaviourTreeNode.BossGolem.Task;
using Controller.BossState.BossDarkWizard;
using Data;
using Enemy.Boss.Darkwizard;
using NetWork;
using Stats.BossStats;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.DarkWizard.Task
{
    [TaskCategory("CustomNode/DarkWizard")]
    public class BossDarkWizardSectorFanAttack : Action
    {
        private enum AttackPhase
        {
            FireFanShots,
            ResumeAnimation
        }

        private const string SectorAttackPath = "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttack";
        private const string SectorAttackMuzzlePath = "Prefabs/Enemy/Boss/AttackPattern/DarkWizard/DarkWizardSectorAttackMuzzle";

        [SerializeField] private int _projectileCountPerShot = 10;
        [SerializeField] private int _fanShotCount = 3;
        [SerializeField] private float _shotInterval = 0.4f;
        [SerializeField] private float _totalSpreadAngle = 60f;
        [SerializeField] private float _projectileLifetime = 3f;

        private BossDependencyHub _bossDependencyHub;
        private BossDarkWizardController _controller;
        private BossDarkWizardStats _attackerStats;
        private DarkWizardAttackPosition _attackPosition;

        private readonly int _attackAnimHash = BossDarkWizardAnimHash.DarkWizardSlashAttack;

        private AttackPhase _attackPhase;
        private float _elapsedShotTime;
        private int _firedFanShotCount;

        private BossDependencyHub BossDependencyHub
        {
            get
            {
                if (_bossDependencyHub == null)
                {
                    _bossDependencyHub = GetComponent<BossDependencyHub>();
                }

                return _bossDependencyHub;
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossDarkWizardController>();
            _attackerStats = Owner.GetComponent<BossDarkWizardStats>();
            _attackPosition = Owner.GetComponentInChildren<DarkWizardAttackPosition>();
        }

        public override void OnStart()
        {
            base.OnStart();

            _elapsedShotTime = 0f;
            _firedFanShotCount = 0;
            _attackPhase = AttackPhase.FireFanShots;
            _controller.Anim.speed = 1f;
            _controller.UpdateSlashHit();
            FireSectorShot();
        }

        public override TaskStatus OnUpdate()
        {
            switch (_attackPhase)
            {
                case AttackPhase.FireFanShots:
                    return UpdateFireFanShots();
                case AttackPhase.ResumeAnimation:
                    return UpdateResumeAnimation();
            }

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _controller.Anim.speed = 1f;
            _controller.CurrentStateType = _controller.BaseIDleState;
        }

        private TaskStatus UpdateFireFanShots()
        {
            if (_firedFanShotCount >= _fanShotCount)
            {
                _controller.Anim.speed = 1f;
                _attackPhase = AttackPhase.ResumeAnimation;
                return TaskStatus.Running;
            }

            _elapsedShotTime += Time.deltaTime;
            if (_elapsedShotTime < _shotInterval)
            {
                return TaskStatus.Running;
            }

            _elapsedShotTime = 0f;
            FireSectorShot();
            return TaskStatus.Running;
        }

        private TaskStatus UpdateResumeAnimation()
        {
            _controller.Anim.speed = 1f;
            return _controller.IsAnimationDone(_attackAnimHash) ? TaskStatus.Success : TaskStatus.Running;
        }

        private void FireSectorShot()
        {
            if (_attackPosition == null)
            {
                UtilDebug.LogWarning("DarkWizardAttackPosition is missing.");
                return;
            }

            BossDependencyHub.VfxManagerServices.InstantiateParticleInArea(
                SectorAttackMuzzlePath,
                _attackPosition.transform.position);

            float angleStep = _projectileCountPerShot <= 1 ? 0f : _totalSpreadAngle / (_projectileCountPerShot - 1);
            float startAngle = _projectileCountPerShot <= 1 ? 0f : -_totalSpreadAngle * 0.5f;

            for (int i = 0; i < _projectileCountPerShot; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 baseForward = _attackPosition.transform.forward;
                baseForward.y = 0f;
                if (baseForward.sqrMagnitude <= 0f)
                {
                    baseForward = Vector3.forward;
                }
                else
                {
                    baseForward.Normalize();
                }

                Vector3 fireDirection = Quaternion.Euler(0f, currentAngle, 0f) * baseForward;
                BossDependencyHub.VfxManagerServices.InstantiateParticleInArea(
                    SectorAttackPath,
                    _attackPosition.transform.position,
                    _projectileLifetime,
                    networkParams: new NetworkParams(
                        argFloat: (float)BossDependencyHub.RelayManager.NetworkManagerEx.ServerTime.Time,
                        argPosVector3: fireDirection,
                        argUlong: _attackerStats == null ? ulong.MaxValue : _attackerStats.NetworkObjectId));
            }

            _firedFanShotCount++;
        }
    }
}
