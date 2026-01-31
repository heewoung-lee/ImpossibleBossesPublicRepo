using System.Collections.Generic;
using System.Linq;
using Data.DataType.StatType;
using GameManagers.Interface.DataManager;
using Stats;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModuleMonkClass : ModulePlayerClass
    {
        private IAllData _allData;
        private Dictionary<int, MonkStat> _originData;
        [Inject]
        public void Construct(IAllData allData)
        {
            _allData = allData;
            _originData = _allData.GetData(typeof(MonkStat)) as Dictionary<int, MonkStat>;
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }
        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Monk;
    }
}
