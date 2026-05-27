using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using Zenject;

namespace ScenesScripts.SecondBossScene.Installer
{
    public class SecondBossSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //배틀씬 하이어라키에 올려진걸로 바인드 하고
            Container.BindInterfacesAndSelfTo<SecondBossScene>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<ISceneStarter>().To<SecondBossSceneStarter>().AsSingle();
            Container.Bind<ISceneMover>().To<GamePlaySceneMover>().AsSingle();

           //그다음 올려진 배틀씬으로 BaseScene도 바인드
            Container.Bind<BaseScene>()
                .FromResolveGetter<SecondBossScene>(t => t.GetComponent<BaseScene>())
                .AsSingle();
            
            //Container.BindInterfacesAndSelfTo<MockSecondBossSceneSpawner>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<UnitSecondBossScene>().AsSingle();
        }
    }
}
