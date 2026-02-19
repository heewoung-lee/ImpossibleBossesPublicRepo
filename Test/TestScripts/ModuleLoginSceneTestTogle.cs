using GameManagers;
using Test.TestUI;
using UnityEngine;
using Zenject;


#if UNITY_EDITOR
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

#endif
