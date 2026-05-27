using Controller;
using Module.UI_Module;
using UI.Scene.SceneUI;
using Zenject;

namespace ScenesScripts.CommonInstaller.InGameInstaller.Implements
{
    public class CommonPlaySceneUIInstaller : Installer<CommonPlaySceneUIInstaller>
    {
        public override void InstallBindings()
        {
            //SceneUI 영역
            Container.BindInterfacesAndSelfTo<UIPlayerInfo.UIPlayerInfoFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIBufferBar.UIBufferBarFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIConsumableBar.UIConsumableBarFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIItemDragImage.UIItemDragImageFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIInGameSettingMenuButton.UIInGameSettingMenuFactory>().AsSingle();//2.26일 인게임에서의 환경설정을 위해 추가
            
            
            
            
            
            //SceneAddComponent 영역
            Container.BindInterfacesAndSelfTo<UIPlayerInventoryController.UIPlayerInventoryControllerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UISkillBarController.UISkillBarControllerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIDescriptionController.UIDescriptionControllerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<MoveMarkerController.MoveMarkerControllerFactory>().AsSingle();
        }
    }
}