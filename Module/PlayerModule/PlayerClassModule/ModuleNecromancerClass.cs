using System.Collections.Generic;
using System.Linq;
using Data.DataType.StatType;
using GameManagers.Interface.DataManager;
using Stats;
using UnityEngine;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModuleNecromancerClass : ModulePlayerClass
    {
        private IAllData _allData;
        private Dictionary<int, NecromancerStat> _originData;
        private const float BackAttackAngle = 120f;
        private const float BackAttackMultiplier = 1.5f;
            
        [Inject]
        public void Construct(IAllData allData)
        {
            _allData = allData;
            _originData = _allData.GetData(typeof(NecromancerStat)) as Dictionary<int, NecromancerStat>;
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Necromancer;
        
        
        #region AnimationClipMethod

        public void AttackEvent()
        {
            if (IsOwner == false) return;
            TargetInSight.AttackTargetInSector(Stats, -1, CalculateBackAttackMultiplier);
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
