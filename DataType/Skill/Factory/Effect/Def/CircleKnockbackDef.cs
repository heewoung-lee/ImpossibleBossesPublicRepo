using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public sealed class CircleKnockbackDef : IEffectDef
    {
        [LabelText("Knockback Radius")]
        [MinValue(0.1f)]
        public float radius = 5f;
        
        [LabelText("Push Duration")]
        [MinValue(0.1f)]
        public float pushDuration = 0.5f;

        public LayerMask targetLayer; 
    }
}