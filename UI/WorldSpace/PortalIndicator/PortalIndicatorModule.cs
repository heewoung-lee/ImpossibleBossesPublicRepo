using GameManagers;
using GameManagers.Interface.ResourcesManager;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;

namespace UI.WorldSpace.PortalIndicator
{
    public class PortalIndicatorModule : MonoBehaviour
    {
        private IUIManagerServices _uiManagerServices;
        private IFactoryCreator _creatorFactory;
        private UIPortalIndicator.UIPortalIndicatorFactory _uiPortalIndicatorFactory;

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices,
            IFactoryCreator creatorFactory,
            UIPortalIndicator.UIPortalIndicatorFactory uiPortalIndicatorFactory)
        {
            _uiManagerServices = uiManagerServices;
            _creatorFactory = creatorFactory;
            _uiPortalIndicatorFactory = uiPortalIndicatorFactory;
        }
        public void Start()
        {
            UIPortalIndicator uiPortalIndicator = _uiManagerServices.MakeUIWorldSpaceUI<UIPortalIndicator>();
            uiPortalIndicator.transform.SetParent(transform);
        }
    }
}

