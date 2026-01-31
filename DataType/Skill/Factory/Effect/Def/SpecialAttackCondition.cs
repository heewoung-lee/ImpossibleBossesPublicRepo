using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Util;

namespace DataType.Skill.Factory.Effect.Def
{
    public enum SpecialAttack
    {
        BackAttack
    }
    
    [Serializable]
    public class SpecialAttackCondition
    {
        public SpecialAttack specialAttack;
        
        [MinValue(1.0f)]
        [LabelText("Damage Multiplier")]
        public float multiplier = 1.5f; // 기본 1.5배

        // 백어택 판정 각도
        [ShowIf("specialAttack", SpecialAttack.BackAttack)]
        [Range(0f, 180f)]
        public float backAttackAngle = 120f; 

        public int CalculateSpecialDamage(int damage, Transform caster, Transform target)
        {
            if (caster == null || target == null) return damage;

            switch (specialAttack)
            {
                case SpecialAttack.BackAttack:
                    if (TargetInSight.IsBackAttack(caster.transform, target.transform,backAttackAngle))
                    {
                        return (int)(damage * multiplier);
                    }
                    break;
            }
            return damage;
        }

        
    }
}