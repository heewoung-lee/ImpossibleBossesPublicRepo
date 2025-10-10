using System;
using GameManagers;
using ProjectContextInstaller;
using UnityEngine;
using Zenject;

namespace Scene.CommonInstaller
{
    [RequireComponent(typeof(DefaultObjectCreatorInstaller))]
    [DisallowMultipleComponent]
    public class CachingObjectInstaller : MonoInstaller
    {
        [Inject(Id=ResourcesLoaderInstaller.ResourceBindCode)] private IResourceKeyTypeProvider _resourcesLoadType;
        public override void InstallBindings()
        {
            Type dictImplementation = typeof(CachingObjectDictManager<>).MakeGenericType(_resourcesLoadType.ResourceKeyType);

            Container.BindInterfacesTo(dictImplementation)
                .AsSingle().NonLazy();
            
            
            Container.BindInterfacesAndSelfTo<UICachingService>().AsSingle().NonLazy();
        }
    }
}