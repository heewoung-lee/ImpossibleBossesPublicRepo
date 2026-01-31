using System;
using System.Threading;
using Character.Skill.AllofSkills.Acher;
using Cysharp.Threading.Tasks;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Mage;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Mage
{
    public class NgoMageSkillTeleportInitialize : NgoPoolingInitializeBase
    {
        public class NgoMageSkillTeleportFactory : NgoZenjectFactory<NgoMageSkillTeleportInitialize>,
            IMageFactoryMarker
        {
            [Inject]
            public NgoMageSkillTeleportFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/Mage/Skill/Teleport");
            }
        }


        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            MoveToCallerRoutine(targetGo,duration).Forget();
        }
        
        private async UniTaskVoid MoveToCallerRoutine(GameObject caller, float duration)
        {
            CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
        
            Vector3 callerCenterPos = caller.transform.position + (Vector3.up * 1f);
            Vector3 startPos = new Vector3(transform.position.x, callerCenterPos.y, transform.position.z);
            transform.position = startPos;
        
            float elapsedTime = 0f;
        
            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancellationRequested) return;
        
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration; 
                transform.position = Vector3.Lerp(transform.position, caller.transform.position + (Vector3.up * 1f), t);
                
                await UniTask.NextFrame(cancellationToken);
            }
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/Mage/Skill/Teleport";
        public override int PoolingCapacity => 5;
    }
}