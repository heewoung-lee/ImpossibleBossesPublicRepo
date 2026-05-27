using GameManagers.SceneManagement;
using GameManagers.UIManagement;

using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace ScenesScripts.LoginScene
{
    public class LoginScene : BaseScene
    {
        [Inject] private IUIManagerServices _uiManager;
        [Inject] private SceneManagerEx _sceneManagerEx;
        
        public override Define.SceneName CurrentSceneName => Define.SceneName.LoginScene;

        protected override void StartInit()
        {
            base.StartInit();
             _uiManager.GetSceneUIFromResource<UILoginTitle>();
            _sceneManagerEx.SetNormalBootMode(true);
            //로그인 상태부터 돌리는 씬은 노멀 루트이므로 테스트모드가 아니다.
        }

        protected override void AwakeInit()
        {
        }

    }
}
