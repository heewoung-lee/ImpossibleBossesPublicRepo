using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NetWork.NGO;
using Scene.CommonInstaller;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace GameManagers.ResourcesEx.implementation
{
    public class ObjectReleaser : IDestroyObject, IRegistrar<INetworkDeSpawner>, IDisposable
    {
        private INetworkDeSpawner _deSpawner;
        
        private readonly Dictionary<int, CancellationTokenSource> _pendingCancels;

        [Inject]
        public ObjectReleaser()
        {
            _pendingCancels = new Dictionary<int, CancellationTokenSource>();
        }

        public void DestroyObject(GameObject go, float duration)
        {
            if (go == null)
            {
                Debug.LogWarning("Destroy object is null");
                return;
            }

            int id = go.GetInstanceID();

            if (_pendingCancels.TryGetValue(id, out var existingCts))
            {
                existingCts.Cancel(); 
                existingCts.Dispose();
                _pendingCancels.Remove(id);
            }//만약 돌고 있는데 또 같은놈이 온다면 기존에 돌고있는애가 들고 있는
            //토큰 파괴 목록에서도 제거 즉 초기상태로 다시 돌리고 이후 파괴로직을 다시 돌리기 위함.
            
            // 즉시 파괴
            if (duration <= 0)
            {
                ProcessDestroyLogic(go);
                return;
            }

            var newCts = new CancellationTokenSource();
            _pendingCancels[id] = newCts;
            
            DestroyRoutine(go, duration, newCts, id).Forget();
        }

        private async UniTaskVoid DestroyRoutine(GameObject go, float duration, CancellationTokenSource cts, int id)
        {
            try
            {
                // 지정된 시간만큼 대기
                // cancellationToken을 넣어주면 cts.Cancel() 호출 시 즉시 멈추고 catch로 넘어감
                // 중요: 토큰 꼭 넣어야함. 기존에 풀에서 돌아가는 같은객체가 있다면
                // 그 객체가 돌아가는 타이머를 해제 시키고 다시 돌려야함.
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cts.Token);

                // 생명주기 체크
                // 코루틴과 달리 UniTask는 객체가 파괴되어도 계속 돌기 때문에 반드시 null 체크를 해야 함.
                if (go == null) return;

                // 정상적으로 시간이 다 돼서 실행되는 경우 딕셔너리에서 제거
                if (_pendingCancels.ContainsKey(id) && _pendingCancels[id] == cts)
                {
                    _pendingCancels.Remove(id);
                }

                ProcessDestroyLogic(go);
            }
            catch (OperationCanceledException)
            {
                // cts.Cancel()이 호출되면 이쪽으로 옴 (풀에 다시 들어가거나 해서 취소된 경우)
                // 아무것도 안 하고 종료하면 됨
            }
            finally
            {
                // 정상 종료되든 취소되든 토큰은 반드시 Dispose 해야 메모리 누수가 없음
                cts.Dispose();
            }
        }

        private void ProcessDestroyLogic(GameObject go)
        {
            if (go == null) return;

            if (go.TryGetComponent(out Poolable poolobj))
            {
                poolobj.Push();
            }
            else if (go.TryGetComponent(out NetworkObject ngo))
            {
                if (ngo != null && ngo.IsSpawned)
                {
                    _deSpawner?.DeSpawnByReferenceServerRpc(new NetworkObjectReference(ngo));
                }
                else
                {
                    Object.Destroy(go);
                }
            }
            else
            {
                Object.Destroy(go);
            }
        }

        public void Register(INetworkDeSpawner targetManager)
        {
            _deSpawner = targetManager;
        }

        public void Unregister(INetworkDeSpawner rpcCaller)
        {
            _deSpawner = null;
        }

        public void Dispose()
        {
            foreach (var cts in _pendingCancels.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _pendingCancels.Clear();
        }
    }
}