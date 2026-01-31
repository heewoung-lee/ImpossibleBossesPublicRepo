using System;
using System.Collections.Generic;
using System.Linq;
using Controller.PlayerState.FighterState;
using Data.DataType.StatType;
using GameManagers;
using GameManagers.Data;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.GameManagerEx;
using Player;
using Stats;
using Stats.BaseStats;
using UnityEngine;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{


    public class ModuleFighterClass : ModulePlayerClass
    {
        private IAllData _allData;
        private Dictionary<int, FighterStat> _originData;
        [Inject]
        public void Construct(IAllData allData)
        {
            _allData = allData;
            _originData = _allData.GetData(typeof(FighterStat)) as Dictionary<int, FighterStat>;
            //이 모듈이 파이터 클래스에 대한 스탯을 가져오도록 정의
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Fighter;


        #region AnimationClipMethod

        public void AttackEvent()
        {
            if (IsOwner == false) return;
            
            TargetInSight.AttackTargetInSector(Stats);
        }

        #endregion
    }
}