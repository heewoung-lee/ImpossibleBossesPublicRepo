using System;
using CustomEditor;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using Module.UI_Module;
using Scene.BattleScene;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Scene.GamePlayScene.Spawner;
using Scene.GamePlayScene.Spwaner;
using Scene.RoomScene;
using UnityEngine;
using Zenject;

namespace Scene.GamePlayScene.Installer
{
    public class GamePlaySceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayScene>().FromComponentInHierarchy().AsSingle();
               
            Container.Bind<ISceneStarter>().To<PlaySceneStarter>().AsSingle();
            Container.Bind<ISceneMover>().To<BattleSceneMover>().AsSingle();
            
            
            Container.Bind<BaseScene>()
                .FromResolveGetter<PlayScene>(t => t.GetComponent<BaseScene>())
                .AsSingle();
            
            Container.Bind<ISceneSpawnBehaviour>().To<UnitNetGamePlayScene>().AsSingle();
        }
    }
}
