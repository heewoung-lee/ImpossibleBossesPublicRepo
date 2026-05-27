using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Controller;
using DataType.Skill.Factory.Target.Def;
using GameManagers.TargetManagement;
using Skill;
using Stats.BaseStats;
using UnityEngine;
using Zenject;

namespace DataType.Skill.Factory.Target.Strategy
{
    public sealed class ChainTargetingStrategy : ITargetingStrategy
    {
        private readonly TargetManagerProvider _tmProvider;

        [Inject]
        public ChainTargetingStrategy(TargetManagerProvider tmProvider)
        {
            _tmProvider = tmProvider;
        }

        public Type DefType => typeof(ChainTargetingDef);

        public ITargetingModule Create(ITargetingDef def, BaseController owner)
        {
            ChainTargetingDef typed = def as ChainTargetingDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed, _tmProvider);
        }

        private sealed class Module : ITargetingModule
        {
            private readonly ChainTargetingDef _def;
            private readonly TargetManagerProvider _tm;

            private Action _onReady;
            private Action _onCancel;
            private bool _released;
            private readonly float _chainDistance;
            private readonly HashSet<GameObject> _targets;

            public Module(ChainTargetingDef def, TargetManagerProvider tm)
            {
                _def = def;
                _tm = tm;
                _chainDistance = _def.chainDistance;
                _targets = new HashSet<GameObject>();
            }

            private BaseStats GetTargetStats(GameObject target)
            {
                if (target == null)
                {
                    return null;
                }

                BaseStats stats = target.GetComponent<BaseStats>();
                if (stats != null)
                {
                    return stats;
                }

                stats = target.GetComponentInParent<BaseStats>();
                if (stats != null)
                {
                    return stats;
                }

                return target.GetComponentInChildren<BaseStats>();
            }

            private BaseStats GetTargetStats(Collider targetCollider)
            {
                if (targetCollider == null)
                {
                    return null;
                }

                BaseStats stats = targetCollider.GetComponentInParent<BaseStats>();
                if (stats != null)
                {
                    return stats;
                }

                return targetCollider.GetComponentInChildren<BaseStats>();
            }

            private GameObject GetTargetKey(GameObject target)
            {
                // 다중 부위 콜라이더가 있어도 같은 BaseStats 루트면 같은 체인 대상로 본다.
                BaseStats stats = GetTargetStats(target);
                if (stats != null)
                {
                    return stats.gameObject;
                }

                return target;
            }

            private GameObject GetTargetKey(Collider targetCollider)
            {
                // 체인 후보 탐색도 콜라이더가 아니라 실제 대상 엔티티 기준으로 중복을 막는다.
                BaseStats stats = GetTargetStats(targetCollider);
                if (stats != null)
                {
                    return stats.gameObject;
                }

                return targetCollider != null ? targetCollider.gameObject : null;
            }

            public void BeginSelection(SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                _released = false;
                _onReady = onComplete;
                _onCancel = onCancel;

                if (ctx == null)
                {
                    _onCancel?.Invoke();
                    return;
                }

                _targets.Clear();
                // 잔재 제거
                ctx.SelectedTarget = null;
                ctx.HitTargets = Array.Empty<Collider>();

                _tm.TargetManager.StartTargeting(
                    targetLayer: _def.targetLayer,
                    highlightMat: _def.highlightMat,
                    OnSelect,
                    OnCancel,
                    IsValidTarget
                    );
                
                void OnSelect(GameObject go)
                {
                    if (_released) return;

                    ctx.SelectedTarget = go;
                    _onReady?.Invoke();
                }
                void OnCancel()
                {
                    if (_released) return;
                    _onCancel?.Invoke();
                }
                bool IsValidTarget(GameObject target)
                {
                    if (_def.extraCondition == null) return true;
                    BaseStats baseStats = GetTargetStats(target);
                    if (baseStats == null) return false;

                    return _def.extraCondition.CheckValidState(baseStats);
                }
                
            }

            public void FillHitTargets(SkillExecutionContext ctx)
            {
                if (ctx == null)
                {
                    Debug.Assert(false, "ctx is null");
                    return;
                }

                Collider nextColliders = null;

                if (ctx.HitTargets == null || ctx.HitTargets.Length == 0)
                {
                    GameObject target = ctx.SelectedTarget;
                    if (target == null)
                    {
                        Debug.Assert(false, "target is null");
                        return;
                    }

                    nextColliders = target.GetComponentInChildren<Collider>(false);
                    if (nextColliders != null)
                    {
                        GameObject targetKey = GetTargetKey(target);
                        if (targetKey != null)
                        {
                            if (_targets.Contains(targetKey)) return;
                            _targets.Add(targetKey);
                        }
                    }
                }
                else
                {
                    Collider sourceCol = ctx.HitTargets.First();

                    if (sourceCol is null) return;
                    Vector3 sourcePos = sourceCol.bounds.center;

                    Collider[] candidates = Physics.OverlapSphere(sourcePos, _chainDistance, _def.targetLayer);
                    Collider nearestCol = null;
                    float minDstSqr = float.MaxValue;

                    foreach (Collider col in candidates)
                    {
                        if (col == sourceCol) continue;

                        GameObject targetKey = GetTargetKey(col);
                        if (targetKey != null && _targets.Contains(targetKey)) continue;

                        BaseStats stats = GetTargetStats(col);
                        if (_def.extraCondition != null)
                        {
                            if (stats == null) continue;
                            if (_def.extraCondition.CheckValidState(stats) == false)
                            {
                                continue;
                            }
                        }

                        float dstSqr = (col.bounds.center - sourcePos).sqrMagnitude;

                        if (dstSqr < minDstSqr)
                        {
                            minDstSqr = dstSqr;
                            nearestCol = col;
                        }
                    }

                    if (nearestCol != null)
                    {
                        nextColliders = nearestCol;

                        GameObject targetKey = GetTargetKey(nearestCol);
                        if (targetKey != null)
                        {
                            _targets.Add(targetKey);
                        }
                    }
                }
                if (nextColliders == null)
                    ctx.HitTargets = Array.Empty<Collider>();
                else
                    ctx.HitTargets = new Collider[] { nextColliders };
            }

            public void Release()
            {
                if (_released) return;
                _released = true;

                _tm.TargetManager.StopTargeting();

                _onReady = null;
                _onCancel = null;
            }
        }
    }
}
