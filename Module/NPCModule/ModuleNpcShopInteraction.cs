using GameManagers;
using GameManagers.Interface.UIManager;
using Module.CommonModule;
using Module.PlayerModule;
using UI.Popup.PopupUI;
using UnityEngine;
using Zenject;

namespace Module.NPCModule
{
    public class ModuleNpcShopInteraction : MonoBehaviour, IInteraction
    {
        [Inject] private IUIManagerServices  _uiManagerServices;
        
        private UIShop _uiShop;
        private CapsuleCollider _collider;

        public bool CanInteraction => true;

        public string InteractionName => "상인";

        public Color InteractionNameColor => Color.white;

        private void Awake()
        {
            _collider = GetComponent<CapsuleCollider>();
        }
        private void Start()
        {
            _uiShop = _uiManagerServices.GetPopupUIFromResource<UIShop>();
            _uiManagerServices.ClosePopupUI(_uiShop);
        }
        public void Interaction(ModulePlayerInteraction caller)
        {
            _uiManagerServices.SwitchPopUpUI(_uiShop);
        }
        public void OutInteraction()
        {
            _uiManagerServices.ClosePopupUI(_uiShop);
        }

    }
}