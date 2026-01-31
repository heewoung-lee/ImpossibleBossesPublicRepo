using Character.Skill.AllofSkills.Mage;
using GameManagers.ResourcesEx;
using Module.PlayerModule.PlayerClassModule.Mage;
using Module.PlayerModule.PlayerClassModule.Monk;
using NetWork.BaseNGO;
using NetWork.NGO;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Character.Skill.AllofSkills.MonkSkillScripts
{
    public class NgoMonkSkillHolyShieldInitialize : NgoPoolingInitializeBase
    {
        public class NgoMonkSkillHolyShieldFactory : NgoZenjectFactory<NgoMonkSkillHolyShieldInitialize>,
            IMonkFactoryMarker
        {
            [Inject]
            public NgoMonkSkillHolyShieldFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>( "Prefabs/Player/VFX/MonkSkillPrefab/HolyShield");
            }
        }


        public override void StartParticleOption(GameObject targetGo, float duration)
        {
            base.StartParticleOption(targetGo, duration);
            transform.position = targetGo.transform.position + Vector3.up;
            _vfxManager.FollowParticleRoutine(targetGo.transform,gameObject);
        }

        public override string PoolingNgoPath => "Prefabs/Player/VFX/MonkSkillPrefab/HolyShield";
        public override int PoolingCapacity => 5;
    }
}
