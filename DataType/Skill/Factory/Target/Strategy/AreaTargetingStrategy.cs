 using System;
 using System.Collections.Generic;
 using Controller;
 using DataType.Skill.Factory.Target.Def;
 using GameManagers.Target;
 using Skill;
 using UnityEngine;
 using Zenject;

 namespace DataType.Skill.Factory.Target.Strategy
 {
     public sealed class AreaTargetingStrategy : ITargetingStrategy
     {
         private readonly TargetManagerProvider _targetManager;
         [Inject]
         public AreaTargetingStrategy(TargetManagerProvider targetManager)
         {
             _targetManager = targetManager;
         }
          public Type DefType => typeof(AreaTargetSelectionDef);

         public ITargetingModule Create(ITargetingDef def, BaseController owner)
         {
             AreaTargetSelectionDef typed = def as AreaTargetSelectionDef;
             if (typed == null)
                 throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

             return new Module(typed, owner, _targetManager);
         }

         private sealed class Module : ITargetingModule
         {
             private readonly AreaTargetSelectionDef _def;
             private readonly BaseController _owner;
             private readonly TargetManagerProvider  _tmProvider;

             private Action _onReady;
             private Action _onCancel;
             private bool _released;

             public Module(AreaTargetSelectionDef def, BaseController owner, TargetManagerProvider tmProvider)
             {
                 _def = def;
                 _owner = owner;
                 _tmProvider = tmProvider;
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
                 ctx.SelectedPoint = null;
                 ctx.HitTargets = Array.Empty<Collider>();

                 LayerMask selectLayer = _def.affectLayer;

                 float radius = _def.scaleRef.Resolve(ctx);
                 
                 _tmProvider.TargetManager.StartAreaTargeting(
                     radius: radius,
                     targetLayer: selectLayer,
                     indicatorMat: _def.indicatorMat,
                     onSelected: pos =>
                     {
                         if (_released) return;
                         ctx.SelectedPoint = pos;
                         _onReady?.Invoke();
                     },
                     onCanceled: () =>
                     {
                         if (_released) return;
                         _onCancel?.Invoke();
                     }
                 );
                 
             }

             public void FillHitTargets(SkillExecutionContext ctx)
             {
                 if (ctx == null)
                 {
                     Debug.Assert(false,"ctx is null");
                     return;
                 }

                 ctx.HitTargets = Array.Empty<Collider>();

                 if (ctx.SelectedPoint == null)
                 {
                     Debug.Assert(false,$"{ctx} SelectPoint Vector is null");
                     return;
                 }
                 Vector3 center = ctx.SelectedPoint.Value;
                 float radius = _def.scaleRef.Resolve(ctx);
                 Collider[] cols = Physics.OverlapSphere(center, radius, _def.affectLayer);
                 
                 List<Collider> filteredTargets = new List<Collider>();
                 HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

                 
                 //한캐릭터의 콜라이더가 여러개일 수 있음. 그래서 필터로 중복 제거
                 foreach (Collider col in cols)
                 {
                     if (uniqueObjects.Contains(col.gameObject)) 
                         continue;

                     uniqueObjects.Add(col.gameObject);
                     filteredTargets.Add(col);
                 }

                 ctx.HitTargets = filteredTargets.ToArray();
                 ctx.HitTargets = cols;
             }

             public void Release()
             {
                 if (_released) return;
                 _released = true;

                 _tmProvider.TargetManager.StopTargeting();

                 _onReady = null;
                 _onCancel = null;
             }
         }
     }
 }