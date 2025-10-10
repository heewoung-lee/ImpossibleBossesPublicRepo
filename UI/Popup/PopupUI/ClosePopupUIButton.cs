using GameManagers;
using GameManagers.Interface.UIManager;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class ClosePopupUIButton : MonoBehaviour
    {
        private Button _windowCloseButton;
        private UIPopup _parentPopup;
        
        [Inject] private IUIManagerServices _uiManagerServices;

        void Start()
        {
            _parentPopup = transform.FindParantComponent<UIPopup>();

            _windowCloseButton = _windowCloseButton = Utill.FindChild(gameObject, "Button_Close", true).GetComponent<Button>();
            _windowCloseButton.onClick.AddListener(() =>
            {
                _uiManagerServices.ClosePopupUI(_parentPopup);
            });
        }

    }
}
