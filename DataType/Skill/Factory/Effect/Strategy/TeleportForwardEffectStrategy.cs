using System;
using Controller;
using DataType.Skill.Factory.Effect.Def;
using Skill;
using UnityEngine;
using UnityEngine.AI;

namespace DataType.Skill.Factory.Effect.Strategy
{
    public sealed class TeleportForwardEffectStrategy : IEffectStrategy
    {
        public Type DefType => typeof(TelePortDef);

        public IEffectModule Create(IEffectDef def, BaseController owner)
        {
            var typed = def as TelePortDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");


            NavMeshAgent agent = owner.GetComponent<NavMeshAgent>();
            Debug.Assert(agent != null, $"agent is null Owner: {owner}");
            
            return new Module(typed,agent);
        }

        private sealed class Module : IEffectModule
        {
            private const float NavMeshSampleDistance = 2f;

            private readonly TelePortDef _def;
            private readonly NavMeshAgent _agent;

            public Module(TelePortDef def,NavMeshAgent agent)
            {
                _def = def;
                _agent = agent;
            }

            public void Apply(ExecutionContext ctx, Action onComplete, Action onCancel)
            {
                if (ctx == null) { onCancel?.Invoke(); return; }
                if (ctx.Caster == null) { onCancel?.Invoke(); return; }

                BaseController controller = ctx.Caster;

                Transform t = controller.transform;
                Vector3 startPos = t.position;

                Vector3 direction = t.forward;
                direction.y = 0f; // 평면 텔포

                float mag = direction.magnitude;
                if (mag <= 0.0001f) { onCancel?.Invoke(); return; }
                direction /= mag;

                Vector3 rayOrigin = startPos + Vector3.up * _def.rayUpOffset;

                if (_def.useColliderCenterAsRayOrigin)
                {
                    Collider col = controller.GetComponent<Collider>();
                    if (col != null)
                        rayOrigin = col.bounds.center;
                }

                Vector3 targetPos = startPos + direction * _def.jumpDistance;

                if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, _def.jumpDistance, _def.obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    float back = _def.stopOffset;
                    if (back < 0f) back = 0f;

                    targetPos = hit.point - direction * back;
                }

                targetPos.y = startPos.y; // y 고정 (중요)

                if (TryResolveNavMeshTarget(startPos, targetPos, out Vector3 navMeshTargetPos) == false)
                {
                    onCancel?.Invoke();
                    return;
                }

                targetPos = navMeshTargetPos;

                MoveTo(targetPos);
                ResetVelocity(controller);

                onComplete?.Invoke();
            }

            private bool TryResolveNavMeshTarget(Vector3 startPos, Vector3 targetPos, out Vector3 resolvedPos)
            {
                if (NavMesh.SamplePosition(startPos, out NavMeshHit startHit, NavMeshSampleDistance, NavMesh.AllAreas) == false)
                {
                    resolvedPos = startPos;
                    return false;
                }

                if (NavMesh.Raycast(startHit.position, targetPos, out NavMeshHit edgeHit, NavMesh.AllAreas))
                {
                    resolvedPos = edgeHit.position;
                    return true;
                }

                if (NavMesh.SamplePosition(targetPos, out NavMeshHit targetHit, NavMeshSampleDistance, NavMesh.AllAreas))
                {
                    resolvedPos = targetHit.position;
                    return true;
                }

                resolvedPos = startHit.position;
                return true;
            }

            private void MoveTo(Vector3 targetPos)
            {
                _agent.Warp(targetPos);
            }

            private void ResetVelocity(BaseController controller)
            {
                Rigidbody rb = controller.GetComponent<Rigidbody>();
                if (rb == null)
                    return;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

        }
    }
}
