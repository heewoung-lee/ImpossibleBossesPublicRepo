using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;

namespace UI.WorldSpace.PortalIndicator
{
    public class PortalIndicatorInstaller : Installer<PortalIndicatorInstaller>
    {
        private ISceneProvider _sceneProvider;
        [Inject]
        public void Construct(ISceneProvider sceneProvider)
        {
            _sceneProvider = sceneProvider;
        }
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UIPortalIndicator.UIPortalIndicatorFactory>().AsCached();
            
            Container.Bind<IPortalIndicator>().WithId(SceneMode.LocalTest).To<UILocalPortalIndicator>().AsCached();
            Container.Bind<IPortalIndicator>().WithId(SceneMode.MultiTest_Multi).To<UINgoPortalIndicator>().AsCached();
            Container.Bind<IPortalIndicator>().WithId(SceneMode.MultiTest_Solo).To<UINgoPortalIndicator>().AsCached();
            Container.Bind<IPortalIndicator>().WithId(SceneMode.NormalBoot).To<UINgoPortalIndicator>().AsCached();

            Container.Bind<IPortalIndicator>().FromMethod(goContext => goContext.Container.ResolveId<IPortalIndicator>(_sceneProvider.CurrentSceneMode));
            
            
        }
    }
    
}
