using GameManagers.VFXManagement;
using NetWork;
using NetWork.NGO.Interface;
using UnityEngine;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.RedDragon
{
    public class RedDragonTailAttackInitialize : MonoBehaviour, ISpawnBehavior
    {
        private IVFXManagerServices _vfxManager;
        private Vector3 _defaultLocalScale;

        private void Awake()
        {
            _defaultLocalScale = transform.localScale;
        }

        [Inject]
        public void Construct(IVFXManagerServices vfxManager)
        {
            _vfxManager = vfxManager;
        }

        public void SpawnObjectToLocal(in NetworkParams param, string path = null)
        {
            _vfxManager.InstanceObjConvertToParticle(
                gameObject,
                path,
                param.ArgPosVector3,
                param.ArgFloat,
                localScale: _defaultLocalScale);
        }
    }
}
