using Data.DataType.ItemType.Interface;
using DataType;
using UnityEngine;

namespace GameManagers.ItamDataManager.Interface
{
    public interface IItemGradeBorder
    {
        public Sprite GetGradeBorder(ItemGradeType gradeType);
    }
}
