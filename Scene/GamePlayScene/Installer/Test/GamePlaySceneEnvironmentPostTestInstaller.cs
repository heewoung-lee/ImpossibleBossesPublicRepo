using NetWork.NGO.Scene_NGO;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene.Spawner;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;


namespace Scene.GamePlayScene.Installer.Test
{
    public class GamePlaySceneEnvironmentPostTestInstaller : MonoInstaller,ITestPostInstaller
    {

        [SerializeField] private bool _isTest = false;
        
        public override void InstallBindings()
        {
#if UNITY_EDITOR
            if (_isTest == true)
            {
                    
                //버튼 테스트
                Container.BindInterfacesAndSelfTo<UICreateItemAndGoldButton.UICreateItemAndGoldButtonFactory>().AsSingle();
           
                Container.Rebind<BossRoomEntrancePosition>().FromInstance(new BossRoomEntrancePosition(new Vector3(-4,0,0)));
                //포탈 위치 테스트
                Container.Rebind<TimeValue>().FromInstance(new TimeValue(9000f,5f,6f));

                //로컬 멀티접속 테스트
                Container.Rebind<ISceneConnectOnline>().To<SceneConnectOnlineMultiDirect>().AsSingle();
            

                Container.Rebind<ISceneSpawn>().To<SpawnDamageTestDummyTester>().AsSingle();
                //Container.Rebind<ISceneSpawn>().To<SpawnPlayerDummyTester>().AsSingle();
            }
#endif
        }
    }
}


