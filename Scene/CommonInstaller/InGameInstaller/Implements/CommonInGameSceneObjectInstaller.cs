using System.Linq;
using NetWork.Item;
using NetWork.NGO;
using NetWork.NGO.InitializeNGO;
using NetWork.NGO.InitializeNGO.EffectVFX;
using Scene.CommonInstaller.Factories;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Scene.CommonInstaller
{
    
    public class CommonInGameSceneObjectInstaller : Installer<CommonInGameSceneObjectInstaller>
    {
        protected const string CharacterLoadPath = "Prefabs/Player/SpawnCharacter";
        protected const string LootItemLoadPath = "Prefabs/NGO/LootingItem";

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NgoVFXInitialize.VFXRootNgoFactory>().AsSingle();
            
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
}
