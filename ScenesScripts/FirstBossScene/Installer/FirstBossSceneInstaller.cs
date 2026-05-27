using ScenesScripts.CommonInstaller.Interfaces;
using ScenesScripts.GamePlayScene;
using Zenject;

namespace ScenesScripts.FirstBossScene.Installer
{
    public class FirstBossSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //배틀씬 하이어라키에 올려진걸로 바인드 하고
            Container.BindInterfacesAndSelfTo<FirstBossScene>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<ISceneStarter>().To<FirstBossSceneStarter>().AsSingle();
            Container.Bind<ISceneMover>().To<GamePlaySceneMover>().AsSingle();

           //그다음 올려진 배틀씬으로 BaseScene도 바인드
            Container.Bind<BaseScene>()
                .FromResolveGetter<FirstBossScene>(t => t.GetComponent<BaseScene>())
                .AsSingle();
            Container.BindInterfacesAndSelfTo<UnitFirstBossScene>().AsSingle();
            //Container.Bind<ISceneSpawnBehaviour>().To<UnitBattleScene>().AsSingle();//2.12일 수정 계약을 인터페이스로
            //계약해버려서 테스터 들이 FromRelove로 찾을때 못찾는 문제가 발생 그래서 본씬에서 인터페이스 + 구현체 전부 바인드
        }
    }
}
