using NetWork.NGO;
using NetWork.NGO.UI;
using ScenesScripts.CommonInstaller.Factories;
using Zenject;

namespace ScenesScripts.RoomScene.Installer
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