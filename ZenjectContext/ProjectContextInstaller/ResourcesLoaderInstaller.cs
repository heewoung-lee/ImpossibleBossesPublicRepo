using System;
using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.ResourcesManager.implementation;
using GameManagers.Interface.UIManager;
using GameManagers.Interface.UIManager.Implements;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Util;
using Zenject;

namespace ProjectContextInstaller
{
    public enum ResourceLoadType
    {
        ResourcesType,
        AddressablesType
    }


    public interface IResourceKeyTypeProvider
    {
        public Type ResourceKeyType { get; }
    }

    public class ResourceKeyTypeProvider : IResourceKeyTypeProvider
    {
        private Type _resourceKeyType;
        public Type ResourceKeyType => _resourceKeyType;

        public ResourceKeyTypeProvider(Type resourceKeyType)
        {
            _resourceKeyType = resourceKeyType;
        }
    }


    [DisallowMultipleComponent]
    public class ResourcesLoaderInstaller : MonoInstaller
    {
        public const string ResourceBindCode = "ResourceBindCode";
        public ResourceLoadType resourceLoadRequestType;
        private ResourceKeyTypeProvider _resoucesType;


        public override void InstallBindings()
        {
            switch (resourceLoadRequestType)
            {
                case ResourceLoadType.ResourcesType:
                    _resoucesType = new ResourceKeyTypeProvider(typeof(string));
                    break;

                case ResourceLoadType.AddressablesType:
                    //TODO: 나중에 어드레서블로 전환하면 추가할것
                    // _resourcesType = typeof(UnityEngine.AddressableAssets.AssetReference);
                    break;

                default:
                    _resoucesType = new ResourceKeyTypeProvider(typeof(string));
                    break;
            }


            Container.Bind<IResourceKeyTypeProvider>()
                .WithId(ResourceBindCode)
                .FromInstance(_resoucesType)
                .AsSingle();
            //SceneContext가 확인해야 하므로 타입 바인드

            Container.Bind(typeof(ResourceLoadType))
                .WithId(ResourceBindCode)
                .FromInstance(resourceLoadRequestType)
                .AsSingle()
                .CopyIntoAllSubContainers();
            //씬인스톨러가 확인할 수 있게 아래 컨테이너들에게 까지 복사

            ResourcesManagerInStall(); //만들어진 타입을 이용해서 리소스 매니저 바인드

            UIManagerInstall(); //UI 매니저 바인드
        }

        void ResourcesManagerInStall()
        {
            #region IInstantiateBind
            Type dictIface = typeof(ICachingObjectDict<>).MakeGenericType(_resoucesType.ResourceKeyType);
            Type instantiatorIface = typeof(IInstantiate<>).MakeGenericType(_resoucesType.ResourceKeyType);
            Type registrarIface =typeof(IRegistrar<>).MakeGenericType(dictIface); //IRegistrar<ICachingObjectDict<Type>>
            Type defaultGameObjectFactory = typeof(IRegistrar<>).MakeGenericType(typeof(IDefaultGameObjectFactory));
            Type factoryCreator = typeof(IRegistrar<>).MakeGenericType(typeof(IFactoryCreator));
            
            Type instSubInstallerT   = typeof(InstantiatorInstaller<>).MakeGenericType(_resoucesType.ResourceKeyType);
            
            
            Container.Bind(new Type[]
                {
                    instantiatorIface,
                    registrarIface,
                    defaultGameObjectFactory,
                    factoryCreator
                }).WithId(ResourceBindCode)
                .FromSubContainerResolve()
                .ByInstaller(instSubInstallerT)
                .AsSingle();
            #endregion
            
            #region IDestroyBind
            Container.Bind(typeof(IDestroyObject), typeof(ICoroutineRunner))
                .WithId(ResourceBindCode)
                .FromSubContainerResolve()
                .ByInstaller<DestroyObjectInstaller>()
                .AsSingle();
            #endregion
            
            #region ResourceLoaderBind
            Type resourceLoader = typeof(IResourcesLoader<>).MakeGenericType(_resoucesType.ResourceKeyType);
            Type rsourcesLoaderInstaller = typeof(ResourceLoaderInstaller<>).MakeGenericType(_resoucesType.ResourceKeyType);
            Container.Bind(resourceLoader).
                WithId(ResourceBindCode)
                .FromSubContainerResolve()
                .ByInstaller(rsourcesLoaderInstaller)
                .AsSingle();
            #endregion
            
            Type resourcesSerciveType = typeof(ResourceManager<>).MakeGenericType(_resoucesType.ResourceKeyType);
            Type resourcesSerciveIface = typeof(IResourcesServices<>).MakeGenericType(_resoucesType.ResourceKeyType);
            Container.Bind(resourcesSerciveIface).WithId(ResourceBindCode).To(resourcesSerciveType).AsSingle();
            
            Container.Bind<IResourcesServices>()
                .FromMethod(context => (IResourcesServices)context.Container.ResolveId(resourcesSerciveIface, ResourceBindCode))
                .AsSingle(); //소비자 들에겐 IResourceServices를 제공해야 하기에 ID가 없는 IResourceServices를 바인드 문제는 FromResolve()가 ID값으로 찾는 걸 지원 안함. 그래서  FromMethod로 찾아야함.
            
        }

        #region UIManagerInstall

        void UIManagerInstall()
        {
            Container.Bind<IUIorganizer>().To<UIOrganizer>().AsSingle();
            
            switch (resourceLoadRequestType)
            {
                case ResourceLoadType.ResourcesType:

                    // 각 매니저의 구현 클래스를 먼저 싱글턴으로 바인딩 한다. 순환참조 방지
                    Container.Bind<UIPopupManagerWithResources>().AsSingle();
                    Container.Bind<UISceneManagerWithResources>().AsSingle();
                    Container.Bind<UISubItemWithResources>().AsSingle();

                    //이제 모든 인터페이스 바인딩이 위에서 생성된 싱글턴 인스턴스를 참조하도록.FromResolve()를 사용

                    // IRegisterCachingUI는 ID 없이 바인딩해 List<IRegisterCachingUI>로 주입
                    Container.Bind<IRegisterCachingUI>().To<UIPopupManagerWithResources>().FromResolve();
                    Container.Bind<IRegisterCachingUI>().To<UISceneManagerWithResources>().FromResolve();

                    // 특정 매니저 인터페이스들은 ID를 사용해 소비자들의 접근을 금지시킴.
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

                    break;
                case ResourceLoadType.AddressablesType:
                    break;
                default:
                    break;
            }

            Container.BindInterfacesTo<UIManagerRequestCaching>().AsSingle();
        }

        #endregion
    }


    public class InstantiatorInstaller<TKey> : Installer<InstantiatorInstaller<TKey>>
    {
        public override void InstallBindings()
        {
            Type instantiatorIface = typeof(IInstantiate<>).MakeGenericType(typeof(TKey));
            Type dictIface = typeof(ICachingObjectDict<>).MakeGenericType(typeof(TKey));
            Type registrarIface =typeof(IRegistrar<>).MakeGenericType(dictIface); //IRegistrar<ICachingObjectDict<Type>>
            Type defaultGameObjectFactory = typeof(IRegistrar<>).MakeGenericType(typeof(IDefaultGameObjectFactory));


            Type instantiatorImpl = typeof(Instantiator<>).MakeGenericType(typeof(TKey));

            Container.Bind(new[] {instantiatorIface, registrarIface, defaultGameObjectFactory})
                .To(instantiatorImpl)
                .AsSingle();
        }
    }
    public class DestroyObjectInstaller : Installer<DestroyObjectInstaller>
    {
        public override void InstallBindings()
        {

            Container.Bind<IDestroyObject>().To<ObjectReleaser>().AsSingle();
        }
    }
    public class ResourceLoaderInstaller<TKey> : Installer<ResourceLoaderInstaller<TKey>>
    {
        private ResourceLoadType _resourceLoadType;

        [Inject]
        public void Construct(
            [Inject(Id = ResourcesLoaderInstaller.ResourceBindCode)]
            ResourceLoadType resourceLoadType)
        {
            _resourceLoadType = resourceLoadType;
        }
        public override void InstallBindings()
        {
            Type resourceLoader = typeof(IResourcesLoader<>).MakeGenericType(typeof(TKey));
            switch (_resourceLoadType)
            {
                case ResourceLoadType.ResourcesType:
                    Container.Bind(resourceLoader)
                        .To<ResourcesLoader>().AsSingle();
                    break;
                case ResourceLoadType.AddressablesType:
                    //TODO: 어드레서블 추가되면 타입작성할 것
                    break;
                default:
                    Container.Bind(resourceLoader)
                        .To(typeof(NullLoader<>))
                        .AsSingle().IfNotBound();
                    break;
            }

        }
    }
}