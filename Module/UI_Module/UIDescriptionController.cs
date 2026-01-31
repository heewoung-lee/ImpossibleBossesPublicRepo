using GameManagers;
using GameManagers.Interface.UIManager;
using GameManagers.UIFactory.UIController;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace Module.UI_Module
{
    public class UIDescriptionController : MonoBehaviour
    {
        public class UIDescriptionControllerFactory : SceneComponentFactory<UIDescriptionController>{}

        private IUIManagerServices  _uiManagerServices;
        [Inject]
        public void Construct(IUIManagerServices  uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }
        
        UIDescription _description;
        public UIDescription Description
        {
            get
            {
                if(_description == null)
                {
                    _description = _uiManagerServices.GetSceneUIFromResource<UIDescription>();
                }

                return _description;
            }
        }
        private void Start()
        {
            Description.GetComponent<Canvas>().sortingOrder = (int)Define.SpecialSortingOrder.Description;
        }
    }
}
