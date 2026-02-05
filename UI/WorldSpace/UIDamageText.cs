using System.Collections;
using TMPro;
using UnityEngine;
using Util;

namespace UI.WorldSpace
{
    public class UIDamageText : UIBase
    {
        TMP_Text _damageText;

        Color _originalColor;
        Vector3 _originalTransform;

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
            _originalColor = _damageText.color;
            _originalTransform = transform.position;
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
        }

        private void OnEnable()
        {
            StartCoroutine(DisplayDamage());
        }

        private void OnDisable()
        {
            _damageText.color = _originalColor;
            transform.position = _originalTransform;
        }

        IEnumerator DisplayDamage()
        {
            Color color = _damageText.color;
            while (true)
            {
                transform.position += Vector3.up * 0.01f;
                color.a -= 0.01f;
                _damageText.color = color;
                if (color.a <= 0)
                {
                    _resourcesServices.DestroyObject(gameObject);
                    break;
                }

                yield return null;
            }
        }

        public void SetTextAndPosition(Transform parentTr, int damage, float offset)
        {
            _damageText.text = damage.ToString();

            Vector3 damageTextPos;
            if (parentTr.TryGetComponentInChildren(out HeadTr headTr))
            {
                damageTextPos = headTr.transform.position;
            }
            else
            {
                if (parentTr.TryGetComponentInChildren(out Collider col))
                {
                    damageTextPos =  parentTr.position + Vector3.up *  col.bounds.size.y;
                }
                else
                {
                    damageTextPos = parentTr.position;
                }
            }
            
            damageTextPos += Vector3.up * offset;
            transform.position = damageTextPos;
        }

        public void LateUpdate()
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}