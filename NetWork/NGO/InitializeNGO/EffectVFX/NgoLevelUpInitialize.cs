using GameManagers.Interface.ResourcesManager;
using NetWork.BaseNGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.InitializeNGO.EffectVFX
{
    public class NgoLevelUpInitialize : NgoPoolingInitializeBase
    {
        public class NgoLevelUpFactory : NgoZenjectFactory<NgoLevelUpInitialize>
        {
            public NgoLevelUpFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/SkillVFX/Level_up");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/SkillVFX/Level_up";
        public override int PoolingCapacity => 5;
    }
}