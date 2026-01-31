using GameManagers;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class VFXManagerInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<VFXManager>().AsSingle();
        }
    }
    
    
  
}
