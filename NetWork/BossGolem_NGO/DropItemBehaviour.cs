using System.Collections;
using Data.DataType.ItemType.Interface;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using NetWork.Item;
using NetWork.NGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.BossGolem_NGO
{
    public class DropItemBehaviour : NetworkBehaviour, ILootItemBehaviour
    {
        public class DropItemBehaviourFactory : NgoZenjectFactory<DropItemBehaviour>
        {
            public DropItemBehaviourFactory(
                DiContainer container,
                IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory,
                IResourcesServices loadService) : base(
                container,
                factoryManager,
                handlerFactory,
                loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGO_BossDropItemBehaviour");
            }
        }

        private RelayManager _relayManager;
        private LootItem _lootItem;

        private readonly float _maxHeight = 3f;
        private readonly float _circleRange = 30f;
        private readonly float _itemFlightDuration = 1.5f;

        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        private void Awake()
        {
            _lootItem = GetComponent<LootItem>();
        }

        public void SpawnBehaviour(Rigidbody rigid)
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
            Collider lootCollider = rb.GetComponent<Collider>();
            int groundLayer = LayerMask.NameToLayer("Ground");
            int groundMask = 1 << groundLayer;

            Vector3 startPos = tr.position;
            Vector2 rndCircle = Random.insideUnitCircle * _circleRange;
            Vector3 targetPos = startPos + new Vector3(rndCircle.x, 0, rndCircle.y);

            Vector3 spinAxis = Random.onUnitSphere.normalized;
            float spinSpeed = Random.Range(180f, 540f);

            float t = 0f;
            while (t < 1f)
            {
                Vector3 previousPos = tr.position;
                t += Time.deltaTime / duration;

                Vector3 flat = Vector3.Lerp(startPos, targetPos, t);
                float y = Mathf.Lerp(startPos.y, targetPos.y, t) +
                          _maxHeight * Mathf.Sin(Mathf.PI * t);
                Vector3 nextPos = new Vector3(flat.x, y, flat.z);

                float halfHeight = lootCollider != null ? lootCollider.bounds.extents.y : 0f;
                Vector3 sweepStart = previousPos - Vector3.up * halfHeight;
                Vector3 sweepEnd = nextPos - Vector3.up * halfHeight;

                if (_lootItem != null &&
                    Physics.Linecast(sweepStart, sweepEnd, out RaycastHit hit, groundMask))
                {
                    tr.position = hit.point + Vector3.up * halfHeight;
                    _lootItem.TryResolveLandingFromTrajectory(groundLayer);
                    yield break;
                }

                tr.position = nextPos;
                tr.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);

                yield return null;
            }

            rb.isKinematic = false;
        }
    }
}
