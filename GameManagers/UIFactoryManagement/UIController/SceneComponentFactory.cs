using GameManagers.ResourcesExManagement;
using GameManagers.UIFactoryManagement.SceneUI;

using ScenesScripts;
using UnityEngine;
using Zenject;

namespace GameManagers.UIFactoryManagement.UIController
{
    public class SceneComponentFactory<T> : IFactory<T>, ISceneUI where T : Component
    {
        [Inject] private IResourcesServices _resourceServices;
        [Inject] private BaseScene _scene;
        public void SceneGameObjectCreate() => Create();
        public T Create()
        {
            T sceneUI = _resourceServices.GetOrAddComponent<T>(_scene.gameObject);
            return sceneUI;
        }
    }
}
