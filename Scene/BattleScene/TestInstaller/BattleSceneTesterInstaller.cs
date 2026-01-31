using NetWork.NGO.Scene_NGO;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.CommonInstaller.TestInstaller;
using UI.Scene.SceneUI;
using Zenject;

namespace Scene.BattleScene.TestInstaller
{
    public class BattleSceneTesterInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UICreateItemAndGoldButton.UICreateItemAndGoldButtonFactory>().AsSingle();
            
            Container.Rebind<TimeValue>().FromInstance(new TimeValue(300f, 10f, 7f)).AsCached();
            
            Container.Rebind<ISceneConnectOnline>().To<SceneConnectOnlineMultiDirect>().AsSingle();
        }
    }
}
