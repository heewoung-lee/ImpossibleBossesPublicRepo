    //
    //  using System.Collections;
    //  using System.Collections.Generic;
    //  using GameManagers;
    //  using GameManagers.Interface.ResourcesManager;
    //  using GameManagers.Interface.VFXManager;
    //  using Unity.Netcode;
    //  using UnityEngine;
    //  using UnityEngine.Assertions;
    //  using Util;
    //  using Zenject;
    //
    //  public class ParticleGeneratorFactory : IGenerateParticleFactory
    // {
    //     private readonly IResourcesServices _resourcesServices;
    //     private readonly RelayManager _relayManager;
    //     private readonly ICoroutineRunner _coroutineRunner;
    //     private readonly Dictionary<string, ParticleInfo> _isCheckNgoDict;
    //
    //     private Transform _vfxRoot;
    //     private Transform _vfxNgoRoot;
    //
    //     public Transform VFXRoot
    //     {
    //         get
    //         {
    //             if (_vfxRoot == null)
    //             {
    //                 _vfxRoot = new GameObject("VFXRoot").transform;
    //             }
    //             return _vfxRoot;
    //         }
    //     }
    //
    //     public Transform VFXNgoRoot
    //     {
    //         get
    //         {
    //             if (_vfxNgoRoot == null)
    //             {
    //                 _vfxNgoRoot =  _relayManager.SpawnNetworkObj("Prefabs/NGO/VFX_Root_NGO").transform;
    //             }
    //             return _vfxNgoRoot;
    //         }
    //     }
    //     
    //     
    //     [Inject]
    //     public ParticleGeneratorFactory(
    //         IResourcesServices resourcesServices,
    //         RelayManager relayManager,
    //         ICoroutineRunner coroutineRunner)
    //     {
    //         _resourcesServices = resourcesServices;
    //         _relayManager = relayManager;
    //         _coroutineRunner = coroutineRunner;
    //         _isCheckNgoDict = new Dictionary<string, ParticleInfo>();
    //     }
    //
    //     public void GenerateParticleToChaseTarget(string path, Transform chaseTr, float settingDuration = -1f) //쫒아가는 파티클을 위해 나눠놓음
    //     {
    //         Assert.IsNotNull(chaseTr,$"chaseObject is null ObjectName{chaseTr.name}");
    //         GameObject particleObject = GetParticleObject(path);
    //         Assert.IsFalse(_isCheckNgoDict.ContainsKey(path),"Particle path is null");
    //
    //         _isCheckNgoDict.TryGetValue(path, out ParticleInfo particleInfo);
    //
    //         if (particleInfo.IsCheckNetworkObject == true)
    //         {
    //             chaseTr.TryGetComponentInParents(out NetworkObject chaseTrNgo);
    //             Assert.IsNotNull(chaseTrNgo, "chaseTrNgo object is null");
    //             ulong followObjectID = chaseTrNgo.NetworkObjectId;
    //             _relayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(path, settingDuration, followObjectID);
    //         }
    //         else
    //         {
    //             particleObject = _resourcesServices.InstantiatePrefab(particleObject);
    //             ParticleObjectSetPosition(particleObject, chaseTr.position, VFXRoot);
    //             FollowParticleRoutine(chaseTr, particleObject, path, settingDuration);
    //         }
    //     }
    //
    //         
    //         
    //     _isCheckNgoDict.TryGetValue(path, out ParticleInfo particleInfo);
    //     Assert.IsNotNull(particleInfo, $"particleInfo is null");
    //
    //     if (particleInfo.IsCheckNetworkObject == true)
    //     {
    //         chaseTr.TryGetComponentInParents(out NetworkObject chaseTrNgo);
    //         Assert.IsNotNull(chaseTrNgo, "chaseTrNgo object is null");
    //         ulong followObjectID = chaseTrNgo.NetworkObjectId;
    //         _relayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(path, settingDuration, followObjectID);
    //     }
    //     else
    //     {
    //         particleObject = _resourcesServices.InstantiatePrefab(particleObject);
    //         ParticleObjectSetPosition(particleObject, chaseTr.position, VFXRoot);
    //         FollowParticleRoutine(chaseTr, particleObject, path, settingDuration);
    //     }
    //     public void GenerateParticle(string path, Vector3 spawnPos = default, float settingDuration = -1f,Transform parentTr = null)
    //     {
    //         GameObject particleObject = GetParticleObject(path);
    //         Assert.IsNotNull(_isCheckNgoDict.ContainsKey(path),"Particle path is null");
    //
    //         _isCheckNgoDict.TryGetValue(path, out ParticleInfo particleInfo);
    //         Assert.IsNotNull(particleInfo, $"particleInfo is null");
    //
    //         if (particleInfo.IsCheckNetworkObject)
    //         {
    //             _relayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(path, settingDuration, spawnPos);
    //         }
    //         else
    //         {
    //             Transform parent = parentTr == null ? VFXRoot : parentTr;
    //             particleObject = _resourcesServices.InstantiatePrefab(particleObject);
    //             ParticleObjectSetPosition(particleObject, spawnPos, parent);
    //             ParticleRoutine(particleObject, path, settingDuration);
    //         }
    //         
    //     }
    //
    //     public void FollowParticleRoutine(Transform chaseTr,GameObject particleObject,string particleSourcePath,float settingDuration)
    //     {
    //         _coroutineRunner.RunCoroutine(FollowingGenerator(chaseTr, particleObject));
    //         ParticleRoutine(particleObject, particleSourcePath, settingDuration);
    //     }
    //
    //     private void ParticleRoutine(GameObject particleObject,string particleSourcePath,float settingDuration)
    //     {
    //         SetParticlePosAndLifeCycle(particleObject, particleSourcePath, settingDuration);
    //         if (particleObject.TryGetComponent(out IVFXAction vfxAction) == true)
    //         {
    //             vfxAction.InvokeVFXAction(particleObject);
    //         }
    //     }
    //     
    //
    //     private GameObject GetParticleObject(string path)
    //     {
    //         GameObject particleObj = _resourcesServices.Load<GameObject>(path);
    //         //처음에 딕셔너리에 키가 없으면 키등록
    //         if (_isCheckNgoDict.ContainsKey(path) == false)
    //         {
    //             ParticleInfo particleInfo = new ParticleInfo()
    //             {
    //                 IsCheckNetworkObject = false,
    //                 IsLooping = false
    //             };
    //             
    //             particleInfo.IsCheckNetworkObject = particleObj.TryGetComponent(out NetworkObject ngo) == true;
    //             particleInfo.IsLooping = particleObj.TryGetComponent(out LoopingParticle loopingParticle) == true;
    //             
    //             _isCheckNgoDict.Add(path, particleInfo);
    //         }
    //
    //         return particleObj;
    //     }
    //
    //     public GameObject SetParticlePosAndLifeCycle(GameObject particleObject, string path, float settingDuration)
    //     {
    //         if (_isCheckNgoDict.TryGetValue(path, out ParticleInfo info))
    //         {
    //             if (info.IsLooping == true)
    //                 return particleObject;
    //         }
    //
    //         SetAndRunParticle(particleObject, settingDuration, out float maxDurationTime);
    //         _resourcesServices.DestroyObject(particleObject, maxDurationTime);
    //         return particleObject;
    //     }
    //
    //     public IEnumerator FollowingGenerator(Transform targetTr, GameObject particle)
    //     {
    //         while (particle != null && particle.activeSelf == true)
    //         {
    //             particle.transform.position = new Vector3(targetTr.position.x, particle.transform.position.y,
    //                 targetTr.position.z);
    //             yield return targetTr;
    //         }
    //     }
    //
    //
    //     private void SetAndRunParticle(GameObject particleObject, float settingDuration,
    //         out float maxDurationTime)
    //     {
    //         maxDurationTime = 0f;
    //         ParticleSystem[] particles = particleObject.GetComponentsInChildren<ParticleSystem>();
    //         foreach (ParticleSystem particle in particles)
    //         {
    //             particle.Stop();
    //             particle.Clear();
    //             float duration = 0f;
    //             ParticleSystem.MainModule main = particle.main;
    //
    //             duration = settingDuration <= 0 ? main.duration : settingDuration;
    //             main.duration = duration;
    //             if (particle.GetComponent<ParticleLifetimeSync>()) //파티클 시스템중 Duration과 시간을 맞춰야 하는 파티클이 있다면 적용
    //             {
    //                 main.startLifetime = duration;
    //             }
    //             else if (duration < particle.main.startLifetime.constantMax) //Duration보다 파티클 생존시간이 큰 경우 파티클 생존시간을 넣는다.
    //             {
    //                 maxDurationTime = particle.main.startLifetime.constantMax;
    //             }
    //             else if (maxDurationTime < duration + particle.main.startLifetime.constantMax &&
    //                      particle.GetComponent<ParticleLifetimeSync>() == null)
    //             {
    //                 maxDurationTime = duration + particle.main.startLifetime.constantMax;
    //             }
    //
    //             particle.Play();
    //         }
    //     }
    //
    //     private void ParticleObjectSetPosition(GameObject particleObject, Vector3 generatePos, Transform parentTr)
    //     {
    //         particleObject.SetActive(false);
    //         particleObject.transform.position = generatePos;
    //         particleObject.transform.SetParent(parentTr);
    //         particleObject.SetActive(true);
    //     }
    // }
    //