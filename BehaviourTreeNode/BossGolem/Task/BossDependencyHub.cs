using GameManagers.Interface.ResourcesManager;
using GameManagers.ItamData.Interface;
using GameManagers.ItamDataManager.Interface;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class BossDependencyHub : MonoBehaviour
    {
        public IResourcesServices ResourcesServices { get; private set; }
        public RelayManager RelayManager { get; private set; }
        public IItemDataManager ItemDataManager { get; private set; }

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            RelayManager relayManager,
            IItemDataManager itemDataManager)
        {
            ResourcesServices = resourcesServices;
            RelayManager = relayManager;
            ItemDataManager = itemDataManager;
        }
    }
}