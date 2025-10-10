using GameManagers;
using GameManagers.Interface.ItemDataManager;
using GameManagers.Interface.ResourcesManager;
using UnityEngine;
using Zenject;

namespace BehaviourTreeNode.BossGolem.Task
{
    
    [DisallowMultipleComponent]
    public sealed class BossDependencyHub : MonoBehaviour
    {
        [Inject] public IResourcesServices ResourcesServices { get; }
        [Inject] public RelayManager RelayManager { get; }
        [Inject] public IItemGetter ItemGetter { get; }
        [Inject] public SceneManagerEx SceneManagerEx { get; }
    }
}
