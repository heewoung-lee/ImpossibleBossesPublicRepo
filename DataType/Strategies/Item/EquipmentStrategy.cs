using System;
using Controller;
using DataType.Item.Equipment;
using DataType.Skill;
using GameManagers.Interface.BufferManager;
using Skill;
using Stats.BaseStats;
using UnityEngine;
using Util;
using Zenject;

namespace DataType.Strategies
{
    public class EquipmentStrategy : IStrategy, IEquippable
    {
        private readonly IBufferManager _bufferManager;

        [Inject]
        public EquipmentStrategy(IBufferManager bufferManager)
        {
            _bufferManager = bufferManager;
        }

        void IStrategy.Execute(ExecutionContext context)
        {
            BaseController controller = context.Caster;
            BaseDataSO data = context.Data;

            UtilDebug.LogError(
                $"[EquipmentStrategy] Do NOT use IStrategy.Execute! Use IEquippable.Equip instead! Target: {controller.name}");
            if (controller.TryGetComponent(out BaseStats stats))
            {
                Equip(stats, data);
            }
            else
            {
                UtilDebug.LogWarning($"[EquipmentStrategy] Check the {controller.name} controller.!");
            }
        }

        public void Equip(BaseStats stats, BaseDataSO data)
        {
            ApplyStats(stats, data, 1);
        }

        public void UnEquip(BaseStats stats, BaseDataSO data)
        {
            ApplyStats(stats, data, -1);
        }

        private void ApplyStats(BaseStats stats, BaseDataSO data, int multiplier)
        {
            if (stats != null && data is EquipmentItemSO equipData)
            {
                foreach (var effect in equipData.itemEffects)
                {
                    _bufferManager.ModifyStat(stats, effect.statType, effect.value * multiplier);
                }
            }
        }
    }
}