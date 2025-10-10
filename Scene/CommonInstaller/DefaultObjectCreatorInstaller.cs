using GameManagers.Interface;
using GameManagers.Interface.CommonImplements;
using GameManagers.Interface.ResourcesManager;
using Scene.CommonInstaller.Tools;
using UnityEngine;
using Zenject;

namespace Scene.CommonInstaller
{
    [DisallowMultipleComponent]
    public class DefaultObjectCreatorInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IDefaultGameObjectFactory>().To<DefaultGameObjectFactory>().AsSingle().NonLazy();
            
            //Container.Bind<IFactoryCreator>().To<SceneContextFactory>().AsSingle();
            //TODO:SceneContextFactory없애는 방향으로 가야함.,
        }
    }
}
