using System;
using Sirenix.OdinInspector;
using Skill;
using UnityEngine;

namespace DataType.Skill.ShareDataDef
{
    public enum ScaleType
    {
        UseShared,
        Override,
        None
    }

    /// <summary>
    /// 스케일에 공유 데이터
    /// </summary>

    [Serializable]
    public sealed class ScaleRefDef
    {
        [SerializeField]
        [LabelText("Source")]
        private ScaleType source = ScaleType.UseShared;

        private bool IsOverride
        {
            get { return source == ScaleType.Override; }
        }

        [SerializeField]
        [ShowIf(nameof(IsOverride))]
        [LabelText("ScaleValue")]
        [MinValue(0.01f)]
        private float overrideValue = 1.0f;

        public ScaleType Source
        {
            get { return source; }
        }

        public float Resolve(SkillExecutionContext ctx)
        {
            if (source == ScaleType.None)
            {
                return 1f; // 원래 스케일 사이즈
            }

            if (source == ScaleType.Override)
            {
                return overrideValue;
            }

            if (ctx == null) return overrideValue;

            SkillDataSO data = ctx.SkillData;
            if (data == null) return overrideValue;

            SkillSharedScaleDef shared = null;
            foreach (ISkillSharedDef sharedDef in data.sharedDefs)
            {
                if (sharedDef is SkillSharedScaleDef typed)
                {
                    shared = typed;
                    break;
                }
            }
            if (shared == null) return overrideValue;

            return shared.effectScale;
        }
    }
}