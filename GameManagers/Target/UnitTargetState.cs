using System;
using Controller;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManagers.Target
{
    public class UnitTargetState : ITargetingState
    {
        private readonly TargetManager _targetManager;
        private readonly LayerMask _layer;
        private readonly Material _mat;
        private readonly Action<GameObject> _onSelected;
        private readonly Action _onCanceled;
        private readonly Func<GameObject, bool> _customValidator;
        // 1.29일 추가 타겟을 추가필터링으로 거르기 위한 함수 예를들어 지금의 타겟은 Layer로 모든걸 감지하지만
        // 레이어로 감지된 애들중에 죽은애들 혹은 아닌 애들을 감지하기 위함.
        public bool IsComplete { get; set; } = false;
        
        
        private ITargetInteractable _currentTarget;

        public UnitTargetState(TargetManager targetManager, LayerMask layer,
            Material mat, Action<GameObject> onSelected, Action onCanceled, Func<GameObject, bool> customValidator)
        {
            _targetManager = targetManager;
            _layer = layer;
            _mat = mat;
            _onSelected = onSelected;
            _onCanceled = onCanceled;
            _customValidator = customValidator;
        }

        public void Enter()
        {
            _targetManager.CursorService.Set(CursorState.Attack); // 커서 변경
        }

        public void Update()
        {
            
            // 취소 입력
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancel();
                return;
            }
            
            Ray ray = _targetManager.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _layer))
            {
                ITargetInteractable hitUnit = hit.collider.GetComponentInParent<ITargetInteractable>();
                if (hitUnit == null)
                {
                    ClearHighlight();
                    return;
                }
                GameObject hitGo = hitUnit.GetGameObject();
                
                if (_customValidator != null && _customValidator(hitGo) == false)
                {
                    ClearHighlight();
                    return;
                }
                
                // 타겟 변경 감지
                if (_currentTarget != hitUnit)
                {
                    ClearHighlight();
                    _currentTarget = hitUnit;
                    if(_currentTarget != null)
                    {
                        _currentTarget.SetHighlight(_mat);
                    }
                }

                // 클릭
                if (Input.GetMouseButtonDown(0) && _currentTarget != null)
                {
                    IsComplete = true;
                    _onSelected?.Invoke(_currentTarget.GetGameObject());
                    _targetManager.StopTargeting();
                    //1.7일 수정 채널링이 추가되면서 강제로IDLE로 진입하는건 문제가 있어서
                    //콜백이 다음상태를 전환하도록 수정
                    return;
                }
            }
            else
            {
                ClearHighlight();
            }
        }

        public void Exit()
        {
            Debug.Log("[UnitTargetState] Exit");
            ClearHighlight();
            _targetManager.CursorService.Reset();
        }

        public void OnCancel()
        {
            Debug.Log("[UnitTargetState] OnCancel]");
            _onCanceled?.Invoke();
        }


        private void ClearHighlight()
        {
            if (_currentTarget != null)
            {
                _currentTarget.RemoveHighlight();
                _currentTarget = null;
            }
        }
    }
}