using Stats.BaseStats;
using Stats.BossStats;
using UnityEngine;
using Util;

namespace Module.EnemyModule.Boss.RedDragon
{
    [DisallowMultipleComponent]
    public sealed class RedDragonDamageRelay : MonoBehaviour, IDamageable
    {
        private BossRedDragonStats _damageGate;
        private BossRedDragonStats _rootStats;

        public float LastDamagedTime => _rootStats != null ? _rootStats.LastDamagedTime : float.MinValue;

        private void Awake()
        {
            _rootStats = GetComponentInParent<BossRedDragonStats>();
            _damageGate = _rootStats;

            if (_rootStats == null)
            {
                UtilDebug.LogError($"[{nameof(RedDragonDamageRelay)}] BossRedDragonStats not found in parents of {gameObject.name}");
            }
        }

        public void OnAttacked(IAttackRange attacker, int damage = -1)
        {
            if (_rootStats == null)
            {
                return;
            }

            if (_damageGate != null && _damageGate.TryAcceptHit(attacker) == false)
            {
                return;
            }

            _rootStats.OnAttacked(attacker, damage);
        }
    }
}
