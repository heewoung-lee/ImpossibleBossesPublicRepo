using GameManagers;
using GameManagers.UIManagement;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UILobbyScene : UIScene
    {
        [Inject]private IUIManagerServices _uiManagerServices; 
        UIUserInfoPanel _uiUserPanel;
        UILobbyChat _uiLobbyChat;
        UIRoomInventory _uiRoomInventory;
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
        }
    }
}
