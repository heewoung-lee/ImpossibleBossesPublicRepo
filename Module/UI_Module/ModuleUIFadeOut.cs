using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI_Module
{
    public class ModuleUIFadeOut : MonoBehaviour
    {
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _disableGameObjectOnComplete = true;

        private Graphic[] _graphics;
        private Coroutine _fadeOutCoroutine;
        private bool _isPlayingFadeout = false;

        public bool IsPlayingFadeOut => _isPlayingFadeout;
        public bool PlayOnEnable => _playOnEnable;
        public bool DisableGameObjectOnComplete => _disableGameObjectOnComplete;

        public Action DoneFadeoutEvent;

        private void Awake()
        {
            CacheGraphics();
        }

        private void OnEnable()
        {
            if (_playOnEnable == true)
            {
                PlayFadeOut();
            }
        }

        private void OnDisable()
        {
            StopFadeOut(resetAlpha: true);
        }

        public void PlayFadeOut()
        {
            CacheGraphics();
            StopFadeOut(resetAlpha: true);

            if (gameObject.activeInHierarchy == false)
            {
                return;
            }

            _fadeOutCoroutine = StartCoroutine(FadeOutImage());
        }

        public void HideImmediate()
        {
            StopFadeOut(resetAlpha: true);

            if (gameObject.activeSelf == true)
            {
                gameObject.SetActive(false);
            }
        }

        private void CacheGraphics()
        {
            if (_graphics != null && _graphics.Length > 0)
            {
                return;
            }

            _graphics = GetComponentsInChildren<Graphic>(true);
        }

        private void StopFadeOut(bool resetAlpha)
        {
            if (_fadeOutCoroutine != null)
            {
                StopCoroutine(_fadeOutCoroutine);
                _fadeOutCoroutine = null;
            }

            _isPlayingFadeout = false;

            if (resetAlpha == true)
            {
                SetAlpha(1f);
            }
        }

        private void SetAlpha(float alpha)
        {
            CacheGraphics();

            foreach (Graphic graphic in _graphics)
            {
                Color color = graphic.color;
                color.a = alpha;
                graphic.color = color;
            }
        }

        private IEnumerator FadeOutImage()
        {
            _isPlayingFadeout = true;
            float duration = 1f;
            SetAlpha(1f);

            while (duration > 0)
            {
                duration -= Time.deltaTime / 2f;
                float alpha = Mathf.Clamp01(duration);

                foreach (Graphic graphic in _graphics)
                {
                    Color color = graphic.color;
                    color.a = alpha;
                    graphic.color = color;
                }

                yield return null;
            }

            _fadeOutCoroutine = null;
            _isPlayingFadeout = false;

            if (_disableGameObjectOnComplete == true)
            {
                gameObject.SetActive(false);
            }

            DoneFadeoutEvent?.Invoke();
        }
    }
}
