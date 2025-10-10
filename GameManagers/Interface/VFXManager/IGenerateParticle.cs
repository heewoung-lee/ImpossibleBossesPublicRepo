using System;
using UnityEngine;

namespace GameManagers.Interface.VFXManager
{
    public interface IGenerateParticle
    {
        public Transform VFXRoot { get; }
        public Transform VFXNgoRoot { get; }
        public void InstantiateParticleToChaseTarget(string path, Transform spawnTr, float settingDuration = -1f);
        public void InstantiateParticle(string path, Vector3 spawnTr, float settingDuration = -1f,Transform parentTr = null);
        public void FollowParticleRoutine(Transform chaseTr, GameObject particleObject, string particleSourcePath,
            float settingDuration);

/// <summary>
/// 생성된 오브젝트를 파티클로 변경해주는 메서드
/// </summary>
/// <param name="instance">인스턴스 오브젝트</param>
/// <param name="path">인스턴스 오브젝트의 생성경로</param>
/// <param name="spawnPos">스폰위치</param>
/// <param name="settingDuration">지속시간</param>
/// <param name="parentTr">설정하고 싶은 트랜스폼</param>
        public void ObjConvertToParticle(GameObject instance, string path, Vector3 spawnPos = default,
            float settingDuration = -1f,
            Transform parentTr = null);
        
        
        
    }
}
