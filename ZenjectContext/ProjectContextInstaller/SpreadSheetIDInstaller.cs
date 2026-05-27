using GameManagers;
using GameManagers.DataManagement;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class SpreadSheetIDInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameDataSpreadSheet>().AsSingle();
        }
    }
}
