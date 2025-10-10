using GameManagers;
using GameManagers.Interface.BufferManager;
using GameManagers.Interface.BufferManager.implementation;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class BufferManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BufferManager>().AsSingle();

            
            //IBufferTypeCreator가 하나만 필요하니 AsSingle선언
            Container.Bind<IBufferTypeCreator>().FromSubContainerResolve()
                .ByInstaller<BufferTypeCreatorInstaller>().AsSingle();
            
            
        }
    }


    public class BufferTypeCreatorInstaller : Installer<BufferTypeCreatorInstaller>
    {
        public override void InstallBindings()
        {
            //IBufferTypeCreator 제품을 어떻게 바인드 할 것인지.
            Container.Bind<IBufferTypeCreator>().To<BufferTypeCreate>().AsSingle();
        }
    }
}
