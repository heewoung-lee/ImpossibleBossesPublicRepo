using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Item.Consumable
{
    [CreateAssetMenu(fileName = "ExperienceBook_", menuName = "DataSO/Item/ExperienceBook")]
    public class ExperienceBookItemSO : ItemDataSO, IHasSellValue
    {
        [Title("Experience Book Data")]
        [MinValue(1)]
        public int experienceAmount = 1;

        [Title("Shop Data")]
        [MinValue(0)]
        public int sellValue;

        public int SellValue => sellValue;

        public override ItemType ItemType => ItemType.ETC;

        // Experience books are consumed directly by shop purchase flow, not inventory-use strategy.
        public override Type GetStrategyType() => null;

        public override string GetItemEffectText()
        {
            return $"EXP +{experienceAmount}";
        }
    }
}
