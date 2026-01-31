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
    public class NgoFighterSkillTauntInitialize : NgoPoolingInitializeBase
    {
        public class NgoFighterSkillTauntFactory : NgoZenjectFactory<NgoFighterSkillTauntInitialize>,IFighterFactoryMarker
        {
            [Inject]
            public NgoFighterSkillTauntFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Fighter/Skill/Taunt_Player");
            }
        }
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Fighter/Skill/Taunt_Player";

        public override int PoolingCapacity => 5;


        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }
    }
}