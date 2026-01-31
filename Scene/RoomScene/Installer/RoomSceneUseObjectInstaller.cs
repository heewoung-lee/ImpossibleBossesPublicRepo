using NetWork.NGO;
using NetWork.NGO.UI;
using Scene.CommonInstaller.Factories;
using Zenject;

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