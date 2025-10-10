using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using NetWork.NGO;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Skill.AllofSkills.Fighter
{
    public class NgoFighterSkillEnemyTauntInitialize : NgoPoolingInitializeBase
    {
        public class NgoFighterSkillEnemyTauntFactory : NgoZenjectFactory<NgoFighterSkillEnemyTauntInitialize>
        {
            [Inject]
            public NgoFighterSkillEnemyTauntFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/SkillVFX/Taunt_Enemy");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/SkillVFX/Taunt_Enemy";

        public override int PoolingCapacity => 5;
    }
}