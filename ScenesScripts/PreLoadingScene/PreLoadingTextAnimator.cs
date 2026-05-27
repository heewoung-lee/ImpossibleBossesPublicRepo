using System.Collections;
using TMPro;
using UnityEngine;

namespace ScenesScripts
{
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(RectTransform))]
    public class PreLoadingTextAnimator : MonoBehaviour
    {
        private static readonly string[] GameTips =
        {
            "TIP. 보스의 큰 공격은 대부분 선딜레이가 있습니다.",
            "TIP. 보스 패턴이 바뀌는 순간에는 무리하게 공격하지 않는 것이 좋습니다.",
            "TIP. A키를 눌러 기본공격을 할 수 있습니다.",
            "TIP. Z키를 눌러 아이템이나 NPC에 상호작용 할 수 있습니다.",
        };

        private const float FadeSeconds = 0.25f;
        private const float VisibleSeconds = 2.5f;
        private const float MoveDistance = 12f;

        private TMP_Text _text;
        private RectTransform _rectTransform;
        private Vector2 _baseAnchoredPosition;
        private string[] _shuffledGameTips;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
            _baseAnchoredPosition = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            ShuffleGameTips();
            StartCoroutine(PlayTipAnimation());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _rectTransform.anchoredPosition = _baseAnchoredPosition;
        }

        private IEnumerator PlayTipAnimation()
        {
            int tipIndex = 0;

            while (enabled && gameObject.activeInHierarchy)
            {
                _text.text = _shuffledGameTips[tipIndex];

                yield return Fade(0f, 1f, FadeSeconds);
                yield return Move(VisibleSeconds);
                yield return Fade(1f, 0f, FadeSeconds);

                tipIndex++;
                if (tipIndex >= _shuffledGameTips.Length)
                {
                    ShuffleGameTips();
                    tipIndex = 0;
                }
            }
        }

        private void ShuffleGameTips()
        {
            _shuffledGameTips = new string[GameTips.Length];
            GameTips.CopyTo(_shuffledGameTips, 0);

            for (int i = _shuffledGameTips.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                string temp = _shuffledGameTips[i];
                _shuffledGameTips[i] = _shuffledGameTips[randomIndex];
                _shuffledGameTips[randomIndex] = temp;
            }
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                _text.alpha = Mathf.Lerp(from, to, timer / duration);
                yield return null;
            }

            _text.alpha = to;
        }

        private IEnumerator Move(float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float moveY = Mathf.Sin(timer * Mathf.PI / duration) * MoveDistance;
                _rectTransform.anchoredPosition = _baseAnchoredPosition + new Vector2(0f, moveY);
                yield return null;
            }

            _rectTransform.anchoredPosition = _baseAnchoredPosition;
        }
    }
}
