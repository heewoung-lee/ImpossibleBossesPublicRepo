using System;
using Controller;
using DataType.Skill.Factory.Trigger;
using GameManagers.Target;
using Scene.CommonInstaller;
using Skill;
using UnityEngine;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace DataType.Skill.Factory
{
    public interface IRuntimeSkillFactory
    {
        RuntimeSkill CreateSkill(SkillDataSO data, BaseController owner);
        bool CheckInitDone { get; }
    }

    public class RuntimeSkillFactory : IRuntimeSkillFactory,IInitializable,IDisposable
    {
        private readonly ITriggerFactory _triggerFactory;
        private readonly ISkillPipelineFactory _pipelineFactory;
        private readonly SignalBus _runTimeSkillfactoryReadySignal;

        private bool _checkinitdone = false;

        public bool CheckInitDone => _checkinitdone;
        
        [Inject]
        public RuntimeSkillFactory(
            SignalBus runTimeSkillfactoryReadySignal,
            ITriggerFactory triggerFactory,
            ISkillPipelineFactory pipelineFactory)
        {
            _runTimeSkillfactoryReadySignal = runTimeSkillfactoryReadySignal;
            _triggerFactory = triggerFactory;
            _pipelineFactory = pipelineFactory;
        }
        public void Initialize()
        {
            _runTimeSkillfactoryReadySignal.Fire(new RuntimeSkillFactoryReadySignal());
            _checkinitdone = true;
            
        }
        public void Dispose()
        {
            _checkinitdone = false;
        }
        public RuntimeSkill CreateSkill(SkillDataSO data, BaseController owner)
        {
            if (data == null || owner == null)
            {
                Debug.LogError("[RuntimeSkillFactory] data/owner is null");
                return null;
            }

            var trigger = _triggerFactory.GetTrigger(data.trigger);
            var pipeline = _pipelineFactory.Create(data, owner);

            if (trigger == null || pipeline == null)
            {
                Debug.LogError($"[RuntimeSkillFactory] Create failed. Skill: {data.name}");
                return null;
            }

            return new RuntimeSkill(data, trigger, pipeline, owner);
        }


     
    }
}
