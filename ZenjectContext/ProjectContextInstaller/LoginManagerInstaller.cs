using GameManagers.LoginManagement;
using System;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{

    public class LoginManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<SteamFirebaseLoginService>().AsSingle();
            Container.Bind<ILoginService>().To<SteamFirebaseLoginService>().FromResolve();
            Container.Bind<IPlayerLogininfo>().To<SteamFirebaseLoginService>().FromResolve();
            Container.Bind<IPlayerIngameLogininfo>().To<SteamFirebaseLoginService>().FromResolve();
            Container.Bind<ITickable>().To<SteamFirebaseLoginService>().FromResolve();
            Container.Bind<IDisposable>().To<SteamFirebaseLoginService>().FromResolve();
        }
    }
}
