using Data.DataType.StatType;
using Stats.BaseStats;
using UnityEngine;
using Util;

namespace Stats.MonsterStats
{
    public class MinionRockStats : MonsterStats, IAttackRange
    {
        private const int MonsterMinionRockID = 5;

        private int _exp;
        private LayerMask _targetLayer;

        public float ViewAngle => 0f;
        public float ViewDistance => 0f;
        public Transform OwnerTransform => transform;
        public Vector3 AttackPosition => transform.position;
        public LayerMask TarGetLayer => _targetLayer;

        protected override void SetStats()
        {
            MonsterStat stat = _statDict[MonsterMinionRockID];
            CharacterBaseStat baseStat =
                new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);

            SetPlayerBaseStatRpc(baseStat);
            _exp = stat.exp;
        }

        protected override void StartInit()
        {
            base.StartInit();
            _targetLayer = LayerMask.GetMask(
                Utill.GetLayerID(Define.ControllerLayer.Player),
                Utill.GetLayerID(Define.ControllerLayer.AnotherPlayer));
            UpdateStat();
        }

        public void ResetForPoolDespawn()
        {
            MonsterStat stat = _statDict[MonsterMinionRockID];
            CharacterBaseStat baseStat =
                new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);

            ResetStatsForPool(baseStat, false);
        }

        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (attacker.TryGetComponent(out PlayerStats playerStat))
            {
                playerStat.AddExpFromMonster(_exp);
            }
        }
    }
}
