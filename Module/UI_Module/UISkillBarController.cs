using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.SkillManager;
using GameManagers.Interface.UIFactoryManager.UIController;
using GameManagers.Interface.UIManager;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.UI_Module
{
    public class UISkillBarController : MonoBehaviour
    {
        public class UISkillBaControllerFactory : SceneComponentFactory<UISkillBarController>{}
        
        private IUIManagerServices _uiManager;
        private ISkillManager _skillManager;

        [Inject]
        public void Construct(IUIManagerServices uiManager, ISkillManager skillManager)
        {
            _uiManager = uiManager;
            _skillManager = skillManager;
        }
        
        
        void Start()
        {
            UISkillBar skillBarUI = _uiManager.GetSceneUIFromResource<UISkillBar>();
            _skillManager.Invoke_Done_UI_SKilBar_Init_Event();
        }

    }
}
