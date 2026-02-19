using System.Collections;
using GameManagers;
using GameManagers.Interface.UIManager;
using UI;
using UI.Scene.SceneUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Zenject;

namespace Skill
{
    public class SkillComponent : UIBase
    {
        private IUIManagerServices _uiManagerServices;
        [Inject]
        public void Construct(IUIManagerServices uiManagerServices)
        {
            _uiManagerServices = uiManagerServices;
            Initialize();
        }

        enum SkillImage
        {
            SkillIconImage,
            CoolTimeImg
        }

        private RuntimeSkill _connectSkill;

        private Image _iconimage;
        private Image _coolTimeImg;
        
        private UIDescription _decriptionObject;
        private RectTransform _skillComponentRectTr;
        public void SetSkillComponent(RuntimeSkill skill)
        {
            //혹시라도 중복 있다면 제거
            if (_connectSkill != null)
            {
                _connectSkill.OnCompleteSkill -= HandleOnCompleteSkill;
            }
            
            _connectSkill = skill;
            if (_connectSkill != null)
            {
                _connectSkill.OnCompleteSkill += HandleOnCompleteSkill; //스킬 성공여부 구독
                if (_connectSkill.Data.icon != null)
                {
                    _iconimage.sprite = _connectSkill.Data.icon;
                }
            }
            _coolTimeImg.fillAmount = 0; 
        }
        protected override void AwakeInit()
        {
            Bind<Image>(typeof(SkillImage));
            _iconimage = Get<Image>((int)SkillImage.SkillIconImage);
            _coolTimeImg = Get<Image>((int)SkillImage.CoolTimeImg);
            _skillComponentRectTr = transform as RectTransform;
           
        }
        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            BindEvent(_iconimage.gameObject, ClicktoSkill);
            BindEvent(gameObject, ShowDescription, Define.UIEvent.PointerEnter);
            BindEvent(gameObject, CloseDescription, Define.UIEvent.PointerExit);
        }
        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            UnBindEvent(_iconimage.gameObject, ClicktoSkill);
            UnBindEvent(gameObject, ShowDescription, Define.UIEvent.PointerEnter);
            UnBindEvent(gameObject, CloseDescription, Define.UIEvent.PointerExit);
            
            if (_connectSkill != null) 
            {
                _connectSkill.OnCompleteSkill -= HandleOnCompleteSkill;
            }
            
        }
        private void ShowDescription(PointerEventData data)
        {
            if (_connectSkill == null || _connectSkill.Data == null) return;
            
            _decriptionObject.UI_DescriptionEnable();

            _decriptionObject.DescriptionWindow.transform.position
                = _decriptionObject.SetDecriptionPos(transform, _skillComponentRectTr.rect.width, _skillComponentRectTr.rect.height);

            _decriptionObject.SetValue(_connectSkill.Data);
        }
        private void CloseDescription(PointerEventData data)
        {
            _decriptionObject.UI_DescriptionDisable();
            _decriptionObject.SetdecriptionOriginPos();
        }
        public void ClicktoSkill(PointerEventData eventdata)
        {
            SkillStart();
        }
        public void SkillStart()
        {
            if (_connectSkill == null) return;
            
            if (_connectSkill.IsReady)
            {
                _connectSkill.Use();//사용하기만 하고 쿨타임이라든지 사용에 대한 결정은 오직 스킬의 전략에서만 
            }
        }
        private void HandleOnCompleteSkill() //이제 여기서 쿨타임 연출
        {
            if (gameObject.activeInHierarchy) // 코루틴 실행 전 활성화 체크
            {
                UtilDebug.Log("[UI] OnCompleteSkill received -> start cooldown");
                StartCoroutine(TriggerCooldown());
            }
        }
        private IEnumerator TriggerCooldown()
        {
            float duration = _connectSkill.Data.cooldown;
            
            _coolTimeImg.fillAmount = 1;
            
            while (_coolTimeImg.fillAmount > 0)
            {
                _coolTimeImg.fillAmount -= Time.deltaTime / duration;
                yield return null;  
            }
            _coolTimeImg.fillAmount = 0;
        }

        public void AttachItemToSlot(GameObject go, Transform slot)
        {
            go.transform.SetParent(slot);
            go.GetComponent<RectTransform>().anchorMin = Vector2.zero; // 좌측 하단 (0, 0)
            go.GetComponent<RectTransform>().anchorMax = Vector2.one;  // 우측 상단 (1, 1)
            go.GetComponent<RectTransform>().offsetMin = Vector2.zero; // 오프셋 제거
            go.GetComponent<RectTransform>().offsetMax = Vector2.zero; // 오프셋 제거
        }
        protected override void StartInit()
        {
            _decriptionObject = _uiManagerServices.Get_Scene_UI<UIDescription>();
        
        }

    }
}