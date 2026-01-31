using System;
using Data.DataType.ItemType.Interface;
using DataType.Skill.Factory.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Item
{
    public interface IGetterStrategyType
    {
        public Type GetStrategyType();
    }
    
    public abstract class ItemDataSO : BaseDataSO,IGetterStrategyType
    {
        [Title("Item Unique Settings")] 
        [VerticalGroup("Identity/Info")] // 부모의 그룹(Identity/Info)에 이어 붙입니다.
        [LabelWidth(100)]
        public int itemNumber; 

        [VerticalGroup("Identity/Info")]
        [LabelWidth(100)]
        public ItemGradeType itemGrade;
        
        // ▼ [변경] 변수가 아니라, 자식이 반드시 구현해야 하는 '추상 프로퍼티'로 선언
        [VerticalGroup("Identity/Info")] 
        [LabelWidth(100)]
        [ShowInInspector, DisplayAsString] // 에디터에서 값은 보여주되, 수정은 못하게(어차피 코드니까)
        public abstract ItemType ItemType { get; }
        public abstract Type GetStrategyType();
        public abstract string GetItemEffectText();
    }
}