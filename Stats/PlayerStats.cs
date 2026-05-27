using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.DataType.StatType;
using GameManagers;
using GameManagers.DataManagement;
using GameManagers.LoginManagement;
using GameManagers.RelayManagement;
using GameManagers.SoundManagement;
using GameManagers.UIManagement;
using GameManagers.VFXManagement;
using Module.PlayerModule.PlayerClassModule;
using Stats.BaseStats;
using UI.Popup.PopupUI;
using Unity.Netcode;
using UnityEngine;
using Util;
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
        private const string GetExpVfxPath = "Prefabs/Player/VFX/Common/GetExp";
        private const string LevelUpVfxPath = "Prefabs/Player/VFX/Common/Level_up";
        private const string DeadSfxCueId = "DeadSFX";

        private IPlayerIngameLogininfo _playerIngameLogininfo;
        private IVFXManagerServices _vfxManager;
        private IUIManagerServices _uiManager;
        private RelayManager _relayManager;

        [Inject]
        public void Construct(IPlayerIngameLogininfo playerIngameLogininfo,
            IVFXManagerServices vfxManager,
            IUIManagerServices uiManager,
            RelayManager relayManager)
        {
            _playerIngameLogininfo = playerIngameLogininfo;
            _vfxManager = vfxManager;
            _uiManager = uiManager;
            _relayManager = relayManager;
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
        private Action<int> _playerExpChangedEvent;
        private LayerMask _targetLayer;

        public event Action<int> PlayerExpChangedEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _playerExpChangedEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _playerExpChangedEvent, value);
            }
        }

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

        public int RequiredExpForNextLevel
        {
            get
            {
                if (_statDict != null && _statDict.TryGetValue(_level + 1, out IUnitStat stat))
                {
                    return stat.xpRequired;
                }

                return 0;
            }
        }

        public bool IsMaxLevel
        {
            get
            {
                if (_statDict == null)
                {
                    return false;
                }

                return _statDict.TryGetValue(_level + 1, out _) == false;
            }
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
                UtilDebug.LogError("[PlayerStats] ModulePlayerClass not found on this GameObject!");
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
                bool hasGainedExp = value > _currentexp;
                _currentexp = value;

                if (hasGainedExp)
                {
                    _vfxManager.InstantiateParticleWithTarget(GetExpVfxPath, transform);
                }

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
                        _vfxManager.InstantiateParticleWithTarget(LevelUpVfxPath, transform);
                    }
                }

                _playerExpChangedEvent?.Invoke(_currentexp);
            }
        }

        public void AddExpFromMonster(int exp)
        {
            if (IsOwner)
            {
                Exp += exp;
                return;
            }

            AddExpFromMonsterRpc(exp);
        }

        // 2026-05-22: 미니언 사망 시 monster OnDead가 미니언 소유자 쪽에서 실행되어
        // 호스트에 있는 공격자 복제본에만 경험치가 적용되던 문제를 공격자 owner에게 경험치 지급 RPC를 보내도록 수정.
        [Rpc(SendTo.Owner)]
        private void AddExpFromMonsterRpc(int exp)
        {
            Exp += exp;
        }


        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            if (IsOwner)
            {
                if (TryGetComponent(out SoundPlayerBinder soundPlayerBinder))
                {
                    soundPlayerBinder.PlayDetached(DeadSfxCueId);
                }
                else
                {
                    UtilDebug.LogError("[PlayerStats] SoundPlayerBinder not found on this GameObject!");
                }

                UtilDebug.Log("Player Dead");
                PlayerDeadEvent.Invoke();
            }
        }

        protected override void SetStats()
        {
            if (_statDict.TryGetValue(_level, out IUnitStat stat))
            {
                // IUnitStat 인터페이스 덕분에 FighterStat이든 MageStat이든 공통 속성 접근 가능
                CharacterBaseStat baseStat =
                    new CharacterBaseStat(stat.hp, stat.hp, stat.attack, stat.defence, stat.speed);
                CharacterBaseStats = baseStat;
                _viewAngle = stat.viewAngle;
                _viewDistance = stat.viewDistance;
            }
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            IsDeadValueChagneEvent += IsDestruction;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            IsDeadValueChagneEvent -= IsDestruction;
        }


        private void IsDestruction(bool oldValue, bool newValue)
        {
            if (newValue == false) return; // 죽음이 아닐때 즉 부활한경우는 제외
            if (IsHost == false) return;
            if (IsAllPlayersDead() == false) return;

            //여기까지 오면 다 죽었다는뜻 UI 출력
            ShowPopupUIAllPlayersDeadRpc();
        }

        private bool IsAllPlayersDead()
        {
            var countPlayer = _relayManager.NetworkManagerEx.ConnectedClientsIds;

            foreach (ulong userID in countPlayer)
            {
                NetworkObject[] ownerNetworkObjects =
                    _relayManager.NetworkManagerEx.SpawnManager.GetClientOwnedObjects(userID);

                foreach (NetworkObject ownerNetworkObject in ownerNetworkObjects)
                {
                    if (ownerNetworkObject.TryGetComponent(out PlayerStats playerStats))
                    {
                        if (playerStats.IsDead == false)
                        {
                            return false; //한 플레이어가 살아있으면 바로 리턴
                        }
                    }
                }
            }

            return true;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ShowPopupUIAllPlayersDeadRpc()
        {
            UIAllPlayerDead allPlayerDeadUI = _uiManager.GetOrCreateSceneUI<UIAllPlayerDead>();
            allPlayerDeadUI.gameObject.SetActive(true);

            // 전멸 시점에는 먼저 모든 클라이언트가 로컬 릴레이 연결을 끊고,
            // 이후 로비 이동은 전멸 UI의 버튼 또는 자동 타이머가 담당한다.
            if (_relayManager.NetworkManagerEx.IsListening || _relayManager.NetworkManagerEx.ShutdownInProgress)
            {
                DelayShutdownRelayAsync(this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private async UniTaskVoid DelayShutdownRelayAsync(CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, cancellationToken: cancellationToken);

            if (_relayManager.NetworkManagerEx.IsListening || _relayManager.NetworkManagerEx.ShutdownInProgress)
            {
                _relayManager.ShutDownRelay(RelayDisconnectCause.IntentionalLeaveToLobby);
            }
        }
    }
}
