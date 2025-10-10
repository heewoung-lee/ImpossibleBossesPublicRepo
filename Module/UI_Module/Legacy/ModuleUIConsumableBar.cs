using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class ModuleUIConsumableBar : MonoBehaviour
    {
        [Inject]private IUIManagerServices _uiManagerServices; 
        void Start()
        {
            UIConsumableBar uiConsumableBar = _uiManagerServices.GetSceneUIFromResource<UIConsumableBar>();
        }

    }
}
