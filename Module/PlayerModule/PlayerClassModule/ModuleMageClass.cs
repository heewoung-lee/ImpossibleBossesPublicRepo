using System.Collections.Generic;
using Data.DataType.StatType;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.VFXManager;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModuleMageClass : ModulePlayerClass
    {
        private IAllData _allData;
        private Dictionary<int, MageStat> _originData;
        private IVFXManagerServices _vfxManagerServices;
        [Inject]
        public void Construct(IAllData allData,IVFXManagerServices vfxManagerServices)
        {
            _allData = allData;
            _vfxManagerServices = vfxManagerServices;
            _originData = _allData.GetData(typeof(MageStat)) as Dictionary<int, MageStat>;
            //각기 모듈들이 클래스의 다름을 정의 하기에 이 부군에서 정의 할 수 밖에 없음.
            InitializeStatTable(_originData);
        }

        public override Define.PlayerClass PlayerClass => Define.PlayerClass.Mage;
          
        
        /// <summary>
        /// 아처 평타 함수 애니메이션 이벤트에 의해 호출됨 
        /// </summary>
        public void MageAttack()
        {
            if(IsOwner == false) return; // 12.31 수정 자신만 호출해야함 안그러면 평타가 모든 클라한테 호출 되어서 여러발 발사됨 
            _vfxManagerServices.InstantiateParticleWithTarget("Prefabs/Player/VFX/Mage/MageAttack",transform);
            _vfxManagerServices.InstantiateParticleWithTarget("Prefabs/Player/VFX/Mage/MageAttackMuzzle",transform);
            
        }
    }
}
