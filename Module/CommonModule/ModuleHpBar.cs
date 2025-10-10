using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.WorldSpace;
using UnityEngine;
using Zenject;

namespace Module.CommonModule
{
    public class ModuleHpBar : MonoBehaviour
    {
        [Inject] private IUIManagerServices _uiManagerServices;

        void Start()
        {
            UIHpBar playerInfoUI = _uiManagerServices.MakeUIWorldSpaceUI<UIHpBar>();
            playerInfoUI.transform.SetParent(transform);
        }
    }
}
