using GameManagers;
using GameManagers.GameManagerExManagement;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class GameManagerExInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameManagerEx>().AsSingle();
        }
    }
}
