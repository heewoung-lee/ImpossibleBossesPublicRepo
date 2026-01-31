using System;
using NetWork;
using UnityEngine;

namespace GameManagers.Interface.VFXManager
{
    public interface IVFXManagerServices
    {
        public Transform VFXRoot { get; }
        public Transform VFXRootNgo { get; }
        public void InstantiateParticleWithTarget(string path, Transform chaseTr ,float settingDuration = -1f,bool isUnique = false,Vector3 localScale = default);
        public void InstantiateParticleWithTarget(string path, Transform chaseTr, Quaternion rotation ,float settingDuration = -1f,bool isUnique = false,Vector3 localScale = default);
        public void InstantiateParticleInArea(string path, Vector3 spawnTr, float settingDuration = -1f,Transform parentTr = null,Vector3 localScale = default,NetworkParams networkParams = default);
        public void SetParticlePosAndLifeCycle(GameObject particleObject, string path, float settingDuration);
        public void FollowParticleRoutine(Transform chaseTr, GameObject particleObject);
        public void InstanceObjConvertToParticle(GameObject particle, string path, Vector3 spawnPos = default,
            float settingDuration = -1f,
            Transform parentTr = null,Vector3 localScale = default);
        public float GetParticleLifeCycle(string path);
    }
}
