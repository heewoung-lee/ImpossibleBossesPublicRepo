using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers.Interface.VFXManager;
using GameManagers.ResourcesEx;
using NetWork;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using Util;
using Zenject;

namespace GameManagers
{
    public class ParticleInfo
    {
        public bool IsNetworkObject;
        public bool IsLooping;
        public GameObject Prefab;
        public float Duration;
    }

    public class VFXManager : IVFXManagerServices
    {
        private readonly IResourcesServices _resourcesServices;
        private readonly RelayManager.RelayManager _relayManager;
        private readonly Dictionary<string, ParticleInfo> _isCheckNgoDict;

        private Transform _vfxRoot;
        private Transform _vfxNgoRoot;

        public Transform VFXRoot
        {
            get
            {
                if (_vfxRoot == null)
                {
                    _vfxRoot = new GameObject("VFXRoot").transform;
                }

                return _vfxRoot;
            }
        }

        public Transform VFXRootNgo
        {
            get
            {
                if (_vfxNgoRoot == null)
                {
                    _vfxNgoRoot = _relayManager.SpawnNetworkObj("Prefabs/NGO/VFXRootNGO").transform;
                }

                return _vfxNgoRoot;
            }
        }


        [Inject]
        public VFXManager(
            IResourcesServices resourcesServices,
            RelayManager.RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _isCheckNgoDict = new Dictionary<string, ParticleInfo>();
        }
        
        public void InstantiateParticleWithTarget(string path, Transform chaseTr,float settingDuration = -1f,bool isUnique = false,Vector3 localScale = default) //호출 한 호출자를 위해 남겨놓음
        {
            InstantiateParticleWithTarget(path,chaseTr,Quaternion.identity,settingDuration,isUnique,localScale);
        }

        public void InstantiateParticleWithTarget(string path,Transform chaseTr, Quaternion rotation, float settingDuration = -1,bool isUnique = false,Vector3 localScale = default)
        {
            Assert.IsNotNull(chaseTr, $"chaseObject is null ObjectName{chaseTr.name}");
            ParticleInfo particleInfo = GetParticleInfo(path);

            if (particleInfo.IsNetworkObject == true)
            {
                chaseTr.TryGetComponentInParents(out NetworkObject chaseTrNgo);
                Assert.IsNotNull(chaseTrNgo, "chaseTrNgo object is null");
                ulong followObjectID = chaseTrNgo.NetworkObjectId;
                _relayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(path, settingDuration,isUnique,followObjectID,rotation,localScale);
                
            }
            else
            {
                GameObject particleObject = _resourcesServices.InstantiatePrefab(particleInfo.Prefab);
                ParticleObjectSetPosition(particleObject, chaseTr.position, VFXRoot,rotation,localScale);
                FollowParticleRoutine(chaseTr, particleObject);
            }
            
        }

        public void InstantiateParticleInArea(string path, Vector3 spawnPos = default, float settingDuration = -1f,
            Transform parentTr = null,Vector3 localScale = default,NetworkParams networkParams = default)
        {

            CommonParticleGenerator(path, spawnPos, settingDuration,parentTr,InstantiateObj,localScale,networkParams);
            GameObject InstantiateObj() =>  _resourcesServices.InstantiateByKey(path);
        }

        private void CommonParticleGenerator(string path, Vector3 spawnPos = default, float settingDuration = -1f,
            Transform parentTr = null,Func<GameObject> acquireParticle = null,Vector3 localScale = default,NetworkParams networkParams = default)
        {
            ParticleInfo particleInfo  = GetParticleInfo(path);
            float duration = settingDuration;

            if (duration < 0)
            {
                duration = particleInfo.Duration;
            }
            
            if (particleInfo.IsNetworkObject)
            {
                _relayManager.NgoRPCCaller.SpawnVFXPrefabServerRpc(path, duration, spawnPos,Quaternion.identity,localScale,networkParams);
            }
            else
            {
                Transform parent = parentTr == null ? VFXRoot : parentTr;
                
                Assert.IsNotNull(acquireParticle, "generateParticle Func delicate is null");
                GameObject particleObject = acquireParticle.Invoke();
                //여기때문에 MoveMarker에 풀이 안들어감.
                ParticleObjectSetPosition(particleObject, spawnPos, parent,Quaternion.identity,localScale);
                SetParticlePosAndLifeCycle(particleObject, path, duration);
            }
        }
        
        

        public void FollowParticleRoutine(Transform chaseTr,GameObject particleObject)
        {
            FollowingGenerator(chaseTr, particleObject,particleObject.GetCancellationTokenOnDestroy()).Forget();
            //SetParticlePosAndLifeCycle(particleObject, particleSourcePath, settingDuration);
            //1.5일 수정 기존에는 FollowParticleRoutine메서드가 파티클을 타겟에 따라다니게 하면서 시간이 다되면 자동으로
            //없애주는 역할도 했는데 역할이 비대해져서 로직을 수정함.
        }

        public void InstanceObjConvertToParticle(GameObject particle, string path, Vector3 spawnPos = default,
            float settingDuration = -1, Transform parentTr = null, Vector3 localScale = default)
        {
            CommonParticleGenerator(path, spawnPos, settingDuration,parentTr,GetInstanceObj,localScale);
            GameObject GetInstanceObj() => particle;
        }

        private ParticleInfo GetParticleInfo(string path)
        {
            if (_isCheckNgoDict.ContainsKey(path) == false)
            {
                GameObject particleObj = _resourcesServices.Load<GameObject>(path);
                ParticleInfo particleInfo = new ParticleInfo()
                {
                    IsNetworkObject = false,
                    IsLooping = false,
                    Prefab = particleObj,
                    Duration = -1f
                };
                
                particleInfo.IsNetworkObject = particleObj.TryGetComponent(out NetworkObject ngo) == true;
                particleInfo.IsLooping = particleObj.GetComponent<ParticleSystem>().main.loop;
                _isCheckNgoDict.Add(path, particleInfo);
                if (particleObj.TryGetComponent(out ParticleSystem particleSystem))
                {
                    if (particleSystem.main.duration > particleInfo.Duration)
                    {
                        particleInfo.Duration = particleSystem.main.duration;
                    }
                }
            }
            return  _isCheckNgoDict[path];
        }

        
        public void SetParticlePosAndLifeCycle(GameObject particleObject, string path, float settingDuration)
        {
            if (_isCheckNgoDict.TryGetValue(path, out ParticleInfo info))
            {
                if (info.IsLooping == true)
                {
                    return;
                }
            }
            SetAndRunParticle(particleObject, settingDuration, out float maxDurationTime);
            _resourcesServices.DestroyObject(particleObject, maxDurationTime);
            //이부분 주의 만약 파티클의 기본 생명력을 유지시키려면 duration을 키울것
        }

        public async UniTaskVoid FollowingGenerator(Transform targetTr, GameObject particle,CancellationToken token)
        {
            while (particle != null && particle.activeSelf == true)
            {
                Debug.Assert(targetTr != null, "targetTr is null");
                
                particle.transform.position = new Vector3(targetTr.position.x, particle.transform.position.y,
                    targetTr.position.z);

                await UniTask.NextFrame(token);
            }
        }


        public float GetParticleLifeCycle(string path)
        {
            GameObject prefab = _resourcesServices.Load<GameObject>(path);
            ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            if (systems == null || systems.Length == 0) return 0.01f;

            float max = 0f;

            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                var main = ps.main;

                if (main.loop) return float.PositiveInfinity;

                float startDelay = GetMaxLength(main.startDelay);
                float duration   = main.duration;

                float total = startDelay + duration;
                float speed = main.simulationSpeed;
                if (speed <= 0f) speed = 1f;
                total /= speed;

                if (total > max) max = total;
            }

            if (max < 0.01f) max = 0.01f;
            return max;
        }
        

        private float GetMaxLength(ParticleSystem.MinMaxCurve curve)
        {
            // 상한만 뽑으면 된다(대부분 Destroy 타이밍은 max 기준이 안전)
            ParticleSystemCurveMode mode = curve.mode;
            if (mode == ParticleSystemCurveMode.Constant) return curve.constant;
            if (mode == ParticleSystemCurveMode.TwoConstants) return curve.constantMax;

            return curve.constantMax; // (Unity 버전에 따라 constantMax 유효)
        }

        
        private void SetAndRunParticle(GameObject particleObject, float settingDuration,
            out float maxDurationTime)
        {
            maxDurationTime = 0f;
            ParticleSystem[] particles = particleObject.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                
                particle.Stop();
                particle.Clear();
                float duration = 0f;
                ParticleSystem.MainModule main = particle.main;

                duration = settingDuration <= 0 ? main.duration : settingDuration;
                main.duration = duration;
                if (particle.GetComponent<ParticleLifetimeSync>()) //파티클 시스템중 Duration과 시간을 맞춰야 하는 파티클이 있다면 적용
                {
                    main.startLifetime = duration;
                }
                else if (duration < particle.main.startLifetime.constantMax) //Duration보다 파티클 생존시간이 큰 경우 파티클 생존시간을 넣는다.
                {
                    maxDurationTime = particle.main.startLifetime.constantMax;
                }
                else if (maxDurationTime < duration + particle.main.startLifetime.constantMax &&
                         particle.GetComponent<ParticleLifetimeSync>() == null)
                {
                    maxDurationTime = duration + particle.main.startLifetime.constantMax;
                }

                particle.Play();
            }
        }

        private void ParticleObjectSetPosition(GameObject particleObject, Vector3 generatePos, Transform parentTr,Quaternion rotation,Vector3 localScale)
        {
            if(localScale.Equals(default))
                localScale = Vector3.one;
            
            particleObject.SetActive(false);
           particleObject.transform.position = generatePos;
           particleObject.transform.rotation = rotation;
           particleObject.transform.localScale = localScale;
           particleObject.transform.SetParent(parentTr);
            particleObject.SetActive(true);
        }
    }
}