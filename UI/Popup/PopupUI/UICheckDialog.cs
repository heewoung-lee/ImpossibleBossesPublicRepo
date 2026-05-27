using System;
using GameManagers.UIManagement;
using UnityEngine.UI;
using Zenject;

namespace UI.Popup.PopupUI
{
    public class UICheckDialog: UIAlertPopupBase
    {
        private IUIManagerServices _uiManagerServices;


        [Inject]
        public void Construct(IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }
        
        enum CancelButton
        {
            CancelButton
        }
        private Button _cancelButton;
        protected override void StartInit()
        {
            AddBind<Button>(typeof(CancelButton),out string[] indexString);
            int extensionButtonIndex = Array.FindIndex(indexString, strings => strings == Enum.GetName(typeof(CancelButton), CancelButton.CancelButton));
            _cancelButton = Get<Button>(extensionButtonIndex);
            
            _cancelButton.onClick.AddListener(() =>
            {
            _uiManagerServices.ClosePopupUI(this);
            });
        }
        
        
        
    }
}
