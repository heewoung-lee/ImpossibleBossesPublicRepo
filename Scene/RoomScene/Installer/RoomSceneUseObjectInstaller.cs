using System.Collections.Generic;
using System.Linq;
using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using NetWork.NGO.UI;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Factories;
using Scene.CommonInstaller.Interfaces;
using Scene.CommonInstaller.Tools;
using Test.TestZenject;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace Scene.RoomScene.Installer
{
    public class RoomSceneUseObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<CharacterSelectorNgo.CharacterSelectorNgoFactory>()
                .AsSingle();

            Container.BindInterfacesTo<NgoUIRootCharacterSelect.NgoUIRootCharacterSelectFactory>()
                .AsSingle();

            Container.BindInterfacesTo<NgoRootUIInitializer.NgoRootUIInitializerFactory>()
                .AsSingle();
            
        }
        
    }
}