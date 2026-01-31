using Buffer;
using GameManagers;
using GameManagers.Interface.BufferManager;
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
        }
    }

}
