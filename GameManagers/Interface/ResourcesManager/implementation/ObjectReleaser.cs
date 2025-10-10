using System.Collections;
using Scene;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameManagers.Interface.ResourcesManager.implementation
{
    public class ObjectReleaser : IDestroyObject
    {
        private readonly ICoroutineRunner _coroutineRunner;

        [Inject]
        public ObjectReleaser(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }
        

        public void DestroyObject(GameObject go,float duration)
        {
            if (go == null)
            {
                Debug.Log("Destroy object is null");
                return;
            }
            
            if (go.TryGetComponent(out Poolable poolobj) == true)
            {
                _coroutineRunner.RunCoroutine(DelayedActionCoroutine(() => { poolobj.Push();}, duration));
            }
            else
            {
                _coroutineRunner.RunCoroutine(DelayedActionCoroutine(() => { Object.Destroy(go);}, duration));
            }
            
        }

        IEnumerator DelayedActionCoroutine(System.Action actionMethod, float duration)
        {
            if (duration <= 0)
            {
                actionMethod.Invoke();
            }
            else
            {
                yield return new WaitForSeconds(duration);
                actionMethod.Invoke();
            }
        }
    }
}