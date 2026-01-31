using System;
using Sirenix.OdinInspector;
using Skill;
using UnityEngine;

namespace DataType.Skill.ShareDataDef
{
    public enum VFXPathRefSourceType
    {
        UseShared,
        Override
    }

    /// <summary>
    /// VFX경로 공유 데이터
    /// </summary>

    [Serializable]
    public sealed class VFXPathRefDef
    {
        [SerializeField]
        [LabelText("Source")]
        private VFXPathRefSourceType source = VFXPathRefSourceType.UseShared;

        private bool IsOverride
        {
            get { return source == VFXPathRefSourceType.Override; }
        }

        [SerializeField]
        [ShowIf(nameof(IsOverride))]
        [LabelText("VFX Path")]
        private string overrideValue = "";

        public VFXPathRefSourceType Source
        {
            get { return source; }
        }

        public string Resolve(SkillExecutionContext ctx)
        {
            if (source == VFXPathRefSourceType.Override)
            {
                return overrideValue;
            }

            if (ctx == null) return overrideValue;

            SkillDataSO data = ctx.SkillData;
            if (data == null) return overrideValue;

            return data.vfxPrefabPath;
        }
    }
}