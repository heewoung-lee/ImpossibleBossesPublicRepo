
using Zenject;

namespace ScenesScripts.RoomScene.Installer
{
    public class RoomSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RoomPlayScene>().FromComponentInHierarchy().AsSingle();
            
            Container.BindInterfacesTo<RoomSceneStarter>().AsSingle();
            
            
            Container.Bind<BaseScene>()
                .FromResolveGetter<RoomPlayScene>(t => t.GetComponent<BaseScene>())
                .AsSingle();

            
        }
    }
}
