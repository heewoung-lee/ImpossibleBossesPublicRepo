using GameManagers.Interface.SceneUIManager;
using UI.Scene;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.UIFactoryManager.SceneUI
{
    public class SceneUIFactory<T> :IFactory<T>, ISceneUI where T : UIScene
    {
        [Inject] private IUIManagerServices _uiManagerServices;
        public void SceneGameObjectCreate() => Create();
        public T Create()
        {
            var sceneUI = _uiManagerServices.GetSceneUIFromResource<T>();
            return sceneUI;
        }
    }
}
