using GameManagers;
using GameManagers.Interface.UIManager;
using Scene.CommonInstaller;
using UI.Scene.SceneUI;
using Zenject;

namespace Scene.RoomScene
{
    public class RoomSceneStarter : ISceneStarter
    {
        private readonly IUIManagerServices _uiManagerServices;
        [Inject]
        public  RoomSceneStarter( IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }
        public void SceneStart()
        {
            UIRoomCharacterSelect uICharacterSelect = _uiManagerServices.GetSceneUIFromResource<UIRoomCharacterSelect>();
            UIRoomChat uiChatting = _uiManagerServices.GetSceneUIFromResource<UIRoomChat>();
            
            //TODO: 얘네 둘이 SceneContainer에 의해 생성되어함.
            
        }
    }
}
