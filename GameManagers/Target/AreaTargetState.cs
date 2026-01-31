using System;
using System.Collections.Generic;
using Controller;
using UnityEngine;

namespace GameManagers.Target
{
    public class AreaTargetState : ITargetingState
    {
        private readonly TargetManager _targetManager;
        private readonly float _radius;
        private readonly LayerMask _checkLayer;    
        private readonly Material _highlightMat;  
        private readonly Action<Vector3> _onSelected;
        private readonly Action _onCanceled;
        
        private readonly LayerMask _groundLayer;
        private HashSet<ITargetInteractable> _currentTargets = new HashSet<ITargetInteractable>();
        private const float RayDistance = 100f;

        public bool IsComplete { get; set; } = false;
        
        private bool _canceled;
        public AreaTargetState(
            TargetManager targetManager, 
            float radius, 
            LayerMask checkLayer, 
            Material highlightMat, 
            Action<Vector3> onSelected, 
            Action onCanceled)
        {
            _targetManager = targetManager;
            _radius = radius;
            _checkLayer = checkLayer;
            _highlightMat = highlightMat;
            _onSelected = onSelected;
            _onCanceled = onCanceled;
            _groundLayer = LayerMask.GetMask("Ground");
        }

        public void Enter()
        {
            // 인디케이터 켜기 및 사이즈 설정
            if (_targetManager.IndicatorRoot != null)
            {
                _targetManager.IndicatorRoot.gameObject.SetActive(true);
                _targetManager.IndicatorCtrl.SetTargetingPreview(_radius);
            }
            
            // 커서 변경
            _targetManager.CursorService.Set(CursorState.Attack);
        }

        public void Update()
        {
            
            //취소 입력 처리
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancel();
                return;
            }

            //마우스 레이캐스트 (바닥 감지)
            Ray ray = _targetManager.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, RayDistance, _groundLayer))
            {
                Vector3 point = hit.point;

                //인디케이터 이동 및 타겟 감지 로직 수행
                UpdateTargetingLogic(point);

                if (Input.GetMouseButtonDown(0))
                {
                    IsComplete = true;
                    _onSelected?.Invoke(point);
                    _targetManager.StopTargeting();
                }
            }
            else
            {
                HandleInvalidPosition();
            }
        }

        /// <summary>
        /// 바닥 위 유효한 좌표에서의 로직
        /// </summary>
        private void UpdateTargetingLogic(Vector3 centerPoint)
        {
            // 인디케이터 이동
            if (_targetManager.IndicatorRoot != null)
            {
                if (!_targetManager.IndicatorRoot.gameObject.activeSelf)
                    _targetManager.IndicatorRoot.gameObject.SetActive(true);

                _targetManager.IndicatorCtrl.SetTargetingPosition(centerPoint + Vector3.up * 0.05f);
            }

            //범위 내 타겟 감지
            Collider[] hits = Physics.OverlapSphere(centerPoint, _radius, _checkLayer);


            // 이번 프레임에 감지된 유닛들 수집
            HashSet<ITargetInteractable> currentFrameTargets = new HashSet<ITargetInteractable>();
            
            foreach (var col in hits)
            {
                ITargetInteractable unit = col.GetComponentInParent<ITargetInteractable>();
                if (unit != null)
                {
                    currentFrameTargets.Add(unit);
                    // 이미 켜져 있어도 중복 호출 비용이 적다면 그냥 호출 (혹은 Contains 체크 후 호출)
                    unit.SetHighlight(_highlightMat); 
                }
            }

            //이전 프레임엔 있었는데, 지금은 범위 밖으로 나간 유닛 -> 하이라이트 끄기
            foreach (var prevUnit in _currentTargets)
            {
                if (!currentFrameTargets.Contains(prevUnit))
                {
                    prevUnit.RemoveHighlight();
                }
            }

            // 현재 목록 갱신
            _currentTargets = currentFrameTargets;
            
        }

        /// <summary>
        /// 마우스가 유효하지 않은 곳을 가리킬 때
        /// </summary>
        private void HandleInvalidPosition()
        {
            // 인디케이터 숨기기
            if (_targetManager.IndicatorRoot != null && _targetManager.IndicatorRoot.gameObject.activeSelf)
                _targetManager.IndicatorRoot.gameObject.SetActive(false);

            // 기존 하이라이트 모두 끄기
            ClearAllHighlights();

        }

        public void Exit()
        {
            ClearAllHighlights();
            // 인디케이터 끄기
            if (_targetManager.IndicatorRoot != null)
                _targetManager.IndicatorRoot.gameObject.SetActive(false);
            // 커서 리셋
            _targetManager.CursorService.Reset();
        }

        public void OnCancel()
        {
            _onCanceled?.Invoke();
        }



        private void ClearAllHighlights()
        {
            foreach (var unit in _currentTargets)
            {
                unit.RemoveHighlight();
            }
            _currentTargets.Clear();
        }
    }
}