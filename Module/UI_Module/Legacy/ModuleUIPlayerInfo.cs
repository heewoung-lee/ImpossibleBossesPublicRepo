using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class ModuleUIPlayerInfo : MonoBehaviour
    {
        [Inject] private IUIManagerServices _uiManagerServices;
        void Start()
        {
            UIPlayerInfo playerInfoUI = _uiManagerServices.GetSceneUIFromResource<UIPlayerInfo>();
        }
    }
}
