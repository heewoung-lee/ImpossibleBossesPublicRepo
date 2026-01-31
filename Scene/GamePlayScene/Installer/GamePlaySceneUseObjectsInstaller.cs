using System.Linq;
using NetWork.NGO;
using NetWork.NGO.InitializeNGO;
using NetWork.NGO.InitializeNGO.EffectVFX;
using NetWork.NGO.Scene_NGO;
using NPC.Dummy;
using Scene.CommonInstaller.Factories;
using Scene.GamePlayScene.Spawner;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Scene.GamePlayScene.Installer
{
    public class GamePlaySceneUseObjectsInstaller : MonoInstaller
    {
        protected const string CharacterLoadPath = "Prefabs/Player/SpawnCharacter";
        protected const string LootItemLoadPath = "Prefabs/NGO/LootingItem";
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoGamePlaySceneSpawn.NgoGamePlaySceneSpawnFactory>().AsSingle();

            Container.BindInterfacesTo<GamePlaySceneNormalSpawn>().AsSingle();
            
            
            Container.BindInterfacesTo<Dummy.DummyFactory>().AsSingle();
            
            Container.BindInterfacesTo<PlayerDummy.PlayerDummyFactory>().AsSingle();

            Container.BindInterfacesTo<NgoBossRoomEntrance.NgoBossRoomEntranceFactory>().AsSingle();
            Container.Bind<BossRoomEntrancePosition>().FromInstance(new BossRoomEntrancePosition(new Vector3(15f,0.15f,32f))).AsCached();

            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();
            Container.Bind<TimeValue>().FromInstance(new TimeValue(300f, 60f, 7f)).AsCached();
            
        }
    }
}
