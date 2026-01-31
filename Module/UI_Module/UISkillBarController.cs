using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.SkillManager;
using GameManagers.Interface.UIManager;
using GameManagers.UIFactory.UIController;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace Module.UI_Module
{
    public class UISkillBarController : MonoBehaviour
    {
        /// <summary>
        /// 스킬을 쓸 수 있는 스킬UI
        /// </summary>
        public class UISkillBarControllerFactory : SceneComponentFactory<UISkillBarController>{}
        
        private IUIManagerServices _uiManager;
        private SignalBus  _signalBus;

        [Inject]
        public void Construct(IUIManagerServices uiManager,SignalBus signalBus)
        {
            _uiManager = uiManager;
            _signalBus = signalBus;
        }
        
        
        void Start()
        {
            UISkillBar skillBarUI = _uiManager.GetSceneUIFromResource<UISkillBar>();
            _signalBus.Fire(new UISkillBarReadySignal());
        }

    }
}
