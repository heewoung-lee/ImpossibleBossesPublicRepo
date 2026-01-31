using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using GameManagers.UIFactory.SceneUI;
using Scene;
using UnityEngine;
using Zenject;

namespace GameManagers.UIFactory.UIController
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
