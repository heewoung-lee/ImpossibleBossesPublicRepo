using System;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Decorator.Def.Start;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataType.Skill.Factory.Sequence.Def
{
    [Flags]
    public enum InterruptFlag
    {
        None            = 0,
    
        OnAnimationMismatch          = 1 << 0,  // 1
        OnTakeDamage    = 1 << 1,  // 2

        Everything      = ~0 
    }
    
    [Serializable]
    public class ChannelingSequenceDef : ISequenceDef
    {
        //채널링 길이
        [SerializeReference] public ISequenceLengthDef channelingLength;
        
        //채널링 이후 실행할 애니메이션
        [Title("Channeling Animation")]
        [LabelText("AnimationNameAfterCasting")]
        public string channelingAnimationNameAfterCasting;

        [MinValue(0.1f)]
        [LabelText("Animation Translation Duration")]
        public float channelingAnimTransitionDuration = 0.1f;
        
        //hit 길이
        [SerializeReference] public ISequenceLengthDef hitLength;
        public HitEventDef[] hits;
        public InterruptFlag interruptFlag;
    }
}