using System;
using System.Collections;
using Stats.MonsterStats;
using UnityEngine;
using Util;

namespace Enemy.Boss.Darkwizard.Minion.Bomber
{
    public class BomberFlashEffect : MonoBehaviour
    {
        private static readonly Color FinalEmissionColor = new Color32(255, 142, 142, 255);
        private const string EmissionKeyword = "_EMISSION_ON";
        private static readonly int EmissiveColorAltId = Shader.PropertyToID("_Emissive_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionEnabledId = Shader.PropertyToID("_EmissionEnabled");
        private static readonly int EmissionSelfGlowId = Shader.PropertyToID("_EmissionSelfGlow");

        private SkinnedMeshRenderer _renderer;
        private Material _runtimeMaterial;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _flashCoroutine;
        private Action _flashDoneEvent;
        private BomberStats _bomberStats;
        [SerializeField] private float _maxFlashGlow = 6f;

        public event Action FlashDoneEvent
        {
            add { UniqueEventRegister.AddSingleEvent(ref _flashDoneEvent, value); }
            remove { UniqueEventRegister.RemovedEvent(ref _flashDoneEvent, value); }
        }

        private void Awake()
        {
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _runtimeMaterial = _renderer.material;
            _propertyBlock = new MaterialPropertyBlock();
            _bomberStats = GetComponent<BomberStats>();

            if (_runtimeMaterial.HasProperty(EmissionEnabledId))
            {
                _runtimeMaterial.SetFloat(EmissionEnabledId, 1f);
                _runtimeMaterial.EnableKeyword(EmissionKeyword);
            }
        }

        private void OnEnable()
        {
            ResetFlash();
        }

        private void OnDisable()
        {
            ResetFlash();
        }

        public void PlayFlash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(PlayFlashCoroutine());
        }

        public void ResetFlash()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            _renderer.GetPropertyBlock(_propertyBlock);
            SetEmissionColor(Color.black, 0f);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private IEnumerator PlayFlashCoroutine()
        {
            if (_renderer == null)
            {
                yield break;
            }

            float elapsed = 0f;
            float flashDuration = _bomberStats != null ? _bomberStats.FlashDuration : 0.3f;
            flashDuration = Mathf.Max(0.01f, flashDuration);

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / flashDuration);

                ApplyFlash(normalizedTime);
                yield return null;
            }

            _renderer.GetPropertyBlock(_propertyBlock);
            SetEmissionColor(Color.black, 0f);
            _renderer.SetPropertyBlock(_propertyBlock);

            _flashCoroutine = null;
            _flashDoneEvent?.Invoke();
        }

        private void ApplyFlash(float normalizedTime)
        {
            _renderer.GetPropertyBlock(_propertyBlock);

            Color targetColor;
            if (normalizedTime < 0.5f)
            {
                float lerpT = normalizedTime / 0.5f;
                targetColor = Color.Lerp(Color.black, Color.red, lerpT);
            }
            else
            {
                float lerpT = (normalizedTime - 0.5f) / 0.5f;
                targetColor = Color.Lerp(Color.red, FinalEmissionColor, lerpT);
            }

            SetEmissionColor(targetColor, Mathf.Lerp(0f, _maxFlashGlow, normalizedTime));
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private void SetEmissionColor(Color color, float selfGlow)
        {
            if (_runtimeMaterial.HasProperty(EmissionColorId))
            {
                _propertyBlock.SetColor(EmissionColorId, color * Mathf.Max(1f, selfGlow));
            }

            if (_runtimeMaterial.HasProperty(EmissionSelfGlowId))
            {
                _propertyBlock.SetFloat(EmissionSelfGlowId, selfGlow);
            }

            if (_runtimeMaterial.HasProperty(EmissiveColorAltId))
            {
                _propertyBlock.SetColor(EmissiveColorAltId, color);
            }
        }
    }
}
