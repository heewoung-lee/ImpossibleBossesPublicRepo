using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.ResourcesManager.implementation;
using Zenject;

namespace ProjectContextInstaller
{
    public class CoroutineRunnerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ICoroutineRunner>()
                .To<CoroutineRunner>()
                .FromNewComponentOnNewGameObject() // 새 GO 생성 + 컴포넌트 부착 + 주입
                .WithGameObjectName("@CoroutineRunner")
                .UnderTransform(ProjectContext.Instance.transform) //이러면 ProjectContext에 붙어서 DontDestroy 옵션이 자동으로 설정됨.
                .AsSingle();
        }
    }
}
