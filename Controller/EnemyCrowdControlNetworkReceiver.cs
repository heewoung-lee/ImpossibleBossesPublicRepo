using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Controller
{
    public class EnemyCrowdControlNetworkReceiver : NetworkBehaviour
    {
        private NavMeshAgent _agent;
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            Debug.Assert(_agent != null,$"NavMeshAgent is null");
        }

        [Rpc(SendTo.Server)]
        public void EnemyPushBackRpc(Vector3 dir, float distance, float duration)
        {
            StopAllCoroutines(); 
            StartCoroutine(PushRoutine(dir, distance, duration));
        }
        
        private IEnumerator PushRoutine(Vector3 dir, float totalDistance, float duration)
        {
            _agent.isStopped = true;
            _agent.ResetPath();

            float elapsedTime = 0f;
    
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
        
                float speed = totalDistance / duration;
        
                _agent.Move(dir * speed * Time.deltaTime);

                yield return null;
            }
            _agent.isStopped = false;
        }
    }
}
