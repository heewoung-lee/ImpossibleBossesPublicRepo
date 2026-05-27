using System.Collections.Generic;
using UnityEngine;

namespace Character.Skill.AllofSkills.Mage
{
    public class MeteorImpactDetect : MonoBehaviour
    {
        
        private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
        private NgoMageSkillMeteorInitialize _meteorInitialize; 

        // 2026-05-22 수정:
        // 네트워크 VFX는 호스트와 클라이언트에서 모두 파티클 충돌 콜백을 받을 수 있어,
        // 클라이언트 복제본까지 데미지 RPC를 보내면 같은 메테오가 여러 번 피격될 수 있었다.
        // 서버에서만 첫 충돌을 처리하고, 풀링 재사용 시 OnEnable에서 다시 초기화한다.
        private bool _hasProcessedImpact;

        private void Awake()
        {
            _meteorInitialize = GetComponentInParent<NgoMageSkillMeteorInitialize>();
        }

        private void OnEnable()
        {
            _hasProcessedImpact = false;
        }

        private void OnParticleCollision(GameObject other)
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            int count = ps.GetCollisionEvents(other, _collisionEvents);
        
            if (count > 0)
            {
                // 클라이언트 복제본과 이미 처리한 충돌은 데미지 판정에서 제외한다.
                if (_meteorInitialize.IsServer == false || _hasProcessedImpact)
                {
                    return;
                }

                _hasProcessedImpact = true;
                Vector3 pos = _collisionEvents[0].intersection;
                _meteorInitialize.RequestMeteorImpactCameraShake();

                Collider[] cols = Physics.OverlapSphere(pos, 5, LayerMask.GetMask("Monster"));
                foreach (Collider col in cols)
                {
                    _meteorInitialize.HitMeteorImpact(col);
                }
            
            }
        }
    }
}
