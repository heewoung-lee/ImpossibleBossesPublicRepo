using GameManagers;
using GameManagers.SoundManagement;
using GameManagers.UIManagement;
using UnityEngine.UI;
using UI;
using Util;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class ClosePopupUIButton : UIBase
    {
        private Button _windowCloseButton;
        
        [Inject] private IUIManagerServices _uiManagerServices;

        protected override void AwakeInit()
        {
            _windowCloseButton = GetComponent<Button>();
        }

        protected override void StartInit()
        {
            if (_windowCloseButton == null)
            {
                UtilDebug.LogError($"[{nameof(ClosePopupUIButton)}] {nameof(Button)} is missing on {gameObject.name}.");
                return;
            }

            _windowCloseButton.onClick.AddListener(CloseParentPopup);
        }

        private void CloseParentPopup()
        {
            UIPopup parentPopup = transform.FindParantComponent<UIPopup>();
            if (parentPopup == null)
            {
                UtilDebug.LogError($"[{nameof(ClosePopupUIButton)}] Failed to find parent {nameof(UIPopup)} on {gameObject.name}.");
                return;
            }

            _soundManagerServices.PlayUiSfx(parentPopup.gameObject, UICommonSoundCueId.Close);
            _uiManagerServices.ClosePopupUI(parentPopup);
        }
    }
}
