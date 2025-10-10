using GameManagers.Interface.ResourcesManager;
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
        public class NgoFighterSkillTauntFactory : NgoZenjectFactory<NgoFighterSkillTauntInitialize>
        {
            [Inject]
            public NgoFighterSkillTauntFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/SkillVFX/Taunt_Player");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/SkillVFX/Taunt_Player";

        public override int PoolingCapacity => 5;
    }
}