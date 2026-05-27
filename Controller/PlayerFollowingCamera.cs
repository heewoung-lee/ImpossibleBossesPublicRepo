using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.InputManagement;
using Stats;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Util;
using Zenject;

namespace Controller
{
    public class PlayerFollowingCamera : MonoBehaviour
    {
        private const float FrontViewVerticalAxisValue = 0f;
        private const float FrontViewBlendDuration = 1f;
        private const string LookOrbitXAxisName = "Look Orbit X";
        private const string LookOrbitYAxisName = "Look Orbit Y";

        // 2026-05-18 변경:
        // 휠/중클릭 축 입력은 CinemachineInputAxisController가 처리하고,
        // 이 스크립트는 UI 위 휠 차단, 중클릭 중 Look Orbit X 활성화와 정면 전환 중 입력 차단만 담당한다.
        // RotationComposer는 코드에서 토글하지 않고 프리팹 설정으로 유지한다.
        private Transform _playerTr;
        private CinemachineCamera _camera;
        private CinemachineOrbitalFollow _cinemachineOrbitalFollow;
        private CinemachineInputAxisController _inputAxisController;
        private CinemachineInputAxisController.Controller _lookOrbitXController;
        private CinemachineInputAxisController.Controller _lookOrbitYController;
        private InputAction _mouseMiddleButton;
        [Inject] private IInputAsset _inputManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;

        private bool _mouseMiddleButtonPressed = false;
        private bool _isCameraBlending = false;
        private int _cameraBlendVersion = 0;
        private CancellationTokenSource _cameraBlendCts;
        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _cinemachineOrbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            _inputAxisController = GetComponent<CinemachineInputAxisController>();
            _lookOrbitXController = _inputAxisController.GetController(LookOrbitXAxisName);
            _lookOrbitYController = _inputAxisController.GetController(LookOrbitYAxisName);
            SetLookOrbitXEnabled(false);

            if(_gameManagerEx.GetPlayer() == null)
            {
                _gameManagerEx.OnPlayerSpawnEvent += InitializeFollowingCamera;
            }
            else
            {
                InitializeFollowingCamera(_gameManagerEx.GetPlayer().GetComponent<PlayerStats>());
            }
        }

        private void Update()
        {
            if (_isCameraBlending == false)
            {
                _lookOrbitYController.Enabled =
                    EventSystem.current == null || EventSystem.current.IsPointerOverGameObject() == false;
            }
        }

        public void InitializeFollowingCamera(PlayerStats playerstats)
        {
            _playerTr = playerstats.transform;
            _camera.Target.TrackingTarget = _playerTr;

            _mouseMiddleButton = _inputManager.GetInputAction(Define.ControllerType.Camera, "MouseScrollButton");
            _mouseMiddleButton.Enable();
            _mouseMiddleButton.started += OnMiddleMouseButtonStarted;
            _mouseMiddleButton.canceled += OnMiddleMouseButtonCanceled;
        }

        private void OnDestroy()
        {
            if (_mouseMiddleButton != null)
            {
                _mouseMiddleButton.started -= OnMiddleMouseButtonStarted;
                _mouseMiddleButton.canceled -= OnMiddleMouseButtonCanceled;
            }

            _cameraBlendCts?.Cancel();
            _cameraBlendCts?.Dispose();
        }

        private void OnMiddleMouseButtonStarted(InputAction.CallbackContext context)
        {
            _mouseMiddleButtonPressed = true;
            SetLookOrbitXEnabled(_isCameraBlending == false);
        }

        private void OnMiddleMouseButtonCanceled(InputAction.CallbackContext context)
        {
            _mouseMiddleButtonPressed = false;
            SetLookOrbitXEnabled(false);
        }

        public void MoveCameraToPlayerFront()
        {
            if (_cinemachineOrbitalFollow == null || _playerTr == null)
            {
                return;
            }

            float targetHorizontalAxisValue = GetPlayerFrontHorizontalAxisValue();

            _cameraBlendVersion++;
            _cameraBlendCts?.Cancel();
            _cameraBlendCts?.Dispose();
            _cameraBlendCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            BlendCameraAxisAsync(
                targetHorizontalAxisValue,
                _cameraBlendVersion,
                _cameraBlendCts.Token).Forget();
        }

        private float GetPlayerFrontHorizontalAxisValue()
        {
            float playerYaw = _playerTr.eulerAngles.y;
            float normalizedFrontAxisValue = Mathf.DeltaAngle(180f, playerYaw);
            float currentHorizontalAxisValue = _cinemachineOrbitalFollow.HorizontalAxis.Value;
            float nearestWrappedDelta = Mathf.DeltaAngle(currentHorizontalAxisValue, normalizedFrontAxisValue);

            return currentHorizontalAxisValue + nearestWrappedDelta;
        }

        private async UniTaskVoid BlendCameraAxisAsync(
            float targetHorizontalAxisValue,
            int requestVersion,
            CancellationToken cancellationToken)
        {
            _isCameraBlending = true;
            SetCameraInputEnabled(false);

            float startHorizontalAxisValue = _cinemachineOrbitalFollow.HorizontalAxis.Value;
            float startVerticalAxisValue = _cinemachineOrbitalFollow.VerticalAxis.Value;
            float clampedVerticalAxisValue = Mathf.Clamp(
                FrontViewVerticalAxisValue,
                _cinemachineOrbitalFollow.VerticalAxis.Range.x,
                _cinemachineOrbitalFollow.VerticalAxis.Range.y);

            float elapsedTime = 0f;
            try
            {
                while (elapsedTime < FrontViewBlendDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsedTime / FrontViewBlendDuration);

                    _cinemachineOrbitalFollow.HorizontalAxis.Value =
                        Mathf.Lerp(startHorizontalAxisValue, targetHorizontalAxisValue, t);
                    _cinemachineOrbitalFollow.VerticalAxis.Value =
                        Mathf.Lerp(startVerticalAxisValue, clampedVerticalAxisValue, t);

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                if (requestVersion == _cameraBlendVersion)
                {
                    if (cancellationToken.IsCancellationRequested == false)
                    {
                        SetCameraAxisValue(targetHorizontalAxisValue, clampedVerticalAxisValue);
                    }

                    _isCameraBlending = false;
                    RestoreCameraInput();
                }
            }
        }

        private void SetCameraAxisValue(float horizontalAxisValue, float verticalAxisValue)
        {
            _cinemachineOrbitalFollow.HorizontalAxis.Value = horizontalAxisValue;
            _cinemachineOrbitalFollow.VerticalAxis.Value = verticalAxisValue;
        }

        private void RestoreCameraInput()
        {
            SetCameraInputEnabled(true);
            SetLookOrbitXEnabled(_mouseMiddleButtonPressed);
        }

        private void SetCameraInputEnabled(bool enabled)
        {
            _inputAxisController.enabled = enabled;
        }

        private void SetLookOrbitXEnabled(bool enabled)
        {
            _lookOrbitXController.Enabled = enabled;
        }
    }
}
