using GameManagers.UIManagement;
using UI.WorldSpace;
using UnityEngine;
using Zenject;

namespace Module.CommonModule
{
    public class ModuleCastingBar : MonoBehaviour
    {
        [Inject] private IUIManagerServices _uiManagerServices;

        private UICastingBar _castingBar;

        public UICastingBar CastingBar => _castingBar;

        private void Start()
        {
            _castingBar = _uiManagerServices.MakeUIWorldSpaceUI<UICastingBar>();
            _castingBar.transform.SetParent(transform);
        }
    }
}
