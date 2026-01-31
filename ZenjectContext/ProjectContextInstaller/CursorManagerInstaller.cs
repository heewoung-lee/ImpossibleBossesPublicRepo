using System;
using Controller;
using Zenject;

public class CursorManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<CursorManager>().AsSingle().NonLazy();
    }
}
