using System;
using UnityEngine;

namespace GameManagers.ResourcesExManagement
{
    public interface IRegisteredFactoryObject
    {
        public GameObject RequestObject { get; }
        public Func<Transform,GameObject> Creator { get; }
        
    }
}
