using GameManagers;
using GameManagers.Interface.VFXManager;
using GameManagers.Interface.VFXManager.Implementation;
using UnityEngine;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class VFXManagerInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.Bind<IVFXManagerServices>().To<VFXManager>().AsSingle();
            Container.BindInterfacesTo<ParticleGenerator>().AsSingle();
        }
    }
    
    
  
}
