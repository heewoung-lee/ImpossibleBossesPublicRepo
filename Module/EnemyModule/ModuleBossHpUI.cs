using GameManagers;
using GameManagers.Interface.UIManager;
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

        private void Start()
        {
            UIBossHp bossHpUI = _uiManagerServices.GetSceneUIFromResource<UIBossHp>();
        }

    }
}
