using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace Scene.LoginScene
{
    public class LoginScene : BaseScene
    {
        [Inject] private IUIManagerServices _uiManager;
        public override Define.Scene CurrentScene => Define.Scene.LoginScene;

        protected override void StartInit()
        {
            base.StartInit();
             _uiManager.GetSceneUIFromResource<UILoginTitle>();
            //로그인 상태부터 돌리는 씬은 노멀 루트이므로 테스트모드가 아니다.
        }

        protected override void AwakeInit()
        {
        }
        public override void Clear()
        {

        }
    }
}
