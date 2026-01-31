using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Archer;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.Acher
{
    public class NgoArcherSkillFlashInitialize : NgoPoolingInitializeBase
    {
        private IResourcesServices _resourcesServices;
        [Inject]
        public void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        
        public class NgoArcherSkillFlashFactory : NgoZenjectFactory<NgoArcherSkillFlashInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public NgoArcherSkillFlashFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Archer/Skill/Flash");
            }
        }
    
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/Flash";
        public override int PoolingCapacity => 5;

        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);

            MoveToCallerRoutine(targetGo,duration).Forget();
        }
        
        private async UniTaskVoid MoveToCallerRoutine(GameObject caller, float duration)
        {
            CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
        
            Vector3 callerCenterPos = caller.transform.position + (Vector3.up * 0.5f);
            Vector3 startPos = new Vector3(transform.position.x, callerCenterPos.y, transform.position.z);
            transform.position = startPos;
        
            float elapsedTime = 0f;
        
            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancellationRequested) return;
        
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration; 
                transform.position = Vector3.Lerp(transform.position, caller.transform.position + (Vector3.up * 0.5f), t);
                
                await UniTask.NextFrame(cancellationToken);
            }
        }
    }
        
}
