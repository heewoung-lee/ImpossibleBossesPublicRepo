using Scene.CommonInstaller.InGameInstaller.Implements;
using Scene.GamePlayScene.Installer;
using UnityEngine;
using Zenject;

namespace Scene.CommonInstaller.InGameInstaller
{
    
    [DisallowMultipleComponent]
    public class CommonInGamePlayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            CachingObjectInstaller.Install(Container);
            //한번 로드된 오브젝트를 캐싱하는 매니저 
            DefaultObjectCreatorInstaller.Install(Container);
            //팩토리혹은 풀에 등록되지 않는 로드된 오브젝트의 인스턴스를 찍어내는 팩토리 
            
            
            CommonInGameSceneObjectInstaller.Install(Container);
            //인게임 플레이 씬에 필요한 주요요소 바인딩 
            CommonPlaySceneCameraInstaller.Install(Container);
            //인게임 플레이 씬에 필요한 카메라 바인딩
            CommonPlaySceneUIInstaller.Install(Container);
            //인게임 플레이 씬에 필요한 
            RuntimeSkillManagerInstaller.Install(Container);
            //스킬 바인딩
            StrategyFactoryInstaller.Install(Container);
            //물약 및 아이템 착용에 필요한 전략 바인딩
        }
        
    }
}
