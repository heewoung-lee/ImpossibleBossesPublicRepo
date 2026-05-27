using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene.Spawner;
using Zenject;

namespace ScenesScripts.GamePlayScene.Installer
{
    public class GamePlaySceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayScene>().FromComponentInHierarchy().AsSingle();
               
            Container.Bind<ISceneStarter>().To<PlaySceneStarter>().AsSingle();
            Container.Bind<ISceneMover>().To<GamePlaySceneMover>().AsSingle();
            
            
            Container.Bind<BaseScene>()
                .FromResolveGetter<PlayScene>(t => t.GetComponent<BaseScene>())
                .AsSingle();

            Container.Bind<ISceneSpawnBehaviour>().To<UnitNetGamePlayScene>().AsCached();
        }
    }
}
