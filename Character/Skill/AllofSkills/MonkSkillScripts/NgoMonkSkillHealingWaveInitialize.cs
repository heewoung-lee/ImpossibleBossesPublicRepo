using Character.Skill.AllofSkills.Mage;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule;
using Module.PlayerModule.PlayerClassModule.Monk;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.MonkSkillScripts
{
    public class NgoMonkSkillHealingWaveInitialize : NgoPoolingInitializeBase,IChainVfxHandler
    {
        
        private IResourcesServices _resourcesServices;

        [Inject]
        public void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        public class NgoMonkSkillHealingWaveFactory : NgoZenjectFactory<NgoMonkSkillHealingWaveInitialize>,
            IMonkFactoryMarker
        {
            [Inject]
            public NgoMonkSkillHealingWaveFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/MonkSkillPrefab/HealingWave");
            }
        }
        
        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position + (Vector3.up * 0.5f);
        }
        public void SetChainData(ulong startId, ulong endId, Vector3 startOff, Vector3 endOff, float dur)
        {
            if (TryGetComponent(out ChainVfxNetSync netSync))
            {
                netSync.InitializeChainData(startId, endId, startOff, endOff);
            }
            _resourcesServices.DestroyObject(gameObject,dur);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/MonkSkillPrefab/HealingWave";
        public override int PoolingCapacity => 5;
    }
}
