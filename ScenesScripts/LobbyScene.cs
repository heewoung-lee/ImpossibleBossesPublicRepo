using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace ScenesScripts
{
    public class LobbyScene : BaseScene
    {
        public override Define.SceneName CurrentSceneName => Define.SceneName.LobbyScene;
        
        [Inject]private IUIManagerServices _uiManagerServices; 
        [Inject]private SceneDataSaveAndLoader _sceneDataSaveAndLoader;
        UILobbyScene _uiLobbyScene;

        protected override void AwakeInit()
        {
        }

        protected override void StartInit()
        {
            _sceneDataSaveAndLoader.Clear();
            base.StartInit();
            _uiLobbyScene = _uiManagerServices.GetSceneUIFromResource<UILobbyScene>();

        }
    }
}
