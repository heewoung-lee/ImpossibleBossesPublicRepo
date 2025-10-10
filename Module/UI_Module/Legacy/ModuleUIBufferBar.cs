using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class ModuleUIBufferBar : MonoBehaviour
    {
        [Inject]private IUIManagerServices _uiManagerServices; 
        UIBufferBar _uiBufferbar;

        void Start()
        {
            _uiBufferbar = _uiManagerServices.GetSceneUIFromResource<UIBufferBar>();
        }
    }
}
