using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;

using ScenesScripts.CommonInstaller.Interfaces;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace ScenesScripts.GamePlayScene.Spawner
{
    public class MockPlaySceneLocalSpawnBehaviour: ISceneSpawnBehaviour
    {
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IResourcesServices _resourceService;
        private readonly ISceneSelectCharacter _sceneSelectCharacter;

        [Inject]
        public MockPlaySceneLocalSpawnBehaviour(
            IUIManagerServices uiManagerServices,
            IResourcesServices resourceService,
            BaseScene baseScene)
        {
            _uiManagerServices = uiManagerServices;
            _resourceService = resourceService;
            _sceneSelectCharacter = baseScene.GetComponent<ISceneSelectCharacter>();
        }
        public void Init()
        {
           //플레이어 스폰
           //TODO: 로컬에 어떤 플레이어 스폰할껀지 추가해야함.
           
           Define.PlayerClass playerClass = _sceneSelectCharacter.GetPlayerableCharacter();
           _resourceService.InstantiateByKey($"Prefabs/Player/SpawnCharacter/{playerClass}Base");
           
           _uiManagerServices.GetOrCreateSceneUI<UILoading>().gameObject.SetActive(false);
        }
        public void SpawnObj()
        {
            GameObject go = new GameObject(){name = "spawner"};
            _resourceService.GetOrAddComponent<LocalGamePlaySceneSpawner>(go);
        }


    }

}
