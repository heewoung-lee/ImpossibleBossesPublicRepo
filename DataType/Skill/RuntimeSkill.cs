using System;
using Controller;
using DataType;
using DataType.Skill;
using DataType.Skill.Factory.Trigger;
using UnityEngine;
using Util;

namespace Skill
{
    public interface ISkillPipeline
    {
        void Execute(SkillExecutionContext ctx, Action onComplete, Action onCancel);
    }
    
    
    public sealed class SkillExecutionContext : ExecutionContext
    {
        public SkillDataSO SkillData => (SkillDataSO)Data;
        public GameObject SelectedTarget { get; set; }
        public Vector3? SelectedPoint { get; set; }
        public Collider[] HitTargets { get; set; }= Array.Empty<Collider>();
        public bool IsCancelled { get; private set; }
        public void Cancel() => IsCancelled = true;
        public SkillExecutionContext(BaseController caster, SkillDataSO data)
            : base(caster, data) { }
    }


    /// <summary>
    /// 사용 가능 여부를 검사하고
    /// 스킬 실행을 시작시키고
    /// 완료됐을 때만 쿨타임/UI 이벤트를 처리하는 ‘껍데기’
    /// </summary>
    public class RuntimeSkill
    {
        public SkillDataSO Data { get; }

        private readonly ISkillTriggerStrategy _trigger;
        private readonly ISkillPipeline _pipeline;
        private readonly BaseController _owner;

        private Action _onCompleteSkill;
        public event Action OnCompleteSkill
        {
            add { UniqueEventRegister.AddSingleEvent(ref _onCompleteSkill, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _onCompleteSkill, value); }
        }

        private float _lastUsedTime;
        public float CurrentCooldown => Mathf.Max(0, (_lastUsedTime + Data.cooldown) - Time.time);
        public bool IsReady => CurrentCooldown <= 0;
        private bool _isExecuting;
        public RuntimeSkill(SkillDataSO data, ISkillTriggerStrategy trigger, ISkillPipeline pipeline, BaseController owner)
        {
            Data = data;
            _trigger = trigger;
            _pipeline = pipeline;
            _owner = owner;
            _lastUsedTime = -999f;
        }

        public void Use()
        {
            if (_isExecuting) return;
            if (!IsReady) return;
            if (_owner.IsAnimationLocked) return;

            if (Data.trigger == null)
            {
                Debug.LogError($"[RuntimeSkill] triggerDef is null. Skill: {Data.name}");
                return;
            }

            if (_trigger == null)
            {
                Debug.LogError($"[RuntimeSkill] trigger strategy is null. Skill: {Data.name}");
                return;
            }

            if (_pipeline == null)
            {
                Debug.LogError($"[RuntimeSkill] pipeline is null. Skill: {Data.name}");
                return;
            }

            SkillExecutionContext ctx = new SkillExecutionContext(_owner, Data);
            
            _isExecuting = true;
            bool finished = false;

            _trigger.Fire( ctx, Data.trigger, OnCommit, OnCancel);


            void OnCommit()
            {
                _pipeline.Execute(ctx, FinishComplete, FinishCancel);
            }

            void OnCancel()
            {
                FinishCancel();
            }
            
            
            void FinishCancel()
            {
                if (finished) return;
                finished = true;
                Debug.Log("[RuntimeSkill] FinishCancel");
                _isExecuting = false;
                // cooldown/event X
            }

            void FinishComplete()
            {
                if (finished) return;
                finished = true;
                Debug.Log("[RuntimeSkill] FinishComplete");
                _isExecuting = false;
                _lastUsedTime = Time.time;
                _onCompleteSkill?.Invoke();
                Debug.Log("[RuntimeSkill] OnCompleteSkill invoked");
            }
        }
    }
}