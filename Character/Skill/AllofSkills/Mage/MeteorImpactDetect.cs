using System.Collections.Generic;
using UnityEngine;

namespace Character.Skill.AllofSkills.Mage
{
    public class MeteorImpactDetect : MonoBehaviour
    {
        
        private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
        private NgoMageSkillMeteorInitialize _meteorInitialize; 

        private void Awake()
        {
            _meteorInitialize = GetComponentInParent<NgoMageSkillMeteorInitialize>();
        }

        private void OnParticleCollision(GameObject other)
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            int count = ps.GetCollisionEvents(other, _collisionEvents);
        
            if (count > 0)
            {
                Vector3 pos = _collisionEvents[0].intersection;
                Collider[] cols = Physics.OverlapSphere(pos, 5, LayerMask.GetMask("Monster"));
                foreach (Collider col in cols)
                {
                    _meteorInitialize.HitMeteorImpact(col);
                }
            
            }
        }
    }
}
