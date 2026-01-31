using System;
using Sirenix.OdinInspector;
using Skill;
using UnityEngine;

namespace DataType.Skill.ShareDataDef
{
    public enum AnimationSourceType
    {
        UseShared,
        Override,
    }

    public readonly struct AnimInfoDefStruct
    {
        private readonly string _animName;
        private readonly bool _isAnimationLocked;
        private readonly float _transitionDuration;

        public AnimInfoDefStruct(string animName, bool isAnimationLocked, float transitionDuration)
        {
            _animName = animName;
            _isAnimationLocked = isAnimationLocked;
            _transitionDuration = transitionDuration;
        }
        
        
        public string AnimationName => _animName;
        public bool IsAnimationLocked => _isAnimationLocked;
        public float TransitionDuration => _transitionDuration;
    }
    
    
    [Serializable]
    public class AnimNameRefDef
    {
        [SerializeField]
        [LabelText("Source")]
        private AnimationSourceType source = AnimationSourceType.UseShared;
        private bool IsOverride
        {
            get { return source == AnimationSourceType.Override; }
        }
        [SerializeField]
        [ShowIf(nameof(IsOverride))]
        [LabelText("AnimName")]
        private string animationName;
        
        [SerializeField]
        [ShowIf(nameof(IsOverride))]
        [LabelText("isAnimationLocked")]
        private bool isAnimationLocked;
        
        [SerializeField]
        [ShowIf(nameof(IsOverride))]
        [LabelText("transitionDuration")]
        [MinValue(0.1f)]
        private float transitionDuration = 0.1f;
        
        
        public AnimationSourceType Source
        {
            get { return source; }
        }
        public AnimInfoDefStruct Resolve(SkillExecutionContext ctx)
        {
            if (source == AnimationSourceType.Override)
            {
                return new AnimInfoDefStruct(animationName, isAnimationLocked, transitionDuration);
            }

            if (ctx == null) return new AnimInfoDefStruct(animationName, isAnimationLocked, transitionDuration);
            SkillDataSO data = ctx.SkillData;
            if (data == null) return new AnimInfoDefStruct(animationName, isAnimationLocked, transitionDuration);
            return new AnimInfoDefStruct(data.animationStateName,data.isAnimationLocked,data.transitionDuration);
        }
        
    }
}