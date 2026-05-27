using System.Collections;
using Stats.BaseStats;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.WorldSpace
{
    public class UICastingBar : UIBase
    {
        private const float FadeOutSeconds = 0.2f;
        private readonly Vector3 _offsetCastingBar = new Vector3(0, 0.7f, 0);

        private Slider _castingSlider;
        private CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;

        enum CastingBarSlider
        {
            CastingBar
        }

        protected override void AwakeInit()
        {
            Bind<Slider>(typeof(CastingBarSlider));
            _castingSlider = Get<Slider>((int)CastingBarSlider.CastingBar);
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _castingSlider.value = 0;
        }

        protected override void StartInit()
        {
            BaseStats stats = GetComponentInParent<BaseStats>();

            Vector3 uiCastingBarPos;
            if (stats.transform.TryGetComponentInChildren(out HeadTr headTr))
            {
                uiCastingBarPos = headTr.transform.position;
            }
            else
            {
                if (stats.transform.TryGetComponentInChildren(out Collider col))
                {
                    uiCastingBarPos = new Vector3(stats.transform.position.x, col.bounds.max.y, stats.transform.position.z);
                }
                else
                {
                    uiCastingBarPos = stats.transform.position;
                }
            }

            transform.position = uiCastingBarPos + _offsetCastingBar;
        }

        private void LateUpdate()
        {
            if (_canvasGroup.alpha > 0)
                transform.rotation = Camera.main.transform.rotation;
        }

        public void Show(float normalizedProgress)
        {
            StopFadeRoutine();
            _canvasGroup.alpha = 1;
            _castingSlider.value = Mathf.Clamp01(normalizedProgress);
        }

        public void FadeOut()
        {
            StopFadeRoutine();
            _fadeRoutine = StartCoroutine(FadeOutRoutine());
        }

        public void Hide()
        {
            StopFadeRoutine();
            _canvasGroup.alpha = 0;
            _castingSlider.value = 0;
        }

        private IEnumerator FadeOutRoutine()
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < FadeOutSeconds)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / FadeOutSeconds);
                yield return null;
            }

            _canvasGroup.alpha = 0;
            _castingSlider.value = 0;
            _fadeRoutine = null;
        }

        private void StopFadeRoutine()
        {
            if (_fadeRoutine == null)
                return;

            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
    }
}
