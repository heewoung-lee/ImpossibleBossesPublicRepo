using System;

namespace DataType.Skill.ShareDataDef
{
    public interface ISkillSharedDef {}
    
    [Serializable]
    public sealed class SkillSharedDurationDef: ISkillSharedDef
    {
        public float commonDuration = 1.0f;
    }
    [Serializable]
    public sealed class SkillSharedScaleDef : ISkillSharedDef
    {
        public float effectScale = 1.0f;
    }
    
}