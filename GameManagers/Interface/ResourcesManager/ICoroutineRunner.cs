using System.Collections;
using UnityEngine;

namespace GameManagers.Interface.ResourcesManager
{
    public interface ICoroutineRunner
    {
        public Coroutine RunCoroutine(IEnumerator enumerator);

        public void AllStopCoroutine();

        public void ManagersStopCoroutine(IEnumerator coroutineIEnumerator);
        public void ManagersStopCoroutine(Coroutine coroutine);
    }
}
