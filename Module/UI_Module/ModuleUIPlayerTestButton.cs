using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class ModuleUIPlayerTestButton : MonoBehaviour
    {
         private IUIManagerServices _uiManagerServices;

         [Inject]
         public void Construct(IUIManagerServices uiManagerServices)
         {
             _uiManagerServices = uiManagerServices;
         }
        
        void Start()
        {
            UICreateItemAndGoldButton buttonUI = _uiManagerServices.GetSceneUIFromResource<UICreateItemAndGoldButton>();
        }

    }
}
