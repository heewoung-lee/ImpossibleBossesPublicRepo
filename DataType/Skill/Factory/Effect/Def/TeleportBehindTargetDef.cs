using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public sealed class TeleportBehindTargetDef : IEffectDef
    {
        [SerializeField]
        [MinValue(0.5f)]                
        [LabelText("Distance Behind Target")]   
        public float distanceBehind = 1.5f;
        
        [LabelText("Look At Target After Teleport")]
        public bool lookAtTarget = true;//옮긴뒤 바라볼것인가
    }
}