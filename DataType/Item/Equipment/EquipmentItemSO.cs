using System;
using System.Collections.Generic;
using System.Text;
using Data.DataType.ItemType;
using Data.DataType.ItemType.Interface;
using DataType.Strategies;
using Sirenix.OdinInspector;
using UnityEngine;
using Util;

namespace DataType.Item.Equipment
{
    public enum EquipmentSlotType
    {
        Helmet,   
        Gauntlet, 
        Shoes,    
        Weapon,   
        Ring,     
        Armor     
    }
    
    [CreateAssetMenu(fileName = "Equip_", menuName = "DataSO/Item/Equipment")]
    public class EquipmentItemSO : ItemDataSO
    {
        [Title("Equipment Data")]
        [EnumToggleButtons]
        public EquipmentSlotType slotType;

        [Title("Stats")]
        [TableList(ShowIndexLabels = true)]
        public List<StatEffect> itemEffects = new List<StatEffect>();

        // 전략 패턴: 장비 관련 전략 타입을 반환
        public override Type GetStrategyType()
        {
            return typeof(EquipmentStrategy);
        }

        public override string GetItemEffectText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Utill.ItemGradeConvertToKorean(itemGrade)} 등급\n");
            
            foreach (StatEffect effect in itemEffects)
            {
                sb.Append($"{Utill.StatTypeConvertToKorean(effect.statType)} : {effect.value}\n");
            }
            return sb.ToString();
        }

        public override ItemType ItemType => ItemType.Equipment;
    }

}