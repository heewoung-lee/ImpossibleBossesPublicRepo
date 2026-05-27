using System.Collections;
using GameManagers.GameManagerExManagement;
using Stats.BossStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIBossHp : UIScene
    {
        private IBossSpawnManager _bossSpawnManager;

        [Inject]
        public void Construct(IBossSpawnManager bossSpawnManager)
        {
            _bossSpawnManager = bossSpawnManager;
        }

        private enum HpSlider
        {
            BossHpSlider
        }

        private enum HpText
        {
            HpText
        }

        private Slider _hpSlider;
        private TMP_Text _hpText;
        private BossStats _stats;
        private Coroutine _hpAnimationCoroutine;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Slider>(typeof(HpSlider));
            Bind<TMP_Text>(typeof(HpText));

            _hpText = GetText((int)HpText.HpText);
            _hpSlider = Get<Slider>((int)HpSlider.BossHpSlider);
            _hpSlider.value = 0f;
        }

        protected override void StartInit()
        {
            if (_bossSpawnManager.GetBossMonster() != null)
            {
                BindBossStats();
                return;
            }

            _bossSpawnManager.OnBossSpawnEvent += BindBossStats;
        }

        private void OnDestroy()
        {
            _bossSpawnManager.OnBossSpawnEvent -= BindBossStats;
            UnbindBossStats();
        }

        private void BindBossStats()
        {
            _bossSpawnManager.OnBossSpawnEvent -= BindBossStats;
            BossStats nextStats = _bossSpawnManager.GetBossMonster().GetComponent<BossStats>();

            if (_stats == nextStats)
            {
                return;
            }

            UnbindBossStats();
            _stats = nextStats;
            _stats.CurrentHpValueChangedEvent += Stats_CurrentHPValueChangedEvent;
            _stats.MaxHpValueChangedEvent += Stats_CurrentMAXHPValueChangedEvent;

            if (_stats.MaxHp <= 0)
            {
                return;
            }

            _hpText.text = $"{_stats.Hp} / {_stats.MaxHp}";
            RestartSliderAnimation(0f, GetCurrentHpRatio(), 1f);
        }

        private void UnbindBossStats()
        {
            if (_stats == null)
            {
                return;
            }

            _stats.CurrentHpValueChangedEvent -= Stats_CurrentHPValueChangedEvent;
            _stats.MaxHpValueChangedEvent -= Stats_CurrentMAXHPValueChangedEvent;
            _stats = null;
        }

        private void Stats_CurrentMAXHPValueChangedEvent(int preCurrentMaxHp, int currentMaxHp)
        {
            _hpText.text = $"{_stats.Hp} / {currentMaxHp}";
            RestartSliderAnimation(_hpSlider.value, GetCurrentHpRatio(), 0.2f);
        }

        private void Stats_CurrentHPValueChangedEvent(int preCurrentHp, int currentHp)
        {
            if (_stats.MaxHp <= 0)
            {
                return;
            }

            _hpText.text = $"{currentHp} / {_stats.MaxHp}";
            RestartSliderAnimation(_hpSlider.value, GetCurrentHpRatio(), 0.35f);
        }

        private float GetCurrentHpRatio()
        {
            return (float)_stats.Hp / _stats.MaxHp;
        }

        private void RestartSliderAnimation(float fromValue, float toValue, float duration)
        {
            if (_hpAnimationCoroutine != null)
            {
                StopCoroutine(_hpAnimationCoroutine);
            }

            _hpAnimationCoroutine = StartCoroutine(AnimationHp(fromValue, toValue, duration));
        }

        private IEnumerator AnimationHp(float fromValue, float toValue, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _hpSlider.value = Mathf.Lerp(fromValue, toValue, elapsedTime / duration);
                yield return null;
            }

            _hpSlider.value = toValue;
            _hpAnimationCoroutine = null;
        }
    }
}
