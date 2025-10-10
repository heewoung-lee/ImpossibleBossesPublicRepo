using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.SceneUIManager;
using Scene;
using Scene.GamePlayScene;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.UIFactoryManager.UIController
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
