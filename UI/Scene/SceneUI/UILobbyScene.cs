using GameManagers;
using GameManagers.Interface.UIManager;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILobbyScene : UIScene
    {
        [Inject]private IUIManagerServices _uiManagerServices; 
        UIUserInfoPanel _uiUserPanel;
        UILobbyChat _uiLobbyChat;
        UIRoomInventory _uiRoomInventory;
        UILoadingPanel _uiLoadingPanel;
        protected override void AwakeInit()
        {
            base.AwakeInit();
            
        }

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _uiUserPanel = _uiManagerServices.GetSceneUIFromResource<UIUserInfoPanel>();
            _uiLobbyChat = _uiManagerServices.GetSceneUIFromResource<UILobbyChat>();
            _uiRoomInventory = _uiManagerServices.GetSceneUIFromResource<UIRoomInventory>();
            _uiLoadingPanel = _uiManagerServices.GetSceneUIFromResource<UILoadingPanel>();
        }
    }
}
