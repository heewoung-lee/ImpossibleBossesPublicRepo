using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.DataType.StatType;
using GameManagers;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using GameManagers.RelayManager;
using Stats;
using UnityEngine;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModuleArcherClass : ModulePlayerClass
    {
        private IAllData _allData;
        private IVFXManagerServices _vfxManagerServices;
        private Dictionary<int, ArcherStat> _originData;

        [Inject]
        public void Construct(IAllData allData,IVFXManagerServices vfxManagerServices)
        {
            _allData = allData;
            _vfxManagerServices = vfxManagerServices;
            _originData = _allData.GetData(typeof(ArcherStat)) as Dictionary<int, ArcherStat>;
            //이 모듈이 파이터 클래스에 대한 스탯을 가져오도록 정의
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }

        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Archer;

        
        
        
        
        /// <summary>
        /// 아처 평타 함수 애니메이션 이벤트에 의해 호출됨 
        /// </summary>
        public void AttackArrow()
        {
            if(IsOwner == false) return; // 12.31 수정 자신만 호출해야함 안그러면 평타가 모든 클라한테 호출 되어서 여러발 발사됨 
             _vfxManagerServices.InstantiateParticleWithTarget("Prefabs/Player/VFX/Archer/ArcherAttack",transform);
            _vfxManagerServices.InstantiateParticleWithTarget("Prefabs/Player/VFX/Archer/ArcherAttackMuzzle",transform);
            
        }

       
        
    }
}
