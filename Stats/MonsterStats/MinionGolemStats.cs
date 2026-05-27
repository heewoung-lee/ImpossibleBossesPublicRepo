using Data.DataType.StatType;
using GameManagers.SoundManagement;
using Stats.BaseStats;
using UnityEngine;
using Util;

namespace Stats.MonsterStats
{
    public class MinionGolemStats : MonsterStats, IAttackRange
    {
        private const int MonsterMinionGolemID = 3;
        private const string MinionGolemDeadCueId = "MinionGolemDeadSFX";

        private int _exp;
        private LayerMask _targetLayer;

        [Header("Move")]
        [SerializeField] private float _turnSpeed = 6f;
        [SerializeField] private float _idleDuration = 0.25f;
        [SerializeField] private float _minPatrolDuration = 1.5f;
        [SerializeField] private float _maxPatrolDuration = 3f;
        [SerializeField] private float _patrolDistance = 4f;
        [SerializeField] private float _navMeshSampleDistance = 1.5f;

        [Header("Catch Up")]
        [SerializeField] private float _catchUpDuration = 0.2f;
        [SerializeField] private float _maxCatchUpMultiplier = 3f;

        [Header("Attack")]
        [SerializeField] private float _fireInterval = 0.8f;
        [SerializeField] private int _projectileCountPerShot = 8;
        [SerializeField] private float _projectileSpawnDistance = 1.5f;
        [SerializeField] private float _projectileSpawnHeight = 0.5f;

        public float TurnSpeed => _turnSpeed;
        public float IdleDuration => _idleDuration;
        public float MinPatrolDuration => _minPatrolDuration;
        public float MaxPatrolDuration => _maxPatrolDuration;
        public float PatrolDistance => _patrolDistance;
        public float NavMeshSampleDistance => _navMeshSampleDistance;
        public float CatchUpDuration => _catchUpDuration;
        public float MaxCatchUpMultiplier => _maxCatchUpMultiplier;
        public float FireInterval => _fireInterval;
        public int ProjectileCountPerShot => Mathf.Max(1, _projectileCountPerShot);
        public float ProjectileSpawnDistance => _projectileSpawnDistance;
        public float ProjectileSpawnHeight => _projectileSpawnHeight;
        public float ViewAngle => 0f;
        public float ViewDistance => 0f;
        public Transform OwnerTransform => transform;
        public Vector3 AttackPosition => transform.position;
        public LayerMask TarGetLayer => _targetLayer;

        protected override void SetStats()
        {
            MonsterStat stat = _statDict[MonsterMinionGolemID];
            CharacterBaseStat baseStat =
                new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);

            SetPlayerBaseStatRpc(baseStat);
            _exp = stat.exp;
        }

        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(MinionGolemDeadCueId);
            }

            if (attacker.TryGetComponent(out PlayerStats playerStat))
            {
                playerStat.AddExpFromMonster(_exp);
            }
        }

        public void ResetForPoolDespawn()
        {
            MonsterStat stat = _statDict[MonsterMinionGolemID];
            CharacterBaseStat baseStat =
                new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);

            ResetStatsForPool(baseStat, false);
        }

        protected override void StartInit()
        {
            base.StartInit();
            _targetLayer = LayerMask.GetMask(
                Utill.GetLayerID(Define.ControllerLayer.Player),
                Utill.GetLayerID(Define.ControllerLayer.AnotherPlayer));
            UpdateStat();
        }
    }
}
