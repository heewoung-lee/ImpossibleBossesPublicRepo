using GameManagers;
using Zenject;

namespace ProjectContextInstaller
{
    public class LootItemManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LootItemManager>().AsSingle();
        }
    }
}
