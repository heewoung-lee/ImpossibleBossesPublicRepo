using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Controller;
using DataType.Skill.Factory.Trigger;
using GameManagers.ResourcesEx;
using GameManagers.Target;
using Scene.CommonInstaller;
using Skill;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;
using Object = UnityEngine.Object;

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
        private List<SkillDataSO> _skills;
        
        
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

            _skills = new List<SkillDataSO>();
        }
        public void Initialize()
        {
            _runTimeSkillfactoryReadySignal.Fire(new RuntimeSkillFactoryReadySignal());
            _checkinitdone = true;
            
        }
        public void Dispose()
        {
            _checkinitdone = false;
            foreach (SkillDataSO skilldata in _skills)
            {
                Object.Destroy(skilldata);
            }
        }
        public RuntimeSkill CreateSkill(SkillDataSO data, BaseController owner)
        {
            if (data == null || owner == null)
            {
                UtilDebug.LogError("[RuntimeSkillFactory] data/owner is null");
                return null;
            }

            SkillDataSO dataInstance = Object.Instantiate(data);
            _skills.Add(dataInstance);
            
            ISkillTriggerStrategy trigger = _triggerFactory.GetTrigger(dataInstance.trigger);
            ISkillPipeline pipeline = _pipelineFactory.Create(dataInstance, owner);

            if (trigger == null || pipeline == null)
            {
                UtilDebug.LogError($"[RuntimeSkillFactory] Create failed. Skill: {dataInstance.name}");
                return null;
            }

            return new RuntimeSkill(dataInstance, trigger, pipeline, owner);
        }


     
    }
}
