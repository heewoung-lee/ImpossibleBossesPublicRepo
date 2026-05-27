using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using Stats.BaseStats;
using UnityEngine;

namespace BehaviourTreeNode.DarkWizard.Task
{
    [TaskCategory("CustomNode/DarkWizard")]
    public class CheckDarkWizardMonsterCountLimit : Conditional
    {
        [SerializeField] private int _maxMonsterCountExceptSelf = 10;
        [SerializeField] private float _scanRadius = 999f;

        public override TaskStatus OnUpdate()
        {
            int monsterLayer = LayerMask.NameToLayer("Monster");
            if (monsterLayer < 0)
            {
                return TaskStatus.Failure;
            }

            Collider[] hitColliders = Physics.OverlapSphere(
                transform.position,
                _scanRadius,
                1 << monsterLayer);

            HashSet<BaseStats> uniqueMonsters = new HashSet<BaseStats>();
            int monsterCount = 0;

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider hitCollider = hitColliders[i];
                if (hitCollider == null)
                {
                    continue;
                }

                BaseStats monsterStats = hitCollider.GetComponentInParent<BaseStats>();
                if (monsterStats == null)
                {
                    continue;
                }

                if (monsterStats.gameObject == gameObject)
                {
                    continue;
                }

                if (uniqueMonsters.Add(monsterStats) == false)
                {
                    continue;
                }

                monsterCount++;
                if (monsterCount >= _maxMonsterCountExceptSelf)
                {
                    return TaskStatus.Failure;
                }
            }

            return TaskStatus.Success;
        }
    }
}
