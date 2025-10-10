using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using NetWork;
using NetWork.NGO.Interface;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;


namespace VFX
{
    public class DustParticleInitialize : MonoBehaviour, ISpawnBehavior
    {
        public class DustParticleFactory : GameObjectContextFactory<DustParticleInitialize>
        {
            public DustParticleFactory(DiContainer container, IResourcesServices loadService,
                IFactoryController registerableFactory) : base(container, loadService, registerableFactory)
            {
                _requestGO = loadService.Load<GameObject>($"Prefabs/Particle/AttackEffect/Dust_Particle");
            }
        }

        public class BiGDustParticleFactory : GameObjectContextFactory<DustParticleInitialize>
        {
            public BiGDustParticleFactory(DiContainer container, IResourcesServices loadService,
                IFactoryController registerableFactory) : base(container, loadService, registerableFactory)
            {
                _requestGO = loadService.Load<GameObject>($"Prefabs/Particle/AttackEffect/Dust_Particle_Big");
            }
        }
        
        private IVFXManagerServices _vfxManager;
        
        [Inject] 
        public void Construct(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }
        public void SpawnObjectToLocal(in SpawnParamBase param,string path = null)
        {
            _vfxManager.InstanceObjConvertToParticle(gameObject,path, param.ArgPosVector3, param.ArgFloat);
        }
    }
}
