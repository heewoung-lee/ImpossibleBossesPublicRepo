using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Controller.BossState.BossRedDragon;
using Data.DataType.StatType;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;

namespace Stats.BossStats
{
    public class BossRedDragonStats : BossStats
    {
        private readonly Dictionary<ulong, int> _lastHitFrameByAttacker = new Dictionary<ulong, int>();
        private int _bossID;
        private BossRedDragonController _controller;

        protected override void StartInit()
        {
            base.StartInit();
            _controller = GetComponent<BossRedDragonController>();
            _lastHitFrameByAttacker.Clear();
            UpdateStat();
        }
        
        protected override void SetStats()
        {
            _bossID = (int)Define.BossID.RedDragon;
            BossStat stat = _statDict[_bossID];
            MaxHp = stat.hp;
            Hp = stat.hp;
            Attack = stat.attack;
            Defence = stat.defence;
            MoveSpeed = stat.speed;
            _viewAngle = stat.viewAngle;
            _viewDistance = stat.viewDistance;
        }

        public bool TryAcceptHit(IAttackRange attacker)
        {
            if (attacker == null || attacker.OwnerTransform == null)
            {
                return true;
            }

            ulong attackerKey = GetAttackerKey(attacker.OwnerTransform);
            int currentFrame = Time.frameCount;

            if (_lastHitFrameByAttacker.TryGetValue(attackerKey, out int lastHitFrame) == true &&
                lastHitFrame == currentFrame)
            {
                return false;
            }

            _lastHitFrameByAttacker[attackerKey] = currentFrame;
            return true;
        }

        private static ulong GetAttackerKey(Transform ownerTransform)
        {
            if (ownerTransform.TryGetComponent(out NetworkObject networkObject) == true)
            {
                return networkObject.NetworkObjectId;
            }

            NetworkObject parentNetworkObject = ownerTransform.GetComponentInParent<NetworkObject>();
            if (parentNetworkObject != null)
            {
                return parentNetworkObject.NetworkObjectId;
            }

            return unchecked((ulong)ownerTransform.GetInstanceID());
        }

        protected override void OnDead(BaseStats.BaseStats attacker)
        {
            _controller.CurrentStateType = _controller.BaseDieState;
            GetComponent<BehaviorTree>().SendEvent("BossDeadEvent");
        }
    }
}
