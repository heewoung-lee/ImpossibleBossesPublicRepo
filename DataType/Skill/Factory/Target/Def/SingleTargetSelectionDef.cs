using System;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Def
{
    [Serializable]
    public sealed class SingleTargetSelectionDef : ITargetingDef
    {
        public LayerMask targetLayer;
        public Material highlightMat;
        
        
        [SerializeReference]
        public ExtraTargetCondition extraTargetCondition;
    }
}