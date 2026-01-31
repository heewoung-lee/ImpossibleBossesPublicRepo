using System;
using System.Collections.Generic;
using Controller;
using Scene.CommonInstaller;
using UnityEngine;
using VFX;
using Zenject;

namespace GameManagers.Target
{
    /// <summary>
    /// 1.4 수정 모든 stats를 가진 객체들이 상속 받아서 사용해야한다
    /// 타겟팅 쓰킬을 쓸 때, 스킨드 메쉬가 조각조각 달려있는 경우가 많아.
    /// 타겟팅을 할때 아래 자식들까지의 메쉬를 가져와 사용하는 머테리얼을 갈아껴야하는데 문제는
    /// 타겟팅 쓸때마다 자기를 포함한 자식들을 순회하면서 메쉬를 가져와야해서 연산이 폭주함.
    /// 그래서 stats를 상속받는애들은 TargetableUnit컴포넌트를 만드시 상속받게 해놨고
    /// ITargetInteractable 인터페이스로 타겟팅 스킬에 대한 상호작용을 하게 만듦
    /// </summary>
    public interface ITargetInteractable
    {
        void SetHighlight(Material mat); // 타겟팅 하이라이트 켜기
        void RemoveHighlight(); // 타겟팅 하이라이트 끄기
        GameObject GetGameObject(); // 성공시에 타겟 오브젝트 반환
    }

    public interface ITargetManager
    {
        public void StartTargeting(LayerMask targetLayer, Material highlightMat, Action<GameObject> onSelected,
            Action onCanceled,Func<GameObject, bool> customValidator = null);

        void StartAreaTargeting(float radius, LayerMask targetLayer, Material indicatorMat, Action<Vector3> onSelected,
            Action onCanceled);

        void StopTargeting();
    }

    public interface ITargetingState
    {
        void Enter(); // 상태 시작 (초기화, 커서 변경 등)
        void Update(); // 매 프레임 로직 (Raycast, 하이라이트 갱신)
        void Exit(); // 상태 종료 (정리, 하이라이트 끄기)
        void OnCancel(); // 취소 시 행동

        //1.22일 추가 타겟팅을 하던중에 다른 타겟팅을 하면,
        //타겟매니저가 State를 전환하는데 문제는
        //이전 스테이트가 실패인지 성공인지 결정해줘야하는 상태에서
        //전환만 되다 보니 실패 성공 여부를 결정지어야하는 루프가 계속 돌아감
        //Iscomplete를 추가해 성공을 하면 전환이 되더라도,
        //oncancel이 호출안되게끔 했고, 그외 상태전환은 oncancel이 호출되도록 만듦
        bool IsComplete { get; set; }
    }

    /// <summary>
    /// 1.6일 수정 타겟매니저가 많은 상태들을 관리해서 무거워짐.
    /// 타겟매니저는 상태만 갈아끼우는 매니저 역할만 하고
    /// 나머지 상태들은 각자 구현하는걸로 변경
    /// </summary>
    public class TargetManager : MonoBehaviour, IInitializable, ITargetManager, ITickable
    {
        private ICursorService _cursorService;
        private Camera _mainCamera;
        private IRegistrar<ITargetManager> _providerRegistar;
        
        //현재 상태를 담는 변수
        private ITargetingState _currentState;

        // 공용 리소스 (State들이 가져다 쓸 수 있게 공개하거나 Context로 묶어서 전달)
        public Camera MainCamera => _mainCamera;
        public ICursorService CursorService => _cursorService;
        public Transform IndicatorRoot { get; private set; }
        public IndicatorController IndicatorCtrl { get; private set; }

        private const string INDICATOR_PATH = "Prefabs/Player/VFX/Common/RangeSkillIndicator";

        [Inject]
        public void Construct(Camera mainCamera, ICursorService cursorService,IRegistrar<ITargetManager> providerRegistar)
        {
            _mainCamera = mainCamera;
            _cursorService = cursorService;
            _providerRegistar = providerRegistar;
        }

        public void Initialize()
        {
            _providerRegistar.Register(this);
            CreateIndicatorInstance();
            ChangeState(new IdleState()); // 기본 상태
        }

        private void CreateIndicatorInstance() //상태들이 필요한 아이템들은 여기에 초기화
        {
            if (IndicatorRoot != null) return;

            GameObject prefab = Resources.Load<GameObject>(INDICATOR_PATH);

            if (prefab == null)
            {
                Debug.LogError($"[TargetManager] 인디케이터를 로드 실패! 경로: Resources/{INDICATOR_PATH}");
                return;
            }

            GameObject go = Instantiate(prefab);
            go.transform.SetParent(this.transform); // 매니저 자식으로 두어 관리
            go.SetActive(false); // 기본 상태는 꺼둠

            IndicatorRoot = go.transform;

            IndicatorCtrl = go.GetComponent<IndicatorController>()
                            ?? go.GetComponentInChildren<IndicatorController>();
        }

        // 상태 변경 함수
        private void ChangeState(ITargetingState newState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
                if (_currentState.IsComplete == false)
                {
                    _currentState.OnCancel();
                }
            }
            _currentState = newState;
            _currentState.Enter();
        }

        public void Tick() // Update 대신 Zenject의 ITickable
        {

            if (_currentState == null) return;
            _currentState.Update();
        }

        public void StartTargeting(LayerMask targetLayer, Material highlightMat, Action<GameObject> onSelected,
            Action onCanceled,Func<GameObject, bool> customValidator = null)
        {
            ChangeState(new UnitTargetState(this, targetLayer, highlightMat, onSelected, onCanceled,customValidator));
        }

        public void StartAreaTargeting(float radius, LayerMask targetLayer, Material indicatorMat,
            Action<Vector3> onSelected, Action onCanceled)
        {
            ChangeState(new AreaTargetState(this, radius, targetLayer, indicatorMat, onSelected, onCanceled));
        }

        public void CancelTargeting()
        {
            if (_currentState != null)
                _currentState.OnCancel(); // 현재 상태의 취소 로직 실행
            ChangeState(new IdleState()); // 대기 상태로 복귀
        }

        private void OnDestroy()
        {
            StopTargeting();
            _providerRegistar.Unregister(this);            
        }

        public void StopTargeting()
        {
            Debug.Log("[TargetManager] StopTargeting -> Idle");
            ChangeState(new IdleState());
            CursorService.Set(CursorState.Base);
        }
    }
}