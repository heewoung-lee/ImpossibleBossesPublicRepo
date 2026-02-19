using System;
using Module.CommonModule;
using Scene.CommonInstaller.InGameInstaller;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Util;

namespace Stats.BaseStats
{
    public enum StatType
    {
        MaxHP,
        CurrentHp,
        Attack,
        Defence,
        MoveSpeed,
        Special
    }

    //1.31일 추가된 인터페이스 버퍼에서 특수한 효과를 쓰고 싶을때
    //스크립트에서 해당 인터페이스를 상속받고 구현하면 된다.
    public interface ISpecialModifier
    {
        public void ApplyModified(float value);
    }

    public interface ITargetable
    {
        public bool IsTargetable { get; }
    }


    public struct CharacterBaseStat : INetworkSerializable
    {
        public int MaxHp;
        public int Hp;
        public int Attack;
        public int Defence;
        public float Speed;
        public static CharacterBaseStat operator -(CharacterBaseStat firstValue, CharacterBaseStat secondValue)
        {
            return new CharacterBaseStat
            {
                MaxHp = firstValue.MaxHp - secondValue.MaxHp,
                Hp = firstValue.Hp - secondValue.Hp,
                Attack = firstValue.Attack - secondValue.Attack,
                Defence = firstValue.Defence - secondValue.Defence,
                Speed = firstValue.Speed - secondValue.Speed
            };
        }
        public CharacterBaseStat(int hp, int maxHp, int attack, int defence, float speed)
        {
            this.Hp = hp;
            this.MaxHp = maxHp;
            this.Attack = attack;
            this.Defence = defence;
            this.Speed = speed;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Hp);
            serializer.SerializeValue(ref MaxHp);
            serializer.SerializeValue(ref Attack);
            serializer.SerializeValue(ref Defence);
            serializer.SerializeValue(ref Speed);
        }
    }
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TargetableUnit))]
    public abstract class BaseStats : NetworkBehaviour, IDamageable
    {
        private Action<int, int> _eventAttacked; //현재 HP가 바로 안넘어와서 두번 매개변수에 현재 HP값 전달
        private Action<CharacterBaseStat> _doneBaseStatsLoading;

        private Action<int, int> _currentHpValueChangedEvent;
        private Action<int, int> _maxHpValueChangedEvent;
        private Action<int, int> _attackValueChangedEvent;
        private Action<int, int> _defenceValueChangedEvent;
        private Action<float, float> _moveSpeedValueChangedEvent;
        private Action<bool, bool> _isDeadValueChangeEvent;



        public event Action<int, int> EventAttacked
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _eventAttacked, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _eventAttacked, value);
            }

        }
        public event Action<CharacterBaseStat> DoneBaseStatsLoading
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _doneBaseStatsLoading, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _doneBaseStatsLoading, value);
            }
        }
    
        public event Action<int,int> CurrentHpValueChangedEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _currentHpValueChangedEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _currentHpValueChangedEvent, value);
            }
        }
        public event Action<int,int> MaxHpValueChangedEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _maxHpValueChangedEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _maxHpValueChangedEvent, value);
            }
        }
        public event Action<int,int> AttackValueChangedEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _attackValueChangedEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _attackValueChangedEvent, value);
            }
        }
        public event Action<int,int> DefenceValueChangedEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _defenceValueChangedEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _defenceValueChangedEvent, value);
            }
        }
        public event Action<float,float> MoveSpeedValueChangedEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _moveSpeedValueChangedEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _moveSpeedValueChangedEvent, value);
            }
        }
        public event Action<bool,bool> IsDeadValueChagneEvent
        {
            add
            {
                UniqueEventRegister.AddSingleEvent(ref _isDeadValueChangeEvent, value);
            }
            remove
            {
                UniqueEventRegister.RemovedEvent(ref _isDeadValueChangeEvent, value);
            }
        }

        private NetworkVariable<CharacterBaseStat> _characterBaseStatValue = new NetworkVariable<CharacterBaseStat>
            (new CharacterBaseStat(0, 0, 0, 0, 0f), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<int> _characterHpValue = new NetworkVariable<int>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<int> _characterMaxHpValue = new NetworkVariable<int>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<int> _characterAttackValue = new NetworkVariable<int>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         private NetworkVariable<int> _characterDefenceValue = new NetworkVariable<int>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         private NetworkVariable<float> _characterMoveSpeedValue = new NetworkVariable<float>
            (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<bool> _isDeadValue = new NetworkVariable<bool>
            (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public CharacterBaseStat CharacterBaseStats
        {
            get => _characterBaseStatValue.Value;
            protected set
            {
                SetPlayerBaseStatRpc(value);
            }
        }
        [Rpc(SendTo.Server)]
        public void SetPlayerBaseStatRpc(CharacterBaseStat baseStats, RpcParams rpcParams = default)
        {
            _characterBaseStatValue.Value = baseStats;
        }
        public int Hp
        {
            get => _characterHpValue.Value;
            protected set
            {
                PlayerHpValueChangedRpc(value);
            }
        }
        [Rpc(SendTo.Server)]
        public void PlayerHpValueChangedRpc(int value)
        {
            _characterHpValue.Value = Mathf.Clamp(value, 0, MaxHp);
        }



        public int MaxHp
        {
            get => _characterMaxHpValue.Value;
            protected set
            {
                PlayerMaxHpValueChangedRpc(value);
            }
        }
        [Rpc(SendTo.Server)]
        public void PlayerMaxHpValueChangedRpc(int value)
        {
            _characterMaxHpValue.Value = Mathf.Clamp(value, 0, int.MaxValue);
        }



        public int Attack
        {
            get => _characterAttackValue.Value;
            protected set
            {
                PlayerAttackValueChangedRpc(value);
            }
        }
        [Rpc(SendTo.Server)]
        public void PlayerAttackValueChangedRpc(int value)
        {
            
            _characterAttackValue.Value = Mathf.Clamp(value, 0, int.MaxValue);
            
            
        }



        public int Defence
        {
            get => _characterDefenceValue.Value;
            protected set
            {
                PlayerDefenceValueChangedRpc(value);
            }
        }
        [Rpc(SendTo.Server)]
        public void PlayerDefenceValueChangedRpc(int value)
        {
            _characterDefenceValue.Value = value;
            
            //1.3일 수정 다른 스탯들은 Clamp로 음수를 뚫고 가지 못하게 막았지만 디버프 효과가 있는
            //것들때문에 -를 허용하게 만듦
        }



        public float MoveSpeed
        {
            get => _characterMoveSpeedValue.Value;
            protected set
            {
                PlayeMoveSpeedValueChangedRpc(value);
            }
        }
        [Rpc(SendTo.Server)]
        public void PlayeMoveSpeedValueChangedRpc(float value)
        {
            _characterMoveSpeedValue.Value = Mathf.Clamp(value, 0, float.MaxValue);
        }

        public bool IsDead
        {
            get => _isDeadValue.Value;
            protected set
            {
                IsDeadValueChangedRpc(value);
            }   
        }
        [Rpc(SendTo.Server)]
        public void IsDeadValueChangedRpc(bool value)
        {
            //여기에서 콜라이더 로직을 변경해야함.
            _isDeadValue.Value = value;
        }



        public void Plus_Current_Hp_Abillity(int value)
        {
            Hp += value;
        }
        public void Plus_Defence_Abillity(int value)
        {
            Defence += value;
        }
        public void Plus_Attack_Ability(int value)
        {
            Attack += value;
        }
        public void Plus_MaxHp_Abillity(int value)
        {
            MaxHp += value;
        }
        public void Plus_MoveSpeed_Abillity(float value)
        {
            MoveSpeed += value;
        }

        protected abstract void SetStats();
        protected abstract void StartInit();
        protected void UpdateStat()
        {
            if (IsOwner == false)
                return;

            SetStats();
        }

        private void Awake()
        {
            AwakeInit();
        }

        private void Start()
        {
            StartInit();
        }

        protected virtual void AwakeInit()
        {
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _characterBaseStatValue.OnValueChanged += PlayerValueChanged;
            _characterHpValue.OnValueChanged += HpValueChanged;
            _characterMaxHpValue.OnValueChanged += MaxHpValueChanged;
            _characterAttackValue.OnValueChanged += AttackValueChanged;
            _characterDefenceValue.OnValueChanged += DefenceValueChanged;
            _characterMoveSpeedValue.OnValueChanged += MoveSpeedValueChanged;
            _isDeadValue.OnValueChanged += IsDeadValueChange;
        }
        private void PlayerValueChanged(CharacterBaseStat previousValue, CharacterBaseStat newValue)
        {
            CharacterBaseStat addValue = newValue - previousValue;

            MaxHp += addValue.MaxHp;
            Hp = newValue.Hp;
            Attack += addValue.Attack;
            Defence += addValue.Defence;
            MoveSpeed += addValue.Speed;

            if (IsOwner)
                DoneInitalizeCharacterBaseStatRpc(newValue);
        }
        private void HpValueChanged(int previousValue, int newValue)
        {
            _currentHpValueChangedEvent?.Invoke(previousValue,newValue);
            int damage = previousValue - newValue;
            if (damage > 0)
            {
                if (IsHost)
                {
                    OnAttackedClientRpc(damage, newValue);
                }
            }
        }
        private void MaxHpValueChanged(int previousValue, int newValue)
        {
            _maxHpValueChangedEvent?.Invoke(previousValue, newValue);
        }
        private void AttackValueChanged(int previousValue, int newValue)
        {
            _attackValueChangedEvent?.Invoke(previousValue, newValue);
        }
        private void DefenceValueChanged(int previousValue, int newValue)
        {
            _defenceValueChangedEvent?.Invoke(previousValue,newValue);
        }
        private void MoveSpeedValueChanged(float previousValue, float newValue)
        {
            _moveSpeedValueChangedEvent?.Invoke(previousValue,newValue);
        }
        private void IsDeadValueChange(bool previousValue, bool newValue)
        {
            _isDeadValueChangeEvent?.Invoke(previousValue,newValue);
        }


        //1.24일 추가 마지막으로 맞은 시간을 추가해 채널링 및 다른 구현에서
        //플레이어가 마지막으로 피격받은 시간이 체크조건을 시작한 시간보다 더 크면 종료하는 식으로 
        public float LastDamagedTime { get; private set; } = float.MinValue;

        public void OnAttacked(IAttackRange attacker, int spacialDamage = -1)
        {
            if (_isDeadValue.Value == true) return;

            LastDamagedTime= Time.time;
            NetworkObjectReference netWorkRef = TryGetOnAttackedOwner(attacker);
            OnAttackedRpc(netWorkRef, spacialDamage);
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void OnAttackedRpc(NetworkObjectReference attackerRef, int spacialDamage = -1)
        {
            ulong ownetClientId = OwnerClientId;

            int damage = 0;
            if (spacialDamage > 0)
            {
                damage = spacialDamage;
            }
            else
            {
                attackerRef.TryGet(out NetworkObject attackerNgo);
                attackerNgo.TryGetComponent(out BaseStats attackerStats);

                damage = attackerStats.Attack;
            }
            
            damage = Mathf.Max(0, damage - Defence);
            Hp -= damage;
            if (Hp <= 0)
            {
                Hp = 0;
                OnDeadRpc(attackerRef,RpcTarget.Single(ownetClientId, RpcTargetUse.Temp));
                _isDeadValue.Value = true;
            }
        }//서버니깐 서버에서 내가 죽으면 누구한테 죽었는지 죽었다고 호출하기 


        [Rpc(SendTo.SpecifiedInParams)]
        public void OnDeadRpc(NetworkObjectReference attackerRef, RpcParams rpcParams = default)
        {
            attackerRef.TryGet(out NetworkObject attackerNgo);
            attackerNgo.TryGetComponent(out BaseStats attackerStats);
            OnDead(attackerStats);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void OnAttackedClientRpc(int damage, int currentHp)
        {
            _eventAttacked?.Invoke(damage, currentHp);
        }


        [Rpc(SendTo.Owner)]
        public void DoneInitalizeCharacterBaseStatRpc(CharacterBaseStat stat) //UI가 이벤트를 걸기도 전에 실행이 되어버린다.
        {
            _doneBaseStatsLoading?.Invoke(stat);
        }

        public NetworkObjectReference TryGetOnAttackedOwner(IAttackRange attacker)
        {
            if (attacker.OwnerTransform.TryGetComponent(out NetworkObject ngo))
            {
                return new NetworkObjectReference(ngo);
            }
            UtilDebug.Log("Attacker hasn't a BaseStats");
            return default;
        }


        public void ModifyStat(StatType statType, float value)
        {
            int intValue = (int)value;
            
            switch (statType)
            {
                case StatType.Attack:
                    Plus_Attack_Ability(intValue);
                    break;
                case StatType.Defence:
                    Plus_Defence_Abillity(intValue);
                    break;
                case StatType.CurrentHp:
                    Plus_Current_Hp_Abillity(intValue);
                    break;
                case StatType.MaxHP:
                    Plus_MaxHp_Abillity(intValue);
                    break;
                case StatType.MoveSpeed:
                    Plus_MoveSpeed_Abillity(intValue);
                    break;
                case StatType.Special:
                    //여기에 각 클래스마다 스페셜하게 제작된 스크립트를 오버라이드해서 넣으면 됨.
                    //확장성은 떨어지지만, 우선 개발을 빨리해야하니 최소한으로 확장하고 나중에 확장이 필요하면 그때 고칠 것
                    if (TryGetComponent(out ISpecialModifier specialModifier) == true)
                    {
                        
                        specialModifier.ApplyModified(value);
                    }
                    else
                    {
                        Debug.Assert(false,$"{gameObject.name} hasn't implemented ISpecialModifier");
                    }
                    break;
                // 새로운 스탯이 생기면 여기에 case만 추가
                default:
                    UtilDebug.LogWarning($"[BufferManager] 정의되지 않은 StatType: {statType}");
                    break;
            }
        }
        



        protected abstract void OnDead(BaseStats attacker);
    }
}