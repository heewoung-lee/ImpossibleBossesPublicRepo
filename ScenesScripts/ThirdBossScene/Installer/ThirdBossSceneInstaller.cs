using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using Zenject;

namespace ScenesScripts.ThirdBossScene.Installer
{
    public class ThirdBossSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ThirdBossScene>().FromComponentInHierarchy().AsSingle();

            Container.Bind<ISceneStarter>().To<ThirdBossSceneStarter>().AsSingle();
            Container.Bind<ISceneMover>().To<GamePlaySceneMover>().AsSingle();

            Container.Bind<BaseScene>()
                .FromResolveGetter<ThirdBossScene>(t => t.GetComponent<BaseScene>())
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitThirdBossScene>().AsSingle();
        }
    }
}
