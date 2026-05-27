using System;
using System.Linq;
using Controller;
using DataType.Skill.Factory.Decorator.Def;
using Module.PlayerModule.PlayerClassModule;
using Skill;
using Unity.Netcode;
using UnityEngine;

namespace DataType.Skill.Factory.Decorator.Strategy
{
    public interface IGetChainVfxLineRenderer
    {
        GameObject VFXStartObject { get; set; }
        GameObject VFXEndObject { get; set; }
        Vector3 VFXStartOffSetPosition { get; set; }
        Vector3 VFXEndOffsetPosition { get; set; }

        void Clear();
    }


    public sealed class ChainVfxDecoratorStrategy : IStackElementDecoratorStrategy
    {
        public Type DefType => typeof(ChainVfxDecoratorDef);
        public IDecoratorModule Create(IDecoratorDef def, BaseController owner)
        {
            var typed = def as ChainVfxDecoratorDef;
            if (typed == null)
                throw new InvalidOperationException($"Wrong def type: {def?.GetType().Name}");

            return new Module(typed);
        }

        private sealed class Module : IDecoratorModule
        {
            private readonly ChainVfxDecoratorDef _def;

            private Transform _prevAnchor;

            public Module(ChainVfxDecoratorDef def)
            {
                _def = def;
            }

            private static ulong ResolveNetworkObjectId(Transform target)
            {
                if (target == null)
                {
                    return 0;
                }

                if (target.TryGetComponent(out NetworkObject networkObject))
                {
                    return networkObject.NetworkObjectId;
                }

                // 드래곤처럼 피격 부위가 자식 본에 있을 수 있어서 부모 NetworkObject까지 따라 올라간다.
                NetworkObject parentNetworkObject = target.GetComponentInParent<NetworkObject>();
                if (parentNetworkObject != null)
                {
                    return parentNetworkObject.NetworkObjectId;
                }

                return 0;
            }

            public void Run(DecoratorPhase phase, SkillExecutionContext ctx, Action onComplete, Action onCancel)
            {
                // Tick ----

                Debug.Assert(ctx != null, $" {ctx} is null");
                Debug.Assert(_def != null, $" {_def} is null");

                if (ctx == null || _def == null)
                {
                    onComplete?.Invoke();
                    return;
                }

                Collider currentCol = null;
                if (ctx.HitTargets.Length >=1)
                {
                    currentCol = ctx.HitTargets.First();
                }
                else
                {
                    onComplete?.Invoke();
                    return;
                }
                
                Transform curTransform = currentCol.transform;
                if (curTransform == null)
                {
                    onComplete?.Invoke();
                    return;
                }

                Transform startAnchor = _prevAnchor;
                if (startAnchor == null)
                {
                    startAnchor = ctx.Caster.transform;
                }
                //이전 체인이 없다면 처음 발사하는거니깐 스타트는 캐스터가 되어야함.


                float lifeSeconds = 0.15f;
                if (_def.hitVfxDuration != null)
                    lifeSeconds = _def.hitVfxDuration.Resolve(ctx);

                // A. 데이터 준비
                ulong startAnchorID = ResolveNetworkObjectId(startAnchor);

                ulong curNetworkObjectID = ResolveNetworkObjectId(curTransform);

                var casterNetObj = ctx.Caster.GetComponent<NetworkObject>();

                
                
                
                if (casterNetObj != null && casterNetObj.IsOwner)
                {
                    ISkillNetworkRouter router = ctx.Caster.GetComponent<ISkillNetworkRouter>();

                    if (router != null)
                    {
                        router.RequestSpawnChainSkillServerRpc(
                            _def.hitvfxPath.Resolve(ctx),
                            startAnchorID, // 계산해둔 ID
                            curNetworkObjectID, // 계산해둔 ID
                            new Vector3(0, _def.startYOffset, 0),
                            new Vector3(0, _def.endYOffset, 0),
                            lifeSeconds
                        );
                    }
                }

                // 다음 Tick의 시작점은 “이번 타겟”
                _prevAnchor = curTransform;
                onComplete?.Invoke();
            }

            public void Release()
            {
                _prevAnchor = null;
            }
        }
    }
}
