using UnityEngine;
using UnityEngine.Rendering;

namespace Controller
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraShaker : MonoBehaviour
    {
        private const float MaxPositionOffset = 0.35f;
        private const float ShakeFrequency = 45f;

        private Camera _camera;
        private Vector3 _appliedOffset;
        private float _shakeStartTime;
        private float _shakeEndTime;
        private float _shakeDuration;
        private float _shakeIntensity;
        private float _noiseSeedX;
        private float _noiseSeedY;
        private bool _isOffsetApplied;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += HandleEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= HandleEndCameraRendering;
            RemoveRenderOffset();
        }

        public void Shake(float intensity, float duration)
        {
            float clampedIntensity = Mathf.Clamp01(intensity);
            float clampedDuration = Mathf.Max(0f, duration);
            if (clampedIntensity <= 0f || clampedDuration <= 0f)
            {
                return;
            }

            _shakeIntensity = clampedIntensity;
            _shakeDuration = clampedDuration;
            _shakeStartTime = Time.unscaledTime;
            _shakeEndTime = _shakeStartTime + _shakeDuration;
            _noiseSeedX = Random.value * 100f;
            _noiseSeedY = Random.value * 100f;
        }

        private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera != _camera)
            {
                return;
            }

            ApplyRenderOffset();
        }

        private void HandleEndCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera != _camera)
            {
                return;
            }

            RemoveRenderOffset();
        }

        private void ApplyRenderOffset()
        {
            if (_isOffsetApplied)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now >= _shakeEndTime)
            {
                return;
            }

            float progress = Mathf.Clamp01((now - _shakeStartTime) / _shakeDuration);
            float amplitude = (1f - progress) * _shakeIntensity * MaxPositionOffset;
            float sampleTime = now * ShakeFrequency;
            float offsetX = Mathf.PerlinNoise(_noiseSeedX, sampleTime) * 2f - 1f;
            float offsetY = Mathf.PerlinNoise(_noiseSeedY, sampleTime) * 2f - 1f;

            _appliedOffset = (transform.right * offsetX + transform.up * offsetY) * amplitude;
            transform.position += _appliedOffset;
            _isOffsetApplied = true;
        }

        private void RemoveRenderOffset()
        {
            if (_isOffsetApplied == false)
            {
                return;
            }

            transform.position -= _appliedOffset;
            _appliedOffset = Vector3.zero;
            _isOffsetApplied = false;
        }
    }
}
