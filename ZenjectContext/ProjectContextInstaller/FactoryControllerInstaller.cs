using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.ResourcesManager.implementation;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Tools;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace ZenjectContext.ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class FactoryControllerInstaller : MonoInstaller
    {
        public const string FactoryBindKey = "FactoryBinder";
        
        public override void InstallBindings()
        {
            Container.Bind<IFactoryController>()
                .To<FactoryController>()
                .AsSingle();

            Container.Bind<IRegistrar<IFactoryCreator>>()
                .WithId(FactoryBindKey)
                .FromResolveGetter<IFactoryController>(factoryController => (IRegistrar<IFactoryCreator>)factoryController);
            
            
            Container.BindInterfacesTo<GameObjectContextFactory>().AsSingle();
            
        }
    }
}
