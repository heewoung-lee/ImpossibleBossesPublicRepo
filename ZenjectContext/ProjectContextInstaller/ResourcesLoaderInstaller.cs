using System;
using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using GameManagers.Interface.UIManager.Implements;
using GameManagers.ResourcesEx;
using GameManagers.ResourcesEx.implementation;
using GameManagers.UI;
using GameManagers.UI.Implements;
using NetWork.NGO;
using Scene.CommonInstaller;
using UnityEngine;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class ResourcesLoaderInstaller : MonoInstaller
    {
        public const string ResourceBindCode = "ResourceBindCode";

        public override void InstallBindings()
        {
            ResourcesManagerInStall();
            UIManagerInstall();
        }

        void ResourcesManagerInStall()
        {
            #region Instantiator & Factory Bind

            Container.Bind(new Type[]
                {
                    typeof(IInstantiate),
                    typeof(IRegistrar<ICachingObjectDict>),
                    typeof(IRegistrar<IDefaultGameObjectFactory>),
                }).WithId(ResourceBindCode)
                .FromSubContainerResolve()
                .ByInstaller<InstantiatorInstaller<string>>()
                .AsSingle();


            Container.Bind<IFactoryManager>().To<ZenjectFactoryManager>().AsSingle();

            #endregion

            #region DestroyObject Bind

            Container.Bind(typeof(IDestroyObject), typeof(IRegistrar<INetworkDeSpawner>))
                .WithId(ResourceBindCode)
                .FromSubContainerResolve()
                .ByInstaller<DestroyObjectInstaller>()
                .AsSingle();

            #endregion

            #region ResourceLoader Bind

            Container.Bind<IResourcesLoader>()
                .WithId(ResourceBindCode)
                .FromSubContainerResolve()
                .ByInstaller<ResourceLoaderInstaller<string>>()
                .AsSingle();

            #endregion

            // ResourceManager 바인딩
            Container.Bind<IResourcesServices>()
                .WithId(ResourceBindCode)
                .To<ResourceManager>()
                .AsSingle();

            // Facade 연결
            Container.Bind<IResourcesServices>()
                .FromMethod(context =>
                    (IResourcesServices)context.Container.ResolveId<IResourcesServices>(ResourceBindCode))
                .AsSingle();
        }

        void UIManagerInstall()
        {
            Container.Bind<IUIorganizer>().To<UIOrganizer>().AsSingle();

            // UI 매니저들은 기존 구현체 유지
            Container.Bind<UIPopupManagerWithResources>().AsSingle();
            Container.Bind<UISceneManagerWithResources>().AsSingle();
            Container.Bind<UISubItemWithResources>().AsSingle();

            Container.Bind<IRegisterCachingUI>().To<UIPopupManagerWithResources>().FromResolve();
            Container.Bind<IRegisterCachingUI>().To<UISceneManagerWithResources>().FromResolve();

            Container.Bind<IUIPopupManager>()
                .WithId(ResourceBindCode)
                .To<UIPopupManagerWithResources>()
                .FromResolve();

            Container.Bind<IUISceneManager>()
                .WithId(ResourceBindCode)
                .To<UISceneManagerWithResources>()
                .FromResolve();

            Container.Bind<IUISubItem>()
                .WithId(ResourceBindCode)
                .To<UISubItemWithResources>()
                .FromResolve();

            Container.BindInterfacesTo<UIManagerRequestCaching>().AsSingle();
        }
    }

    public class InstantiatorInstaller<TKey> : Installer<InstantiatorInstaller<TKey>>
    {
        public override void InstallBindings()
        {
            Container.Bind(new[]
                {
                    typeof(IInstantiate),
                    typeof(IRegistrar<ICachingObjectDict>),
                    typeof(IRegistrar<IDefaultGameObjectFactory>),
                })
                .To<Instantiator>()
                .AsSingle();
        }
    }

    public class DestroyObjectInstaller : Installer<DestroyObjectInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ObjectReleaser>().AsSingle();
        }
    }

    public class ResourceLoaderInstaller<TKey> : Installer<ResourceLoaderInstaller<TKey>>
    {
        public override void InstallBindings()
        {
            Container.Bind<IResourcesLoader>()
                .To<ResourcesLoader>()
                .AsSingle();
        }
    }
}