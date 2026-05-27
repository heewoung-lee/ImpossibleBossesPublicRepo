using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using Controller;
using Stats.BaseStats;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.CommonNode
{
    [TaskDescription("Seek the target specified using the Unity NavMesh.")]
    [TaskCategory("CustomNode")]
    [BehaviorDesigner.Runtime.Tasks.HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("3278c95539f686f47a519013713b31ac", "9f01c6fc9429bae4bacb3d426405ffe4")]
    public class LegacyChaser : NavMeshMovement
    {
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If target is null then use the target position")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetPosition")]
        [SerializeField]private SharedBool _hasArrived;
        private BossController _controller;
        [SerializeField]private SharedGameObject _targetObject;
        private Collider[] _targetObjects;

        public override void OnAwake()
        {
            base.OnAwake();
            _controller = Owner.GetComponent<BossController>();
            _targetObjects = new Collider[Define.MaxPlayer];
        }
        public override void OnStart()
        {
            base.OnStart();
            
            //현재 상태가 무브가 아닐시에만 무브로 바꾸고 무브가 맞다면 안바꿈. 그래야 애니메이션이 자연스럽게 연결됨
            if (_controller.CurrentStateType != _controller.BaseMoveState)
            {
                _controller.UpdateMove();
            }
            _hasArrived.Value = false;

            if (_targetObject.Value == null)
            {
                Physics.OverlapSphereNonAlloc(transform.position, float.MaxValue, _targetObjects, 
                    LayerMask.GetMask(Utill.GetLayerID(Define.ControllerLayer.Player), Utill.GetLayerID(Define.ControllerLayer.AnotherPlayer)
                ));
                float findClosePlayer = float.MaxValue;
                foreach (Collider collider in _targetObjects)
                {
                    if(collider == null)
                        continue;
                    
                    if (collider.TryGetComponent(out BaseStats baseStats))
                    {
                        if (baseStats.IsDead == true)
                            continue;
                    }
                    if (collider.TryGetComponent(out ITargetable targetable))
                    {
                        if (targetable.IsTargetable == false)
                            continue; // 은신 중이면 무시하고 다음 타겟 찾기
                    }

                    float distance = (transform.position - collider.transform.position).sqrMagnitude;
                    findClosePlayer = findClosePlayer > distance ? distance : findClosePlayer;
                    if (Mathf.Approximately(findClosePlayer, distance))
                    {
                        _targetObject.Value = collider.transform.gameObject;
                    }
                }
            }
            SetDestination(Target());
        }

        // Seek the destination. Return success once the agent has reached the destination.
        // Return running if the agent hasn't reached the destination yet
        public override TaskStatus OnUpdate()
        {

            //만약 돌아가고 있는 도중에 애니메이션이 바뀌었다면 그 애니메이션을 끝까지 실행하도록 납둔 다음에 다시 무브 애니메이션 실행
            if (_controller.CurrentStateType != _controller.BaseMoveState)
            {
                _controller.ChangeAnimIfCurrentIsDone(_controller.GetCurrentOrNextAnimHash(),_controller.BaseMoveState);
            }
            
            // 타겟이 없는 경우 ex) 타겟이 다 죽은 경우
            if (_targetObject.Value == null) 
            {
                _controller.UpdateIdle();
                return TaskStatus.Failure;
            }

            //타겟이 타겟이 불가능한 상태일땐 실패를 반환해 다른 타겟을 찾도록
            if (_targetObject.Value.TryGetComponent(out ITargetable target) == true)
            {
                if (target.IsTargetable == false)
                {
                    _controller.UpdateIdle();
                    return TaskStatus.Failure;
                }
            }
            

            //_controller.UpdateMove();
            
            //타겟과 가까워졌는지와, 타겟이 시야에 들어왔는지.
            _hasArrived.Value = HasArrived() && 
                                TargetInSight.IsTargetInSight
                                    (_controller.GetComponent<IAttackRange>(), _targetObject.Value.transform, 0.2f);

            if (_hasArrived.Value)
            {
                SetDestination(transform.position);
                return TaskStatus.Success;
            }
            SetDestination(Target());
            return TaskStatus.Running;
        }

        // Return targetPosition if target is null
        private Vector3 Target()
        {
            if (_targetObject.Value != null)
            {
                return _targetObject.Value.transform.position;
            }
            return Vector3.zero;
        }

    }
}