using GameManagers.ItemDataManagement.Interface;
using GameManagers.NGOPoolManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.VFXManagement;
using UnityEngine;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    public class BossDependencyHub : MonoBehaviour
    {
        public IResourcesServices ResourcesServices { get; private set; }
        public RelayManager RelayManager { get; private set; }
        public NgoPoolManager NgoPoolManager { get; private set; }
        public IItemDataManager ItemDataManager { get; private set; }
        public IVFXManagerServices VfxManagerServices { get; private set; }

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            RelayManager relayManager,
            NgoPoolManager ngoPoolManager,
            IItemDataManager itemDataManager,
            IVFXManagerServices vfxManagerServices)
        {
            ResourcesServices = resourcesServices;
            RelayManager = relayManager;
            NgoPoolManager = ngoPoolManager;
            ItemDataManager = itemDataManager;
            VfxManagerServices = vfxManagerServices;
        }
    }
}
