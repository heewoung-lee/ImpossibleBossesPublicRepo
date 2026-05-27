using Data.DataType.StatType;
using Stats.BaseStats;

namespace Stats.MonsterStats
{
    public class RootBinderStats : MonsterStats
    {
        private const int MonsterRootBinderID = 4;

        private int _exp;

        protected override void SetStats()
        {
            MonsterStat stat = _statDict[MonsterRootBinderID];
            CharacterBaseStat baseStat =
                new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);

            SetPlayerBaseStatRpc(baseStat);
            _exp = stat.exp;
        }

        protected override void StartInit()
        {
            base.StartInit();
            UpdateStat();
        }

        public void ResetForPoolDespawn()
        {
            MonsterStat stat = _statDict[MonsterRootBinderID];
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
