using System.Collections;
using Scene.CommonInstaller.Interfaces;
using UnityEngine;

namespace GameManagers.Interface.ResourcesManager.implementation
{
    public class CoroutineRunner : MonoBehaviour,ICoroutineRunner
    {
        public Coroutine RunCoroutine(IEnumerator enumerator)
        {
          return StartCoroutine(enumerator);
        }
        public void AllStopCoroutine()
        {
            StopAllCoroutines();
        }
        public void ManagersStopCoroutine(IEnumerator coroutineIEnumerator)
        {
            StopCoroutine(coroutineIEnumerator);
        }
        public void ManagersStopCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

    }
}
