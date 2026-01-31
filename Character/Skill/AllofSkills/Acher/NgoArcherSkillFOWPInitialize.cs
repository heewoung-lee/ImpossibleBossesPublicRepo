
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Acher
{
    public class NgoArcherSkillFOWPInitialize: NgoPoolingInitializeBase
    {
        public class ArcherFOWPFactory : NgoZenjectFactory<NgoArcherSkillFOWPInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public ArcherFOWPFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Archer/Skill/FocusOnWeakPoint");
            }
        }
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/FocusOnWeakPoint";
        public override int PoolingCapacity => 5;
    }
}