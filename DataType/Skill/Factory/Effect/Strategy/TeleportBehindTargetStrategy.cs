using System;
using System.Linq;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using Skill;
using UnityEngine;
using UnityEngine.AI;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public sealed class TeleportBehindTargetStrategy : IEffectStrategy
    {
        public Type DefType => typeof(TeleportBehindTargetDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            var typed = def as TeleportBehindTargetDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            NavMeshAgent agent = owner.GetComponent<NavMeshAgent>();
            return new Module(typed, agent);
        }

        private sealed class Module : IEffectModule
        {
            private readonly TeleportBehindTargetDef _def;
            private readonly NavMeshAgent _agent;

            public Module(TeleportBehindTargetDef def, NavMeshAgent agent)
            {
                _def = def;
                _agent = agent;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                
                if (ctx == null || ctx.Caster == null) 
                { 
                    onCancel?.Invoke(); 
                    return; 
                }
                
                
                if (ctx is SkillExecutionContext skillContext == false)
                {
                    onCancel?.Invoke(); 
                    Debug.Assert(false,"ctx is not SkillExecutionContext");
                    return; 
                }

                if (skillContext.HitTargets == null || skillContext.HitTargets.Length == 0)
                {
                    onCancel?.Invoke(); 
                    Debug.Assert(false,"there is not target");
                    return; 
                }
                
                

                BaseController caster = ctx.Caster;
                int random = UnityEngine.Random.Range(0, skillContext.HitTargets.Length);
                Transform targetTr = skillContext.HitTargets[random].transform;
                //여러게 들어올 수 있는 경우가 있는데 재미를 위해서 랜덤으로 할까...

                Vector3 targetForward = targetTr.forward;
                targetForward.y = 0;
                targetForward.Normalize();

                Vector3 destPos = targetTr.position - (targetForward * _def.distanceBehind);
                
                Vector3 checkOrigin = targetTr.position + Vector3.up; 
                Vector3 checkDir = -targetForward; // 뒤쪽 방향

                //벽이 있다면 보정
                if (Physics.Raycast(checkOrigin, checkDir, out RaycastHit hit, _def.distanceBehind, 
                        LayerMask.GetMask( "Ground"), QueryTriggerInteraction.Ignore))
                {
                     destPos = hit.point + (targetForward * 0.5f);
                }

                //NavMesh 위에 있는 유효한 위치인지 샘플링
                // 벽 뚫고 나가는 것 방지 및 맵 밖으로 나가는 것 방지
                if (NavMesh.SamplePosition(destPos, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
                {
                    destPos = navHit.position;
                }
                else
                {
                    destPos = targetTr.position;
                }

                _agent.Warp(destPos);

                //이동 후 타겟 바라보기
                if (_def.lookAtTarget)
                {
                    caster.transform.LookAt(targetTr.position);
                }

                // 물리 속도 초기화 (관성 제거)
                ResetVelocity(caster);

                onComplete?.Invoke();
            }

            private void ResetVelocity(BaseController controller)
            {
                Rigidbody rb = controller.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}