using UnityEngine;
using Zenject;

namespace GameManagers.PoolManagement
{
    public class LocalPoolable : Poolable
    {
        private LocalPoolManager _localPoolManager;

        [Inject]
        public void Construct(LocalPoolManager localPoolManager)
        {
            _localPoolManager = localPoolManager;
        }

        public override GameObject Pop()
        {
            return _localPoolManager.Pop(gameObject).gameObject; 
        }

        public override void Push()
        {
            _localPoolManager.Push(this);
        }
    }
}