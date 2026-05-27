using GameManagers.UIManagement;
using UI.Scene.SceneUI;
using UnityEngine;
using Zenject;

namespace Module.EnemyModule
{
    public class ModuleBossHpUI : MonoBehaviour
    {
        private IUIManagerServices _uiManagerServices;
        [Inject]
        public void Construct(IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
        }

        public void ShowBossHpUI()
        {
            _uiManagerServices.GetOrCreateSceneUI<UIBossHp>();
        }
    }
}
