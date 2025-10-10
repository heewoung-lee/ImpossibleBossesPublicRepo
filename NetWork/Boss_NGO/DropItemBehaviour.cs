using System.Collections;
using BehaviourTreeNode.BossGolem.Task;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.Boss_NGO
{
    public class DropItemBehaviour : NetworkBehaviour,ILootItemBehaviour
    {
        public class DropItemBehaviourFactory : NgoZenjectFactory<DropItemBehaviour>
        {
            public DropItemBehaviourFactory(DiContainer container, IFactoryRegister registerableFactory,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, registerableFactory, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_BossDropItemBehaviour");
            }
        }
        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }
        
        private RelayManager _relayManager;
        
        
        private readonly float _maxHeight = 3f;
        private readonly float _circleRange = 30f;
        private readonly float _itemFlightDuration = 1.5f;
        public void SpawnBahaviour(Rigidbody rigid)
        {
            rigid.isKinematic = true;
            if (_relayManager.NetworkManagerEx.IsHost)
            {
                StartCoroutine(ThrowStoneParabola(rigid, _itemFlightDuration));
            }
        }
        public IEnumerator ThrowStoneParabola(Rigidbody rb, float duration)
        {
            Transform tr = rb.transform;

            Vector3 startPos = tr.position;                      // 시작점
            Vector2 rndCircle = Random.insideUnitCircle * _circleRange;
            Vector3 targetPos = startPos + new Vector3(rndCircle.x, 0, rndCircle.y);

            Vector3 spinAxis = Random.onUnitSphere.normalized;   // 임의 축
            float spinSpeed = Random.Range(180f, 540f);         // °/sec

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;

                /* ---------------- 위치(포물선) ---------------- */
                Vector3 flat = Vector3.Lerp(startPos, targetPos, t);
                float y = Mathf.Lerp(startPos.y, targetPos.y, t) +
                          _maxHeight * Mathf.Sin(Mathf.PI * t);
                tr.position = new Vector3(flat.x, y, flat.z);

                /* ---------------- 회전(랜덤 축 스핀) ---------------- */
                tr.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);

                yield return null;
            }

            // 충돌·회전을 물리에 맡기고 싶다면 다시 동적 모드로
            rb.isKinematic = false;
        }

    }
}