using System;
using UnityEngine;

namespace GameManagers.Interface.VFXManager
{
    public interface IVFXAction
    {
        public event Action<GameObject> VFXAction;
        public void InvokeVFXAction(GameObject targetObj);
    }
}
