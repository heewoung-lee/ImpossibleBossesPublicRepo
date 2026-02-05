using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Controller;
using DataType.Skill.Factory.Target.Def;
using GameManagers.Target;
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
            private HashSet<GameObject> _targets;

            public Module(ChainTargetingDef def, TargetManagerProvider tm)
            {
                _def = def;
                _tm = tm;
                _chainDistance = _def.chainDistance;
                _targets = new HashSet<GameObject>();
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
                    BaseStats baseStats = target.GetComponentInChildren<BaseStats>();
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
                        if (_targets.Contains(nextColliders.gameObject)) return;
                        _targets.Add(nextColliders.gameObject);
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
                        if (_targets.Contains(col.gameObject)) continue;
                        if (col.TryGetComponent(out BaseStats stats))
                        {
                            if (_def.extraCondition != null && _def.extraCondition.CheckValidState(stats) == false)
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
                        _targets.Add(nearestCol.gameObject);
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