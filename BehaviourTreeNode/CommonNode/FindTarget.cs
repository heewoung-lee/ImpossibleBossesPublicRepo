using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Controller;
using Stats.BaseStats;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.CommonNode
{
    [TaskDescription("Finds the closest valid player and sets it as the target.")]
    [TaskCategory("CustomNode")]
    public class FindTarget : Action
    {
        [SerializeField] private SharedGameObject _targetObject;
        [SerializeField] private SharedBool _isTaunted; 
        private BossController _bossController;
        
        // 가비지 생성을 막기 위한 NonAlloc 배열
        private Collider[] _targetObjects;

        public override void OnAwake()
        {
            base.OnAwake();
            _targetObjects = new Collider[Define.MaxPlayer];
            _bossController = Owner.GetComponent<BossController>(); 
        }

        public override TaskStatus OnUpdate()
        {
            // 도발 상태일 때
            if (_isTaunted.Value == true)
            {
                // 컨트롤러가 들고 있는 최신 타겟을 강제로 가져옴
                var controllerTarget = _bossController.TargetObjectInBehaviourTree;
                if (controllerTarget != null)
                {
                    _targetObject.Value = controllerTarget;
                    return TaskStatus.Success;
                }
            }

            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position, 
                float.MaxValue, // 필요시 보스의 최대 인지 범위로 변경
                _targetObjects, 
                LayerMask.GetMask(Utill.GetLayerID(Define.ControllerLayer.Player), Utill.GetLayerID(Define.ControllerLayer.AnotherPlayer))
            );

            GameObject closestPlayer = EnemyFindTarget.FindNearestPlayer(_targetObjects,hitCount,_bossController.gameObject);
            
            
            // 타겟 갱신 및 결과 반환
            if (closestPlayer != null)
            {
                _targetObject.Value = closestPlayer;
                return TaskStatus.Success;
            }

            // 살아있는 유효한 타겟이 없다면 변수를 비우고 실패 반환
            _targetObject.Value = null;
            return TaskStatus.Failure;
        }
    }
}