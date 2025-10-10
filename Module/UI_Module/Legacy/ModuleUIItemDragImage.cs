using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class ModuleUIItemDragImage : MonoBehaviour
    {
        private UIItemDragImage _uiItemDragImage;
        
        [Inject]private IUIManagerServices _uiManagerServices; 
        void Start()
        {
            UIItemDragImage uIItemDragImage = _uiManagerServices.GetSceneUIFromResource<UIItemDragImage>();
        }
    }
}
