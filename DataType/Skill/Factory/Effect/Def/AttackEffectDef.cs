using System;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public sealed class AttackEffectDef : IEffectDef
    {
        [MinValue(0.1f)]
        public float multiplier = 1.0f; //곱 계수
        public float additional = 0f; // 더하는 계수

        [SerializeReference] 
        public SpecialAttackCondition specialAttackCondition;
    }

}