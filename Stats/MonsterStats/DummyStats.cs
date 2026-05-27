using Data.DataType.StatType;
using GameManagers.ResourcesExManagement;
using Stats.BaseStats;
using Unity.Netcode;
using Util;
using Zenject;

namespace Stats.MonsterStats
{
    public class DummyStats : MonsterStats
    {
        private const int MonsterDummyID = 1;
        [Inject] private IResourcesServices _resources;
        private int _exp;
        protected override void SetStats()
        {
            MonsterStat stat = _statDict[MonsterDummyID];
            CharacterBaseStat basestat = new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence,stat.speed);
            SetPlayerBaseStatRpc(basestat);
            _exp = stat.exp;
        }

        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (attacker.TryGetComponent(out PlayerStats playerStat))
            {
                playerStat.AddExpFromMonster(_exp);
            }

            if (gameObject.TryGetComponent(out NetworkObject ngo))
            {
                _resources.DestroyObject(ngo.gameObject);
            }
        }

        protected override void StartInit()
        {
            base.StartInit();
            UpdateStat();
        }
    }
}
