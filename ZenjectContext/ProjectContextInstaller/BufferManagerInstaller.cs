using Buffer;
using GameManagers;
using GameManagers.BufferManagement;
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
