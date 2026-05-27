using Controller.BossState.BossRedDragon;
using NetWork.NGO.Scene_NGO;
using UnityEngine;
using Zenject;

namespace ScenesScripts.ThirdBossScene
{
    public class ThirdBossSceneUseObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoThirdBossSceneSpawn.NgoThirdBossSceneSpawnFactory>().AsSingle();
            
            Container.BindInterfacesTo<BossRedDragonController.BossBossRedDragonFactory>().AsSingle();

            Container.Bind<SpawnPosition>()
                .FromInstance(new SpawnPosition(new Vector3(15.4f, 7.5f, 1.2f), new Vector3(18f, 8.83854961f, -21f)))
                .AsCached();
            
            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();
            Container.Bind<TimeValue>().FromInstance(new TimeValue(300f, 60f, 7f)).AsCached();
        }
    }
}
