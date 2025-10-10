using System;
using System.Collections;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.VFXManager;
using UnityEngine;
using Zenject;
using ZenjectContext.ProjectContextInstaller;

namespace GameManagers
{
    public struct ParticleInfo
    {
        public bool IsCheckNetworkObject;
        public bool IsLooping;
        public GameObject Prefab;
    }

    public class VFXManager : IVFXManagerServices
    {
        private readonly IGenerateParticle _generateParticle;
        private readonly Transform _vfxRoot;
        private readonly Transform _vfxNgoRoot;
        
        public VFXManager(
            IGenerateParticle generateParticle)
        {
            _generateParticle = generateParticle;
        }

        public Transform VFXRoot => _generateParticle.VFXRoot;
        public Transform VFXRootNgo => _generateParticle.VFXNgoRoot;

        public void InstantiateParticleToChaseTarget(string path, Transform chaseTr, float settingDuration = -1)
        {
            _generateParticle.InstantiateParticleToChaseTarget(path, chaseTr, settingDuration);
        }

        public void InstantiateParticle(string path, Vector3 spawnTr, float settingDuration = -1,Transform parentTr = null)
        {
            _generateParticle.InstantiateParticle(path, spawnTr, settingDuration,parentTr);
        }
        public void FollowParticleRoutine(Transform chaseTr, GameObject particleObject, string particleSourcePath,
            float settingDuration)
        {
            _generateParticle.FollowParticleRoutine(chaseTr, particleObject, particleSourcePath, settingDuration);
        }

        public void InstanceObjConvertToParticle(GameObject particle, string path, Vector3 spawnPos = default, float settingDuration = -1,
            Transform parentTr = null)
        {
            _generateParticle.ObjConvertToParticle(particle, path, spawnPos, settingDuration);
        }
    }
}