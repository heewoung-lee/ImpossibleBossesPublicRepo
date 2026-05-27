namespace UI.Popup.PopupUI
{
    public class UIAlertDialog : UIAlertPopupBase
    {
        protected override void StartInit()
        {
        
        }

        //모든 경고창에서는 팝업창을 ESC키로 없애만 안되고 플레이어가 직접 경고창을 확인하고,
        //확인버튼을 누르게 해야함
        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _closePopupUI.Disable();
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            _closePopupUI.Enable();
        }
    }
}
