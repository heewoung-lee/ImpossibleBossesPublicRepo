using System;
using DataType.Skill.Factory.Target.Def;
using UnityEngine;

namespace DataType.Skill.Factory.Decorator.Def
{
    
    /// <summary>
    /// 원형탐지 데코레이션 원형값에 검출된 타겟들에게 vfx를 넣어줌
    /// </summary>
    [Serializable]
    public sealed class QueryTargetsDecoratorDef : IStackElementDecoratorDef
    {
        public HitVfxDecoratorDef hitVfx;
    }
}