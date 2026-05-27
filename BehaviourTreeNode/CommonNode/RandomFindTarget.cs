using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Controller;
using Stats.BaseStats;
using UnityEngine;
using Util;

namespace BehaviourTreeNode.CommonNode
{
    [TaskDescription("Finds a random valid player and sets it as the target.")]
    [TaskCategory("CustomNode")]
    public class RandomFindTarget : Action
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

            GameObject randomTarget = null;

            int validCount = 0;
          
            // hitCount만큼만 순회하여 이전 프레임의 쓰레기 데이터 접근 방지
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _targetObjects[i];
                if (col == null) continue;

                // 사망 상태 체크
                if (col.TryGetComponent(out BaseStats baseStats) && baseStats.IsDead)
                    continue;

                // 타겟팅 불가 상태 (무적, 은신 등) 체크
                if (col.TryGetComponent(out ITargetable targetable) && targetable.IsTargetable == false)
                    continue;

                validCount++;

                //reservoir sampling 알고리즘
                //처음에 사람이 들어오면 100%이지만 두번째 사람이 들어오면 두번쨰 사람이 들어올 확률 50%
                //처음에 사람이 나갈 확률이 50% 이기에 동일한 확률(매우신기)
                if (Random.Range(0, validCount) == 0)
                {
                    randomTarget = col.gameObject;
                }
            }
            
            // 타겟 갱신 및 결과 반환
            if (randomTarget != null)
            {
                _targetObject.Value = randomTarget;
                return TaskStatus.Success;
            }

            // 살아있는 유효한 타겟이 없다면 변수를 비우고 실패 반환
            _targetObject.Value = null;
            return TaskStatus.Failure;
        }
    }
}