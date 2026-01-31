using Data.DataType.StatType;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Stats.BaseStats;
using Unity.Netcode;
using Util;
using Zenject;

namespace Stats.MonsterStats.SlimeStats
{
    public class SlimeStats : MonsterStats
    {
        [Inject] private IResourcesServices _resources;
        

        private Define.MonsterID _slimeID;
        private int _exp;


        protected override void AwakeInit()
        {
            base.AwakeInit();
            _slimeID = Define.MonsterID.Slime;
        }
        protected override void SetStats()
        {
            MonsterStat stat = _statDict[(int)_slimeID];
            CharacterBaseStat basestat = new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence,stat.speed);
            SetPlayerBaseStatRpc(basestat);
            _exp = stat.exp;
        }

        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (attacker.TryGetComponent(out PlayerStats playerStat))
            {
                playerStat.Exp += _exp;
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
