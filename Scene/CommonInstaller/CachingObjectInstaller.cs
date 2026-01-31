using System;
using GameManagers;
using GameManagers.ResourcesEx;
using GameManagers.UI;
using ProjectContextInstaller;
using UnityEngine;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace Scene.CommonInstaller
{
    public class CachingObjectInstaller : Installer<CachingObjectInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CachingObjectDictManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UICachingService>().AsSingle().NonLazy();
        }
    }
}