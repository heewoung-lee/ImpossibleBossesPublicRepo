using Data.DataType.StatType;
using GameManagers.SoundManagement;
using Stats.BaseStats;
using UnityEngine;
using Util;
using Zenject;

namespace Stats.MonsterStats
{
    public class BomberStats : MonsterStats, IAttackRange
    {
        private const int MonsterBomberID = 2;
        private const string BomberDeadCueId = "MinionBomberDeadSFX";

        private int _exp;
        private LayerMask _targetLayer;
        
        [Header("Move")]
        [SerializeField] private float _turnSpeed = 6f;
        [SerializeField] private float _bombRange = 1.5f;

        [Header("Catch Up")]
        [SerializeField] private float _catchUpDuration = 0.2f;
        [SerializeField] private float _maxCatchUpMultiplier = 3f;

        [Header("Explosion")]
        [SerializeField] private float _flashDuration = 0.3f;
        [SerializeField] private float _explosionRadius = 2.5f;
        [SerializeField] private float _pushDistance = 2f;
        [SerializeField] private float _pushDuration = 0.25f;
        
        
        public float TurnSpeed => _turnSpeed;
        public float BombRange => _bombRange;
        public float CatchUpDuration => _catchUpDuration;
        public float MaxCatchUpMultiplier => _maxCatchUpMultiplier;
        public float FlashDuration => _flashDuration;
        public float ExplosionRadius => _explosionRadius;
        public float PushDistance => _pushDistance;
        public float PushDuration => _pushDuration;

        public float ViewAngle => 0f;
        public float ViewDistance => 0f;
        public Transform OwnerTransform => transform;
        public Vector3 AttackPosition => transform.position;
        public LayerMask TarGetLayer => _targetLayer;
        

        protected override void SetStats()
        {
            MonsterStat stat = _statDict[MonsterBomberID];
            CharacterBaseStat basestat = new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence,stat.speed);
            SetPlayerBaseStatRpc(basestat);
            _exp = stat.exp;
        }

        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
            {
                soundPlayerBinder.PlayDetached(BomberDeadCueId);
            }

            if (attacker.TryGetComponent(out PlayerStats playerStat))
            {
                playerStat.AddExpFromMonster(_exp);
            }
        }

        public void ResetForPoolDespawn()
        {
            MonsterStat stat = _statDict[MonsterBomberID];
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
