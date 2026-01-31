using Enemy.Boss;
using NetWork.NGO.Scene_NGO;
using Scene.BattleScene.Spawner;
using Zenject;

namespace Scene.BattleScene.Installer
{
    public class BattleSceneUseObjectInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoBattleSceneSpawn.NgoBattleSceneSpawnFactory>().AsSingle();

            Container.BindInterfacesTo<NgoBossGolemSpawner.BossGolemFactory>().AsSingle();
            
            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();
            Container.Bind<TimeValue>().FromInstance(new TimeValue(300f, 60f, 7f)).AsCached();

        }
    }
}