using System;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType
{
    public enum ItemGradeType
    {
        Normal,
        Magic,
        Rare,
        Unique,
        Epic
    }
    public enum ItemType
    {
        Equipment,
        Consumable,
        ETC
    }
    
    public abstract class BaseDataSO : ScriptableObject
    {
        [Header("Basic Information")] 
        
        [HorizontalGroup("Identity", 75)] 
        [PreviewField(75, ObjectFieldAlignment.Left), HideLabel]
        public Sprite icon; 

        [VerticalGroup("Identity/Info")]
        [LabelWidth(100)]
        public string dataName; 
        
        [VerticalGroup("Identity/Info")]
        [LabelWidth(100), TextArea] 
        public string description; 
     
    }
}