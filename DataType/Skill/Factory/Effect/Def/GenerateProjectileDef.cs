using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public sealed class GenerateProjectileDef : IEffectDef
    {
        [Header("Projectile Path")]
        public string projectilePrefabPath;
        
        [Header("Projectile LifeTime")]
        [MinValue(0.01f)]
        public float lifeTime = 1f;
        
        [Header("Send Params")]
        public string stringParams;
        public float floatParams;
        public int integerParams;
        public bool booleanParams;
        
        
        
    }
}