using GameManagers;
using GameManagers.PoolManagement;
using GameManagers.UIManagement;
using Stats.BaseStats;
using UI.WorldSpace;
using UnityEngine;
using Zenject;

namespace Module.CommonModule
{
    public class ModuleDamageText : MonoBehaviour
    {
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private LocalPoolManager _localPoolManager;
        private void Start()
        {
            BaseStats stat = GetComponent<BaseStats>();
            stat.EventAttacked += ShowDamageText_UI;
        }
        public void ShowDamageText_UI(int damage, int currentHp)
        {
            UIDamageText uIDamageText = _uiManagerServices.MakeUIWorldSpaceUI<UIDamageText>();

            // 2026-05-22: 데미지 텍스트를 피격 대상 자식으로 붙이면 대상이 사망/풀링될 때 텍스트도 같이 비활성화된다.
            // 위치 추적은 SetDamage에서 targetTransform으로 계산하므로, 부모는 대상이 아니라 기존 UIDamageText 풀 루트를 사용한다.
            if (_localPoolManager.TryGetPoolRoot(uIDamageText.gameObject, out Transform damageTextRoot) == true)
            {
                uIDamageText.transform.SetParent(damageTextRoot);
            }

            uIDamageText.SetDamage(transform, damage);
        }
    }
}
