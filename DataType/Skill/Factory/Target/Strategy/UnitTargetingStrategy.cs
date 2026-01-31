using System;
using Controller;
using DataType.Skill.Factory.Target.Def;
using GameManagers.Target;
using Skill;
using Stats.BaseStats;
using UnityEngine;

namespace DataType.Skill.Factory.Target.Strategy
{
  
    public sealed class UnitTargetingStrategy : ITargetingStrategy
    {
        public Type DefType => typeof(SingleTargetSelectionDef);

        private readonly TargetManagerProvider _tm;

        public UnitTargetingStrategy(TargetManagerProvider tm)
        {
            _tm = tm;
        }

        public ITargetingModule Create(ITargetingDef def, BaseController owner)
            => new Module((SingleTargetSelectionDef)def, _tm);

        private sealed class Module : ITargetingModule
        {
            private readonly SingleTargetSelectionDef _def;
            private readonly TargetManagerProvider _tm;

            
            private Action _onReady;
            private Action _onCancel;
            private bool _released;

            public Module(SingleTargetSelectionDef def, TargetManagerProvider tm)
            {
                _def = def;
                _tm = tm;
            }

           
            public void BeginSelection(SkillExecutionContext ctx, Action onReady, Action onCancel)
            {
                _released = false;
                _onReady = onReady;
                _onCancel = onCancel;

                if (ctx == null)
                {
                    _onCancel?.Invoke();
                    return;
                }

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
                    if (_def.extraTargetCondition == null) return true;
                    BaseStats baseStats = target.GetComponentInChildren<BaseStats>();
                    if (baseStats == null) return false;

                    return _def.extraTargetCondition.CheckValidState(baseStats);
                }
            }

            public void FillHitTargets(SkillExecutionContext ctx)
            {
                if (ctx == null)
                {
                    Debug.Assert(false,"ctx is null");
                    return;
                }
                // 항상 초기화해서 잔재를 막는다
                ctx.HitTargets = Array.Empty<Collider>();

                GameObject target = ctx.SelectedTarget;
                if (target == null) return;

                //1.31일 수정 여러개의 콜라이더를 받으면 중복으로 실행됨.
                //
                Collider cols = target.GetComponentInChildren<Collider>(false);
                if (cols == null)
                {
                    Debug.Assert(false,$"{target.gameObject.name} collider is null");
                    return;
                }
                
                ctx.HitTargets = new []{cols};
            }

            public void Release()
            {
                Debug.Log("[UnitTargetingModule] Release");
                
                if (_released) return;
                _released = true;

                _tm.TargetManager.StopTargeting();

                _onReady = null;
                _onCancel = null;
            }
        }
    }
}
