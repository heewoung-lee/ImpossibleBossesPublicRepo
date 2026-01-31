using UI.WorldSpace.PortalIndicator;
using UnityEngine;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public class CommonCharacterInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            PortalIndicatorInstaller.Install(Container);//플레이어 포탈 상호작용 인디케이터 
        }
    }
}