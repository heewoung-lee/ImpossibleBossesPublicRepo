using GameManagers.Interface.ResourcesManager;
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
        public class NgoFighterSkillRoarFactory : NgoZenjectFactory<NgoFighterSkillRoarInitialize>
        {
            [Inject]
            public NgoFighterSkillRoarFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/SkillVFX/Aura_Roar");
            }
        }


        public override string PoolingNgoPath => "Prefabs/Player/SkillVFX/Aura_Roar";

        public override int PoolingCapacity => 5;
    }
}