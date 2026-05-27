using GameManagers;
using GameManagers.LobbyManagement;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
    public class LobbyManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LobbyManager>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<DefaultDisconnectStrategy>().AsSingle();
        }
    }
}