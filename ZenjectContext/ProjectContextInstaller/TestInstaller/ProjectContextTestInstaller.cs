using GameManagers;
using GameManagers.Interface.RelayManagerInterface.Implementation;
using GameManagers.RelayManager;
using Scene.CommonInstaller;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller.TestInstaller
{
    public class ProjectContextTestInstaller : MonoInstaller,ITestInstaller
    {
        public override void InstallBindings()
        {
            // //릴레이서버 로컬모드 테스트
            Container.Rebind<IConnectionStrategy>()
                .To<LocalTestConnection>()
                .AsSingle();
              
            //로컬모드로 리바인드 할때
            //모든 멀티 로직들이 로컬 127,0,0,1 로 향하도록 수정
            
            
            
        }
    }
}
