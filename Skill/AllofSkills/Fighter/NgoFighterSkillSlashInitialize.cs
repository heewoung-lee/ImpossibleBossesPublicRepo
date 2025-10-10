using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using VFX;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.Fighter
{
    public class NgoFighterSkillSlashInitialize : NgoPoolingInitializeBase
    {
        public class NgoFighterSkillSlashFactory : NgoZenjectFactory<NgoFighterSkillSlashInitialize>
        {
            [Inject]
            public NgoFighterSkillSlashFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/SkillVFX/Fighter_Slash");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/SkillVFX/Fighter_Slash";
        public override int PoolingCapacity => 5;

        public override void StartParticleOption(Action<GameObject> callBack)
        {
            base.StartParticleOption(callBack);
            ParticleNgo.transform.rotation = TargetNgo.transform.rotation;
        }
    }
}