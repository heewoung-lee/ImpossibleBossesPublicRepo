using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.Fighter
{
    public class NgoFighterSkillDeterminationInitialize : NgoPoolingInitializeBase
    {
        public class NgoFighterSkillDeterminationFactory : NgoZenjectFactory<NgoFighterSkillDeterminationInitialize>
        {
            [Inject]
            public NgoFighterSkillDeterminationFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/SkillVFX/Shield_Determination");
            }
        }
        public override string PoolingNgoPath => "Prefabs/Player/SkillVFX/Shield_Determination";

        public override int PoolingCapacity => 5;

    }
}
