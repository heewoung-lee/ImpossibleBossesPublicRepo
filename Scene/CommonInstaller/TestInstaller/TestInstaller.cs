using Test.TestZenject;
using Zenject;

namespace Scene.CommonInstaller.TestInstaller
{
    public class TestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<TestLifeCycle.TestLifeCycleFactory>()
                .AsSingle();
        }
    }
}
