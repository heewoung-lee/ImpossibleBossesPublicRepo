using System.Linq;
using Enemy.Boss;
using GameManagers.Interface.ResourcesManager;
using NetWork.Boss_NGO;
using NetWork.LootItem;
using NetWork.NGO;
using NetWork.NGO.InitializeNGO;
using NetWork.NGO.InitializeNGO.EffectVFX;
using NetWork.NGO.Scene_NGO;
using NetWork.NGO.UI;
using Scene.BattleScene.Spawner;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Factories;
using Scene.CommonInstaller.Interfaces;
using Skill.AllofSkills.BossMonster.StoneGolem;
using Skill.AllofSkills.Fighter;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Scene.BattleScene.Installer
{
    public class BattleSceneUseObjectInstaller : MonoInstaller
    {
        protected const string CharacterLoadPath = "Prefabs/Player/SpawnCharacter";
        protected const string LootItemLoadPath = "Prefabs/NGO/LootingItem";

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoBattleSceneSpawn.NgoBattleSceneSpawnFactory>().AsSingle();

            Container.BindInterfacesTo<NgoVFXInitialize.VFXRootNgoFactory>().AsSingle();

            Container.BindInterfacesTo<NgoStageTimerController.NgoStageTimerControllerFactory>().AsSingle();

            Container.BindInterfacesTo<NgoPoolRootInitialize.NgoPoolRootInitializeFactory>().AsSingle();

            Container.BindInterfacesTo<NetworkObjectPool.NetworkObjectPoolFactory>().AsSingle();

            Container.BindInterfacesTo<NgoRootInitializer.NgoRootFactory>().AsSingle();

            Container.BindInterfacesTo<ItemRootInitialize.ItemRootFactory>().AsSingle();

            Container.BindInterfacesAndSelfTo<NgoLevelUpInitialize.NgoLevelUpFactory>().AsSingle();

            Container.BindInterfacesTo<NgoBossGolemSpawner.BossGolemFactory>().AsSingle();

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
}