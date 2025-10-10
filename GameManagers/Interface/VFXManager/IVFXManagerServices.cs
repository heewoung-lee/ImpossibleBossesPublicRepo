using System;
using UnityEngine;

namespace GameManagers.Interface.VFXManager
{
    public interface IVFXManagerServices
    {
        public Transform VFXRoot { get; }
        public Transform VFXRootNgo { get; }
        public void InstantiateParticleToChaseTarget(string path, Transform chaseTr, float settingDuration = -1f);
        public void InstantiateParticle(string path, Vector3 spawnTr, float settingDuration = -1f,Transform parentTr = null);
        public void FollowParticleRoutine(Transform chaseTr, GameObject particleObject, string particleSourcePath,
            float settingDuration);
        public void InstanceObjConvertToParticle(GameObject particle, string path, Vector3 spawnPos = default,
            float settingDuration = -1f,
            Transform parentTr = null);
        
    }
}
