using System.Collections;
using Stats.BaseStats;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.WorldSpace
{
    public class UIHpBar : UIBase
    {
        private readonly Vector3 _offsetHpbar = new Vector3(0, 0.5f,0);
        BaseStats _stats;
        Slider _hpSlider;
        CanvasGroup _canvasGroup;
        bool _isDamaged = false;
        enum HpBarSlider
        {
            HpBar
        }

        protected override void AwakeInit()
        {
            Bind<Slider>(typeof(HpBarSlider));
            _hpSlider = Get<Slider>((int)HpBarSlider.HpBar);
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;//처음에는 HP가 안보이게 설정 맞았을때 보이게끔 설정
        }

        protected override void StartInit()
        {
            _stats = GetComponentInParent<BaseStats>();

            Vector3 uiHpBarPos;
            if (_stats.transform.TryGetComponentInChildren(out HeadTr headTr))
            {
                uiHpBarPos = headTr.transform.position;
            }
            else
            {
                if (_stats.transform.TryGetComponentInChildren(out Collider col))
                {
                    uiHpBarPos = new Vector3(_stats.transform.position.x, col.bounds.max.y, _stats.transform.position.z);
                    //uiHpBarPos =  _stats.transform.position + Vector3.up *  col.bounds.size.y;
                }
                else
                {
                    uiHpBarPos = _stats.transform.position;
                }
            }
            
            transform.position = _offsetHpbar + uiHpBarPos;
            _stats.EventAttacked += SetHpUI;
        }

        void LateUpdate()
        {
            if(_isDamaged)
                transform.rotation = Camera.main.transform.rotation;
        }
        public void SetHpUI(int damage, int currentHp)
        {
            _isDamaged = true;
            _hpSlider.value = (float)currentHp / (float)_stats.MaxHp;
            _canvasGroup.alpha = 1f;
            StopCoroutine(Hpbar_fadeaway());
            StartCoroutine(Hpbar_fadeaway());

        }

        IEnumerator Hpbar_fadeaway()
        {
            while (true)
            {
                _canvasGroup.alpha -= 0.3f * Time.deltaTime;

                if(_canvasGroup.alpha <= 0f)
                {
                    _isDamaged = false;
                    yield break;
                }
                yield return null;
            }
        }
    }
}
