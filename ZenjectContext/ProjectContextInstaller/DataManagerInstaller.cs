using GameManagers;
using GameManagers.DataManagement;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class DataManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<DataManager>().AsSingle().NonLazy();
        }
    }
}
