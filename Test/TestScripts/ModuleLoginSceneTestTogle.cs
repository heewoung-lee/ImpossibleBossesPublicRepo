using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using Test.TestUI;
using UnityEngine;
using Zenject;

namespace Test.TestScripts
{
    public class ModuleLoginSceneTestTogle : MonoBehaviour
    {
        [Inject] private IUIManagerServices _uiManager;

        void Start()
        {
            LogInTestToggle testTogle = _uiManager.GetSceneUIFromResource<LogInTestToggle>(path: "Prefabs/UI/TestUI/LogInTestToggle");
        }
    }
}
