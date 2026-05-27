using DataType;
using UnityEngine;

namespace GameManagers.ItemDataManagement.Interface
{
    public interface IItemGradeBorder
    {
        public Sprite GetGradeBorder(ItemGradeType gradeType);
    }
}
