using Unity.Cinemachine;
using UnityEngine;

namespace ScenesScripts.CommonInstaller
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(BossFocusCameraTargetBinder))]
    public class BossFocusCameraTravelDriver : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float _travelProgress;

        private CinemachineCamera _cinemachineCamera;
        private BossFocusCameraTargetBinder _targetBinder;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Quaternion _startRotation;
        private Quaternion _targetRotation;
        private bool _isTravelPrepared;

        private void Awake()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            _targetBinder = GetComponent<BossFocusCameraTargetBinder>();
        }

        private void LateUpdate()
        {
            if (_isTravelPrepared == false)
            {
                return;
            }

            ApplyTravel();
        }

        public void PrepareTravel()
        {
            Vector3 targetLookDirection = _targetBinder.BossTarget.position - _targetBinder.CameraAnchor.position;
            GetCurrentLiveCameraPose(out _startPosition, out _startRotation);
            _targetPosition = _targetBinder.CameraAnchor.position;
            _targetRotation = Quaternion.LookRotation(targetLookDirection, Vector3.up);
            _travelProgress = 0f;
            _isTravelPrepared = true;
            ApplyTravel();
        }

        public void SnapToScenePose()
        {
            _isTravelPrepared = false;
            _cinemachineCamera.ForceCameraPosition(transform.position, transform.rotation);
        }

        private void GetCurrentLiveCameraPose(out Vector3 position, out Quaternion rotation)
        {
            Camera mainCamera = Camera.main;
            CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();

            if (brain != null && brain.ActiveVirtualCamera != null)
            {
                CameraState currentState = brain.ActiveVirtualCamera.State;
                position = currentState.GetFinalPosition();
                rotation = currentState.GetFinalOrientation();
                return;
            }

            Transform mainCameraTransform = mainCamera.transform;
            position = mainCameraTransform.position;
            rotation = mainCameraTransform.rotation;
        }

        private void ApplyTravel()
        {
            Vector3 nextPosition = Vector3.Lerp(_startPosition, _targetPosition, _travelProgress);
            Quaternion nextRotation = Quaternion.Slerp(_startRotation, _targetRotation, _travelProgress);
            _cinemachineCamera.ForceCameraPosition(nextPosition, nextRotation);
        }
    }
}
