using System;
using UnityEngine;

namespace DataType
{
    public interface ITargetStrategy
    {
        public Collider[] GetTargets(Action onComplete,LayerMask targetLayer,Material targetMaterial);
    }
    public interface ITargetOne : ITargetStrategy
    {
        public GameObject Target { get;}
    }

    public interface ITargetArea : ITargetStrategy
    {
        public Vector3 AreaTargetPosition { get; set; }
        
        
        
    }
    
}