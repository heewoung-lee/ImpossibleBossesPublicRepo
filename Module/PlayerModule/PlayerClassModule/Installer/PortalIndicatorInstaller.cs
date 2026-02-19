using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;

namespace UI.WorldSpace.PortalIndicator
{
    public class PortalIndicatorInstaller : Installer<PortalIndicatorInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UIPortalIndicator.UIPortalIndicatorFactory>().AsSingle();
            Container.Bind<IPortalIndicator>().To<UINgoPortalIndicator>().AsSingle();
        }
    }
    
}
