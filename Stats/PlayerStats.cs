using System;
using System.Collections.Generic;
using Data.DataType.StatType;
using GameManagers;
using GameManagers.Data;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.LoginManager;
using GameManagers.Interface.VFXManager;
using Module.PlayerModule.PlayerClassModule;
using Stats.BaseStats;
using UnityEngine;
using Zenject;

namespace Stats
{
    public interface IUnitStat : IGoogleSheetData
    {
        int hp { get; }
        int attack { get; }
        int defence { get; }
        float speed { get; }
        float viewAngle { get; }
        float viewDistance { get; }
        int xpRequired { get; }
    }


    public class PlayerStats : BaseStats.BaseStats, IAttackRange
    {
        private IAllData _allData;
        private IPlayerIngameLogininfo _playerIngameLogininfo;
        private IVFXManagerServices _vfxManager;

        [Inject]
        public void Construct(IAllData allData, IPlayerIngameLogininfo playerIngameLogininfo,
            IVFXManagerServices vfxManager)
        {
            _allData = allData;
            _playerIngameLogininfo = playerIngameLogininfo;
            _vfxManager = vfxManager;
        }


        private Dictionary<int, IUnitStat> _statDict;
        private int _level;
        private int _currentexp;
        private int _gold;
        private float _viewAngle;
        private float _viewDistance;

        private string _playerName;

        public Action PlayerDeadEvent;
        public Action<int> PlayerHasGoldChangeEvent;
        private LayerMask _targetLayer;

        public string Name
        {
            get
            {
                if (_playerName == null)
                {
                    _playerName = _playerIngameLogininfo.GetPlayerIngameLoginInfo().PlayerNickName;
                }

                return _playerName;
            }
        }

        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                _gold = Mathf.Clamp(_gold, 0, int.MaxValue);
                PlayerHasGoldChangeEvent?.Invoke(_gold);
            }
        }

        public bool TrySpendMoney(int price)
        {
            if (price > Gold)
            {
                return false;
            }
            else
            {
                Gold -= price;
                return true;
            }
        }

        public float ViewAngle
        {
            get => _viewAngle;
        }

        public float ViewDistance
        {
            get => _viewDistance;
        }

        public int Level
        {
            get => _level;
        }

        public Transform OwnerTransform => transform;
        public LayerMask TarGetLayer => _targetLayer;
        public Vector3 AttackPosition => transform.position;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            _level = 1;
            _currentexp = 0;
            _gold = 0;
        }

        protected override void StartInit()
        {
            ModulePlayerClass playerModule = GetComponent<ModulePlayerClass>();


            if (playerModule == null)
            {
                Debug.LogError("[PlayerStats] ModulePlayerClass not found on this GameObject!");
                return;
            }

            // 2. 모듈에게 스탯 데이터를 요청합니다.
            _statDict = playerModule.GetStatTable();
             _targetLayer = LayerMask.GetMask("Monster");

            if (IsOwner == false)
                return;

            SetStats();
        }

        public int Exp
        {
            get => _currentexp;
            set
            {
                _currentexp = value;
                //레벨업 체크
                while (true)
                {
                    IUnitStat stat; 
                    if (_statDict.TryGetValue(_level + 1, out stat) == false)
                        break;

                    if (_currentexp < stat.xpRequired)
                        break;

                    else if (_currentexp >= stat.xpRequired) // 100/20
                    {
                        _currentexp -= stat.xpRequired;
                        _level++;
                        UpdateStat();
                        _vfxManager.InstantiateParticleWithTarget("Prefabs/Player/VFX/Common/Level_up",
                            gameObject.transform);
                    }
                }
            }
        }


        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (IsOwner)
            {
                Debug.Log("Player Dead");
                PlayerDeadEvent.Invoke();
            }
        }

        protected override void SetStats()
        {
            if (_statDict.TryGetValue(_level, out IUnitStat stat))
            {
                // IUnitStat 인터페이스 덕분에 FighterStat이든 MageStat이든 공통 속성 접근 가능
                CharacterBaseStat baseStat = new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);
                CharacterBaseStats = baseStat;
                _viewAngle = stat.viewAngle;
                _viewDistance = stat.viewDistance;
            }
        }
    }
}