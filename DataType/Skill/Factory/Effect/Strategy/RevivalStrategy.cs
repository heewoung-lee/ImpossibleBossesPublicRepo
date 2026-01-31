using System;
using System.Linq;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using Module.PlayerModule.PlayerClassModule;
using Skill;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public sealed class RevivalStrategy: IEffectStrategy
    {
        public Type DefType  => typeof(RevivalDef);
        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            return new Module();
        }
        
        private sealed class Module : IEffectModule
        {
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

                if (skillContext.HitTargets.Length > 0)
                {
                    Collider target = skillContext.HitTargets.First();
                    ulong networkId = target.GetComponent<NetworkObject>().NetworkObjectId;

                    if (target.TryGetComponent(out SkillNetworkRouter networkRouter))
                    {
                        networkRouter.RequestPlayerChangeStateRpc(networkId,ChangePlayerStateID.Idle);
                    }
                    else
                    {
                        Debug.Assert(false,$"{target.name} doesn't have a SkillNetworkRouter");
                    }
                }
                if (onComplete != null) onComplete.Invoke(); // 버프 등록 완료 = 스킬 완료(쿨다운 시작)
                
            }
           
        }
    }
}