using System;
using Controller;
using DataType.Item.Consumable;
using DataType.Skill.Factory.Decorator.Def;
using DataType.Skill.Factory.Effect.Def;
using GameManagers.Interface.BufferManager;
using GameManagers.RelayManager;
using Skill;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace DataType.Skill.Factory.Effect.Strategy
{
    
    public class BufferEffectStrategy: IEffectStrategy 
    {
        private readonly IBufferManager _bufferManager;
        [Inject]
        public BufferEffectStrategy(IBufferManager bufferManager)
        {
            _bufferManager = bufferManager;
        }
        public Type DefType  => typeof(BufferEffectDef);
        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            return new Module((BufferEffectDef)def,owner,_bufferManager);
        }

        private sealed class Module : IEffectModule
        {
            private readonly BufferEffectDef _def;
            private readonly BaseController _caster;
            private readonly IBufferManager _bufferManager;
            
            public Module(BufferEffectDef def, BaseController owner,
                IBufferManager bufferManager)
            {
                _def = def;
                _caster = owner;
                _bufferManager = bufferManager;
            }
            
            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (ctx == null)
                {
                    if (onCancel != null) onCancel.Invoke();
                    return;
                }
               
                
                if (ctx is SkillExecutionContext skillContext == false)
                {
                    Debug.Assert(false,"check ctx it is not skillCtx");
                    return;
                }
                Collider[] targets = skillContext.HitTargets;
                StatEffect effect = new StatEffect(_def.buffType,_def.Value,ctx.Data.dataName);
                float duration = 1f;
                if (_def != null)
                {
                    duration = _def.skillduration.Resolve((SkillExecutionContext)ctx);
                }
                else
                {
                    Debug.Assert(false,"Def is null");
                }
                
                _bufferManager.ApplyActionToTargetsWithBuff(targets,effect,duration,_def.buffIconPath);
                if (onComplete != null) onComplete.Invoke(); // 버프 등록 완료 = 스킬 완료(쿨다운 시작)
                
            }
           
        }
    }
}