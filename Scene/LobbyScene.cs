using System.Threading.Tasks;
using GameManagers;
using GameManagers.Interface.UIManager;
using Scene.GamePlayScene;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace Scene
{
    public class LobbyScene : BaseScene
    {
        public override Define.Scene CurrentScene => Define.Scene.LobbyScene;
        
        [Inject]private IUIManagerServices _uiManagerServices; 
        UILobbyScene _uiLobbyScene;

        protected override void AwakeInit()
        {
        }

        protected override void StartInit()
        {
            base.StartInit();
            _uiLobbyScene = _uiManagerServices.GetSceneUIFromResource<UILobbyScene>();

        }
    }
}
