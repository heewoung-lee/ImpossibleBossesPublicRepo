using System;
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
    public class NgoArcherSkillArrowRainInitialize : NgoPoolingInitializeBase
    {
        public class NgoArcherSkillArrowRainFactory : NgoZenjectFactory<NgoArcherSkillArrowRainInitialize>,IArcherFactoryMarker
        {
            [Inject]
            public NgoArcherSkillArrowRainFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService): base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Player/VFX/Archer/Skill/ArrowRain");
            }
        }
    
        public override string PoolingNgoPath => "Prefabs/Player/VFX/Archer/Skill/ArrowRain";
        public override int PoolingCapacity => 5;

    }
        
}
