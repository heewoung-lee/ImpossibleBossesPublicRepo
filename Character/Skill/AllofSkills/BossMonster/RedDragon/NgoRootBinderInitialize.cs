using GameManagers.ResourcesExManagement;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class NgoRootBinderInitialize : NgoPoolingInitializeBase
    {
        public class RootBinderFactory : NgoZenjectFactory<NgoRootBinderInitialize>
        {
            [Inject]
            public RootBinderFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Minion/RootBinder");
            }
        }

        public override string PoolingNgoPath => "Prefabs/Enemy/Minion/RootBinder";
        public override int PoolingCapacity => 5;
    }
}
