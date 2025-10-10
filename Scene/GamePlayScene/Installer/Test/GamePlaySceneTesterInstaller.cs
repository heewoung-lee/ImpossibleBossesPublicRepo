using UI.Scene.SceneUI;
using Zenject;

namespace Scene.GamePlayScene.Installer
{
    public class GamePlaySceneTesterInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UICreateItemAndGoldButton.UICreateItemAndGoldButtonFactory>().AsSingle();
        }
    }
}
