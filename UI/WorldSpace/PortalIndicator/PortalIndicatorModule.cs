using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;
using Zenject;

namespace UI.WorldSpace.PortalIndicator
{
    public class PortalIndicatorModule : MonoBehaviour
    {
        private IUIManagerServices _uiManagerServices;

        [Inject]
        public void Construct(
            IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }
        public void Start()
        {
            UIPortalIndicator uiPortalIndicator = _uiManagerServices.MakeUIWorldSpaceUI<UIPortalIndicator>();
            if (uiPortalIndicator is IInitializable initializable)
            {
                initializable.Initialize();
            }
            uiPortalIndicator.transform.SetParent(transform);
        }
    }
}

