using System;
using UnityEngine;

namespace GameManagers.Interface.ResourcesManager
{
    public interface IRegisteredFactoryObject
    {
        public GameObject RequestObject { get; }
        public Func<Transform,GameObject> Creator { get; }
        
    }
}
