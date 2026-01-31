using System;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Def
{
    [Serializable]
    public class ChainTargetingDef : ITargetingDef
    {
        public float chainDistance;
        public LayerMask targetLayer;
        public Material highlightMat;
        
        
        [SerializeReference]
        public ExtraTargetCondition extraCondition;
    }
}