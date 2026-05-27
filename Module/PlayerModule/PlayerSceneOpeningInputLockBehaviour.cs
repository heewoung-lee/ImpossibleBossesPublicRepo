using GameManagers.InputManagement;
using ScenesScripts.CommonInstaller.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace Module.PlayerModule
{
    [DisallowMultipleComponent]
    public class PlayerSceneOpeningInputLockBehaviour : MonoBehaviour, IPlayerSceneOpeningTarget
    {
        private IInputAsset _inputAsset;
        private InputActionMap[] _managedActionMaps;

        [Inject]
        public void Construct(IInputAsset inputAsset)
        {
            _inputAsset = inputAsset;
        }

        public void OnSceneOpeningStart()
        {
            SetActionMapEnabled(false);
        }

        public void OnSceneOpeningEnd()
        {
            SetActionMapEnabled(true);
        }

        private void SetActionMapEnabled(bool isEnabled)
        {
            CacheActionMaps();

            if (_managedActionMaps == null)
            {
                return;
            }

            for (int i = 0; i < _managedActionMaps.Length; i++)
            {
                InputActionMap actionMap = _managedActionMaps[i];
                if (actionMap == null)
                {
                    continue;
                }

                if (isEnabled)
                {
                    actionMap.Enable();
                    continue;
                }

                actionMap.Disable();
            }
        }

        private void CacheActionMaps()
        {
            if (_managedActionMaps != null)
            {
                return;
            }

            InputActionAsset actionAsset = _inputAsset.GetInputActionAsset();
            _managedActionMaps = new[]
            {
                actionAsset.FindActionMap(Define.ControllerType.Player.ToString(), false),
                actionAsset.FindActionMap(Define.ControllerType.Camera.ToString(), false),
                actionAsset.FindActionMap(Define.ControllerType.UI.ToString(), false)
            };
        }
    }
}
