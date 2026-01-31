using UI.Scene;
using Zenject;

namespace GameManagers.UIFactory.SceneUI
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
