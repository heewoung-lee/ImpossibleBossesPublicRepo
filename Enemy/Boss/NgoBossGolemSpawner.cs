using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace Enemy.Boss
{
    public class NgoBossGolemSpawner : NetworkBehaviour
    {
        public class BossGolemFactory : NgoZenjectFactory<NgoBossGolemSpawner>
        {
            public BossGolemFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/Enemy/Boss/Character/StoneGolem");
            }
        }
    }
}