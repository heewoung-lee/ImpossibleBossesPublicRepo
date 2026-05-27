using System;
using Data.DataType.ItemType.Interface;
using DataType.Strategies.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Item
{
    [CreateAssetMenu(fileName = "Coin_", menuName = "DataSO/Item/Coin")]
    public class CoinItemSO : ItemDataSO, ICanDrop
    {
        [Title("Coin Data")]
        [MinValue(1)]
        public int coinAmount = 1;

        public override ItemType ItemType => ItemType.ETC;
        public override bool UseLootGradeEffect => false;
        public override Type GetStrategyType() => typeof(CoinStrategy);

        public override string GetItemEffectText()
        {
            return string.Empty;
        }
    }
}
