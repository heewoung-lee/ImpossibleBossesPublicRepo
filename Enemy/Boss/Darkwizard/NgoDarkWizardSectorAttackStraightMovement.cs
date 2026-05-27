using System.Collections;
using GameManagers.RelayManagement;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Enemy.Boss.Darkwizard
{
    public class NgoDarkWizardSectorAttackStraightMovement : NetworkBehaviour
    {
        [SerializeField] private float _moveSpeed = 12f;
        [SerializeField] private float _fixedHeight = 1f;
        [SerializeField] private float _catchUpDuration = 0.2f;
        [SerializeField] private float _maxCatchUpMultiplier = 3f;

        private RelayManager _relayManager;
        private Coroutine _moveCoroutine;
        private bool _isInitialized;
        private bool _isCatchUpInitialized;
        private float _remainingCatchUpDistance;
        private float _serverStartTime;

        [Inject]
        public void Construct(RelayManager relayManager)
        {
            _relayManager = relayManager;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Vector3 spawnPosition = transform.position;
            spawnPosition.y = _fixedHeight;
            transform.position = spawnPosition;
            _isInitialized = false;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _serverStartTime = 0f;

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveRoutine());
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }

        public void Initialize(float serverStartTime)
        {
            _serverStartTime = serverStartTime;
            _isCatchUpInitialized = false;
            _remainingCatchUpDistance = 0f;
            _isInitialized = true;
        }

        private IEnumerator MoveRoutine()
        {
            while (true)
            {
                if (_isInitialized == false)
                {
                    yield return null;
                    continue;
                }

                Vector3 currentPosition = transform.position;
                if (Mathf.Approximately(currentPosition.y, _fixedHeight) == false)
                {
                    currentPosition.y = _fixedHeight;
                    transform.position = currentPosition;
                }

                float baseDistance = _moveSpeed * Time.deltaTime;
                float extraDistance = GetExtraCatchUpDistance(Time.deltaTime);
                transform.Translate(Vector3.forward * (baseDistance + extraDistance), Space.Self);
                yield return null;
            }
        }

        private float GetExtraCatchUpDistance(float deltaTime)
        {
            if (_isCatchUpInitialized == false)
            {
                float elapsedTime = Mathf.Max(0f, (float)_relayManager.NetworkManagerEx.ServerTime.Time - _serverStartTime);
                _remainingCatchUpDistance = elapsedTime * _moveSpeed;
                _isCatchUpInitialized = true;
            }

            if (_remainingCatchUpDistance <= 0f)
            {
                return 0f;
            }

            float effectiveCatchUpDuration = Mathf.Max(_catchUpDuration, 0.01f);
            float catchUpSpeed = Mathf.Min(
                _remainingCatchUpDistance / effectiveCatchUpDuration,
                _moveSpeed * (_maxCatchUpMultiplier - 1f));

            float extraDistance = catchUpSpeed * deltaTime;
            _remainingCatchUpDistance = Mathf.Max(0f, _remainingCatchUpDistance - extraDistance);
            return extraDistance;
        }
    }
}
