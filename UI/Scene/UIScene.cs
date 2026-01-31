using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.Popup.PopupUI;
using UnityEngine;
using Zenject;

namespace UI.Scene
{
    public class UIScene : UIBase
    {
        private IUIorganizer _uiManager;

        [Inject] 
        public void Construct(IUIorganizer uiManager)
        {
            _uiManager = uiManager;
        }
        protected override void AwakeInit() { } //Don't use injected Object

        protected override void InitAfterInject()
        {
            base.InitAfterInject();
            _uiManager.SetCanvas(gameObject.GetComponent<Canvas>(), true);
        }
        
        protected override void StartInit()
        {
        }
    }
}
