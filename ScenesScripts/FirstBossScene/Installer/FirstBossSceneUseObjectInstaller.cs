using Enemy.Boss;
using NetWork.NGO.Scene_NGO;
using ScenesScripts.FirstBossScene.Spawner;
using UnityEngine;
using Zenject;

namespace ScenesScripts.FirstBossScene.Installer
{
    public class FirstBossSceneUseObjectInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoFirstBossSceneSpawn.NgoFirstBossSceneSpawnFactory>().AsSingle();

            Container.BindInterfacesTo<NgoBossGolemSpawner.BossGolemFactory>().AsSingle();

            Container.Bind<SpawnPosition>()
                .FromInstance(new SpawnPosition(new Vector3(40f, 0f, 30f), Vector3.zero))
                .AsCached();
            
            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();
            Container.Bind<TimeValue>().FromInstance(new TimeValue(300f, 30f, 7f)).AsCached();

        }
    }
}
