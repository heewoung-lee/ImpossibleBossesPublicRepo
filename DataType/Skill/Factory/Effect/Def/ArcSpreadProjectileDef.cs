using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Def
{
    [Serializable]
    public sealed class ArcSpreadProjectileDef : IEffectDef
    {
        [Header("Spread")]
        [Unit(Units.Degree)]
        [PropertyRange(0, 360)]
        public float spreadAngle = 90f;

        [MinValue(1)]
        public int projectileCount = 1;

        [Header("Projectile Path")]
        public string projectilePrefabPath;
        
        [Header("Projectile LifeTime")]
        [MinValue(0.01f)]
        public float lifeTime = 1f;
    }
}