using System;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Decorator.Def.Start;
using DataType.Skill.ShareDataDef;
using Module.PlayerModule.PlayerClassModule;
using Skill;
using UnityEngine;
using Util;

namespace DataType.Skill.Factory.Decorator.Strategy
{
    public sealed class PlayAnimationDecoratorStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(PlayAnimationDecoratorDef);

        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            var typed = def as PlayAnimationDecoratorDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, owner);
        }

        private sealed class Module : IDecoratorModule
        {
            private readonly PlayAnimationDecoratorDef _def;
            private readonly BaseController _owner;
            private int _animHash;
            private AnimInfoDefStruct _animInfo;

            public Module(PlayAnimationDecoratorDef def, BaseController owner)
            {
                _def = def;
                _owner = owner;
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                _animHash = 0;

                if (_def != null)
                {
                    _animInfo = _def.animNameRef.Resolve(ctx);
                    string name = _animInfo.AnimationName;
                    
                    if (!string.IsNullOrWhiteSpace(name))
                        _animHash = Animator.StringToHash(name);
                }
                
                if (_def == null || _animHash == 0)
                {
                    if (onComplete != null) onComplete();
                    return;
                }

                BaseController controller = _owner;
                if (ctx != null && ctx.Caster != null)
                    controller = ctx.Caster;

                ModulePlayerClass playerModule = controller.GetComponent<ModulePlayerClass>();
                if (playerModule == null)
                {
                    UtilDebug.LogWarning("[PlayAnimationDecorator] ModulePlayerClass missing on caster. Controller=" + controller.name);
                    if (onComplete != null) onComplete();
                    return;
                }

                if (playerModule.CommonSkillState == null)
                {
                    UtilDebug.LogError("[PlayAnimationDecorator] CommonSkillState is NOT initialized yet!");
                    if (onCancel != null) onCancel();   // 여기서는 실패로 보는 게 맞음
                    return;
                }

                playerModule.CommonSkillState.Prepare(
                    _animHash,
                    _animInfo.IsAnimationLocked,
                    _animInfo.TransitionDuration
                );
                
                controller.CurrentStateType = playerModule.CommonSkillState; //스킬상태를 현재 상태에 덮어쓰기

                if (onComplete != null) onComplete();
            }

            public void Release()
            {
                
            }
        }
    }
}
