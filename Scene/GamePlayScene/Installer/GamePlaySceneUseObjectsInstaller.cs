using System;
using System.ComponentModel;
using System.Linq;
using GameManagers.Interface.ResourcesManager;
using NetWork.LootItem;
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

public class GamePlaySceneUseObjectsInstaller : MonoInstaller
{
     protected const string CharacterLoadPath = "Prefabs/Player/SpawnCharacter";
        protected const string LootItemLoadPath = "Prefabs/NGO/LootingItem";
        public override void InstallBindings()
        {
           Container.BindInterfacesTo<NgoGamePlaySceneSpawn.NgoGamePlaySceneSpawnFactory>().AsSingle();

            Container.BindInterfacesTo<NgoVFXInitialize.VFXRootNgoFactory>().AsSingle();

            Container.BindInterfacesTo<Dummy.DummyFactory>().AsSingle();

            Container.BindInterfacesTo<NgoBossRoomEntrance.NgoBossRoomEntranceFactory>().AsSingle();

            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();

            Container.BindInterfacesTo<NgoPoolRootInitialize.NgoPoolRootInitializeFactory>().AsSingle();

            Container.BindInterfacesTo<NetworkObjectPool.NetworkObjectPoolFactory>().AsSingle();

            Container.BindInterfacesTo<NgoRootInitializer.NgoRootFactory>().AsSingle();
            
            Container.BindInterfacesTo<ItemRootInitialize.ItemRootFactory>().AsSingle();

            Container.BindInterfacesAndSelfTo<NgoLevelUpInitialize.NgoLevelUpFactory>().AsSingle();
            
            BindAllPlayableCharacterFactories();

            BindDropItemFactories();
            
            
            void BindAllPlayableCharacterFactories()
            {
                //Player Register
                string[] allCharacter = Resources.LoadAll<GameObject>(CharacterLoadPath)
                    .Select(characterPrefab => CharacterLoadPath + "/" + characterPrefab.name).ToArray();

                
                Assert.IsNotNull(allCharacter,"allCharacter is null");
                foreach (string characterPrefabPath in allCharacter)
                {
                    Container.BindInterfacesTo<PlayerInitializeNgo.CharacterSpawnFactory>().AsCached().WithArguments(characterPrefabPath);
                }
            }


            void BindDropItemFactories()
            {
                string[] lootItems = Resources.LoadAll<LootItem>(LootItemLoadPath)
                    .Select(lootItemprefab => LootItemLoadPath + "/" + lootItemprefab.name).ToArray();

                Assert.IsNotNull(lootItems,"lootItems is null");
                foreach (string lootitem in lootItems)
                {
                    Container.BindInterfacesTo<LootItem.LootItemFactory>().AsCached().WithArguments(lootitem);
                }
            }
        }
}
