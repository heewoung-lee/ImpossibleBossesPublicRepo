using ScenesScripts.CommonInstaller.InGameInstaller.Implements;
using Zenject;

namespace ScenesScripts.CommonInstaller.InGameInstaller
{
    internal class CommonPlaySceneNetworkDisconnectInstaller : Installer<CommonPlaySceneNetworkDisconnectInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InGameSceneNetworkDisConnector>().AsSingle();
        }
    }
}
