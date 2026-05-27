using System;
using NPC.Dummy;
using Zenject;

public class GamePlaySceneBindTest : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<Dummy.DummyFactory>().AsSingle();
    }
}
