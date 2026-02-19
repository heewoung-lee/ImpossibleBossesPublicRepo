using UnityEngine;
using Zenject;

namespace CoreScripts
{
    /// <summary>
    /// 해당 스크립트는 컴포넌트들을 위해 만들어짐
    /// 컴포넌트들은 가진 프리펩이 없으니 팩토리를 만들수도 없고 또한 일일히 바인드를 하자니 관리가 어려워 지기 때문에
    /// 그래서 컴포넌트들을 위한 ZenjectMonobehaviour를 만들었고 만약 컴포넌트가 젠젝트 주입 이후에
    /// 초기화 로직이 필요하다면 해당 스크립트를 상속받고 InitAfterInject에 초기화 로직을 작성하면됨
    /// 단 Enable이나 Awake는 절대 의존성을 주입받는 객체를 쓰지말 것 초기화 순서가 Awake-> Enable -> Initialize-> Start 순이니 참고
    /// </summary>
    public abstract class ZenjectMonoBehaviour : MonoBehaviour,IInitializable
    {
        private bool _isDoneInject = false;

        protected virtual void ZenjectEnable(){}
        protected virtual void ZenjectDisable(){}
        protected virtual void InitAfterInject(){}

        private void OnEnable()
        {
            if (_isDoneInject == false)
            {
                //UtilDebug.LogError($"{gameObject.name} hasn't been injected yet.");
                return;
            }
            ZenjectEnable();
        }

        private void OnDisable()
        {
            if (_isDoneInject == false)
            {
                //UtilDebug.LogError($"{gameObject.name} hasn't been injected yet.");
                return;
            }
            
            ZenjectDisable();
        }

        public void Initialize()
        {
            // 12.8일 추가 혹시나 컴포넌트가 아닌 팩토리 객체가 이 스크립트를 상속받을 경우
            // Initialize가 2번 호출 될 수 있으므로 방어로직 추가 
            if (_isDoneInject) 
                return;
            
            _isDoneInject = true;
            if (gameObject.activeSelf == true)
            {
                InitAfterInject();
                ZenjectEnable();
            }    
        }
    }
}
