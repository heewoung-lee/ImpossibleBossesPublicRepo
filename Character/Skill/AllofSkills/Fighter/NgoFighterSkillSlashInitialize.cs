using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Fighter;
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
        public class NgoFighterSkillSlashFactory : NgoZenjectFactory<NgoFighterSkillSlashInitialize>,IFighterFactoryMarker
        {
            [Inject]
            public NgoFighterSkillSlashFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Fighter/Skill/Fighter_Slash");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Fighter/Skill/Fighter_Slash";
        public override int PoolingCapacity => 5;
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            gameObject.transform.rotation = targetGo.transform.rotation;
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }
    }
}