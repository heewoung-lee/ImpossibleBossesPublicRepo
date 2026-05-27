using Controller.BossState.BossDarkWizard;
using Enemy.Boss;
using NetWork.NGO.Scene_NGO;
using ScenesScripts.FirstBossScene.Spawner;
using UnityEngine;
using Zenject;

namespace ScenesScripts.SecondBossScene
{
    public class SecondBossSceneUseObjectInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoSecondBossSceneSpawn.NgoSecondBossSceneSpawnFactory>().AsSingle();

            Container.BindInterfacesTo<BossDarkWizardController.BossBossDarkWizardFactory>().AsSingle();

            Container.Bind<SpawnPosition>()
                .FromInstance(new SpawnPosition(new Vector3(0f, 0f, 27f), new Vector3(0f, 0f, -20f)))
                .AsCached();
            
            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();
            Container.Bind<TimeValue>().FromInstance(new TimeValue(300f, 60f, 7f)).AsCached();

        }
    }
}
