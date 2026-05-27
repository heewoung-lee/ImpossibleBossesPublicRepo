using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.WorldSpace
{
    public class UIDamageText : UIBase
    {
        private const float RiseSpeedPerSecond = 80f;
        private const float LifetimeSeconds = 1.67f;
        private const int SortingOrder = 10;
        private const float StartYOffset = -35f;
        private static readonly Vector2 RandomOffsetRange = new Vector2(45f, 20f);

        TMP_Text _damageText;
        RectTransform _damageTextRectTransform;
        Transform _targetTransform;
        Vector3 _targetLocalAnchorPosition;
        Vector2 _randomOffset;
        float _riseOffset;
        bool _isDisplaying;

        Color _originalColor;
        Coroutine _displayRoutine;

        enum DamegeText
        {
            DamageText
        }

        protected override void StartInit()
        {
        }

        protected override void AwakeInit()
        {
            Bind<TMP_Text>(typeof(DamegeText));
            _damageText = GetText((int)DamegeText.DamageText);
            _damageTextRectTransform = _damageText.rectTransform;
            _damageText.raycastTarget = false;
            _originalColor = _damageText.color;
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
        }

        private void OnEnable()
        {
            UnityEngine.Canvas.willRenderCanvases += UpdateScreenPositionBeforeCanvasRender;
        }

        private void OnDisable()
        {
            UnityEngine.Canvas.willRenderCanvases -= UpdateScreenPositionBeforeCanvasRender;

            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
                _displayRoutine = null;
            }

            _isDisplaying = false;
            _damageText.color = _originalColor;
            _damageText.enabled = true;
        }

        IEnumerator DisplayDamage()
        {
            Color startColor = _damageText.color;
            float elapsed = 0f;

            while (elapsed < LifetimeSeconds)
            {
                elapsed += Time.deltaTime;
                _riseOffset += RiseSpeedPerSecond * Time.deltaTime;

                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, elapsed / LifetimeSeconds);
                _damageText.color = color;

                yield return null;
            }

            _isDisplaying = false;
            _resourcesServices.DestroyObject(gameObject);
        }

        public void SetDamage(Transform targetTransform, int damage)
        {
            SetupScreenSpaceCanvas();
            _targetTransform = targetTransform;
            _targetLocalAnchorPosition = targetTransform.InverseTransformPoint(GetTargetAnchorWorldPosition(targetTransform));
            _randomOffset = new Vector2(
                Random.Range(-RandomOffsetRange.x, RandomOffsetRange.x),
                Random.Range(-RandomOffsetRange.y, RandomOffsetRange.y));
            _riseOffset = 0f;

            _damageText.text = damage.ToString();
            _damageText.color = _originalColor;
            _damageText.enabled = true;
            _isDisplaying = true;
            UpdateScreenPosition();

            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
            }

            _displayRoutine = StartCoroutine(DisplayDamage());
        }

        private void SetupScreenSpaceCanvas()
        {
            Canvas canvas = Canvas;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.overrideSorting = true;
            canvas.sortingOrder = SortingOrder;
            transform.localScale = Vector3.one;
        }

        private void UpdateScreenPositionBeforeCanvasRender()
        {
            if (_isDisplaying == false)
            {
                return;
            }

            UpdateScreenPosition();
        }

        private void UpdateScreenPosition()
        {
            if (_targetTransform == null)
            {
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 anchorWorldPosition = _targetTransform.TransformPoint(_targetLocalAnchorPosition);
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(anchorWorldPosition);
            if (IsInViewport(viewportPosition) == false)
            {
                _damageText.enabled = false;
                return;
            }

            _damageText.enabled = true;

            Vector2 canvasSize = GetCanvasSize();
            Vector2 anchoredPosition = new Vector2(
                (viewportPosition.x - 0.5f) * canvasSize.x,
                (viewportPosition.y - 0.5f) * canvasSize.y);

            Vector2 finalPosition = anchoredPosition + _randomOffset + Vector2.up * (StartYOffset + _riseOffset);
            finalPosition.x = Mathf.Round(finalPosition.x);
            finalPosition.y = Mathf.Round(finalPosition.y);
            _damageTextRectTransform.anchoredPosition = finalPosition;
        }

        private bool IsInViewport(Vector3 viewportPosition)
        {
            return viewportPosition.z > 0f &&
                   viewportPosition.x >= 0f &&
                   viewportPosition.x <= 1f &&
                   viewportPosition.y >= 0f &&
                   viewportPosition.y <= 1f;
        }

        private Vector3 GetTargetAnchorWorldPosition(Transform targetTransform)
        {
            Collider col = targetTransform.GetComponentInChildren<Collider>();
            if (col != null)
            {
                return col.bounds.center;
            }

            return targetTransform.position;
        }

        private Vector2 GetCanvasSize()
        {
            Canvas canvas = Canvas;
            if (canvas.pixelRect.size != Vector2.zero)
            {
                return canvas.pixelRect.size;
            }

            return new Vector2(Screen.width, Screen.height);
        }
    }
}
