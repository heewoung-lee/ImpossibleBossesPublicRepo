using System;
using DataType.Skill.ShareDataDef;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Def
{
    [Serializable]
    public sealed class AreaTargetSelectionDef : ITargetingDef
    {
        public ScaleRefDef scaleRef;
        public LayerMask affectLayer;  
        public Material indicatorMat;
    }
}