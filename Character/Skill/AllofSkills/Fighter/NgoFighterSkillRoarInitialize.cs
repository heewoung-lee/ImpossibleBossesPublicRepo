using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Fighter;
using NetWork.BaseNGO;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.Fighter
{
    public class NgoFighterSkillRoarInitialize : NgoPoolingInitializeBase
    {
        
        public class NgoFighterSkillRoarFactory : NgoZenjectFactory<NgoFighterSkillRoarInitialize>,IFighterFactoryMarker
        {
            [Inject]
            public NgoFighterSkillRoarFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Fighter/Skill/Aura_Roar");
            }
        }


        public override string PoolingNgoPath => "Prefabs/Player/VFX/Fighter/Skill/Aura_Roar";

        public override int PoolingCapacity => 5;
        
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }
    }
}