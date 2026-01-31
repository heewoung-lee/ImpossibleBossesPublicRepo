using System;
using Controller;
using DataType.Item.Consumable;
using DataType.Skill;
using GameManagers.Interface.BufferManager;
using GameManagers.RelayManager;
using Skill; 
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace DataType.Strategies
{
    public class ConsumableStrategy : IStrategy
    {
        private readonly IBufferManager _bufferManager;
        private readonly RelayManager _relayManager;

        [Inject]
        public ConsumableStrategy(
            IBufferManager bufferManager,
            RelayManager relayManager)
        {
            _bufferManager = bufferManager;
            _relayManager = relayManager;
        }

        public void Execute(ExecutionContext context)
        {
            BaseController controller = context.Caster;
            BaseDataSO data = context.Data;
            
            if (data is not ConsumableItemSO consumableData)
            {
                Debug.Assert(false, "[Strategy] 데이터 타입 오류! ConsumableItemSO가 아닙니다.");
                return;
            }
            ApplyConsumable(controller, consumableData);
        }

        private void ApplyConsumable(BaseController controller, ConsumableItemSO data)
        {
            Collider[] targets = new[] { controller.GetComponent<Collider>() };

            _bufferManager.ApplyActionToTargetsTotal(
                targets,
                (targetNgo) => { /* 파티클 로직이 필요하면 여기에 작성 */ },
                (targetNgo) => ApplyEffectsToTarget(targetNgo, data) 
            );
        }

        private void ApplyEffectsToTarget(NetworkObject targetNgo, ConsumableItemSO data)
        {
            foreach (var buffData in data.itemEffects)
            {
                _relayManager.NgoRPCCaller.Call_InitBuffer_ServerRpc(
                    buffData.effect,     
                    buffData.iconPath,   
                    data.duration,        
                    targetNgo.NetworkObjectId
                );
            }
        }

    }
}