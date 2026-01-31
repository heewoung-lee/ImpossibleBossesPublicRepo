using Controller;
using Module.UI_Module;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Scene.GamePlayScene.Installer
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
            
            
            //SceneAddComponent 영역
            Container.BindInterfacesAndSelfTo<UIPlayerInventoryController.UIPlayerInventoryControllerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UISkillBarController.UISkillBarControllerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<UIDescriptionController.UIDescriptionControllerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<MoveMarkerController.MoveMarkerControllerFactory>().AsSingle();
        }
    }
}