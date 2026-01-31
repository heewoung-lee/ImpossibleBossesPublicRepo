using System;
using DataType.Skill;
using DataType.Skill.ShareDataDef;
using Sirenix.OdinInspector;
using Skill;
using UnityEngine;

public enum DurationSourceType
{
    UseShared,
    Override,
    None
}

/// <summary>
/// 공유지속시간 데이터
/// </summary>

[Serializable]
public sealed class DurationRefDef
{
    [SerializeField]
    [LabelText("Source")]
    private DurationSourceType source = DurationSourceType.UseShared;

    private bool IsOverride
    {
        get { return source == DurationSourceType.Override; }
    }

    [SerializeField]
    [ShowIf(nameof(IsOverride))]
    [LabelText("Seconds")]
    [MinValue(0.01f)]
    private float overrideSeconds = 1.0f;

    public DurationSourceType Source
    {
        get { return source; }
    }

    public float Resolve(SkillExecutionContext ctx)
    {
        if (source == DurationSourceType.None)
        {
            return -1f; // 프리팹 라이프타임 사용
        }

        if (source == DurationSourceType.Override)
        {
            return overrideSeconds;
        }

        if (ctx == null) return overrideSeconds;

        SkillDataSO data = ctx.SkillData;
        if (data == null) return overrideSeconds;

        SkillSharedDurationDef durationSharedDuration = null;
        foreach (ISkillSharedDef sharedDef in data.sharedDefs)
        {
            if (sharedDef is SkillSharedDurationDef typed)
            {
                durationSharedDuration = typed;
                break;
            }
        }
        if (durationSharedDuration == null) return overrideSeconds;
        return durationSharedDuration.commonDuration;
    }
}