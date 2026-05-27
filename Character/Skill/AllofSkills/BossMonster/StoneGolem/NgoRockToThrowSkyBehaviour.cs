using GameManagers.ResourcesExManagement;
using GameManagers.RelayManagement;
using NetWork.BaseNGO;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Character.Skill.AllofSkills.BossMonster.StoneGolem
{
    public class NgoRockToThrowSkyBehaviour : NetworkBehaviour
    {
        private const float MinFlightDuration = 0.1f;

        [SerializeField] private float _gravity = 12f;
        [SerializeField] private float _minSpinSpeed = 180f;
        [SerializeField] private float _maxSpinSpeed = 540f;

        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private NgoPoolingInitializeBase _poolingInitializeBase;
        private bool _isConfigured;
        private float _flightDuration;
        private double _startServerTime;
        private Vector3 _startPosition;
        private Vector3 _initialVelocity;
        private Vector3 _angularVelocity;
        private Quaternion _startRotation;

        [Inject]
        public void Construct(IResourcesServices resourcesServices, RelayManager relayManager)
        {
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
        }

        private void Awake()
        {
            _poolingInitializeBase = GetComponent<NgoPoolingInitializeBase>();
            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent += ResetThrowState;
            }
        }

        public override void OnDestroy()
        {
            if (_poolingInitializeBase != null)
            {
                _poolingInitializeBase.PoolObjectReleaseEvent -= ResetThrowState;
            }

            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetThrowState();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetThrowState();
        }

        private void Update()
        {
            if (_isConfigured == false || _relayManager == null)
            {
                return;
            }

            float elapsedTime = (float)(_relayManager.NetworkManagerEx.ServerTime.Time - _startServerTime);
            if (elapsedTime >= _flightDuration)
            {
                if (IsServer)
                {
                    ReleaseThrowObject();
                }

                return;
            }

            Vector3 gravityVector = Vector3.down * _gravity;
            transform.position = _startPosition +
                                 (_initialVelocity * elapsedTime) +
                                 (0.5f * gravityVector * elapsedTime * elapsedTime);
            transform.rotation = Quaternion.Euler(_angularVelocity * elapsedTime) * _startRotation;
        }

        public void ConfigureThrowOnHost(float duration, Vector3 targetPosition)
        {
            if (IsHost == false)
            {
                return;
            }

            float flightDuration = Mathf.Max(duration, MinFlightDuration);
            Vector3 startPosition = transform.position;
            Vector3 gravityVector = Vector3.down * _gravity;
            Vector3 initialVelocity =
                (targetPosition - startPosition - (0.5f * gravityVector * flightDuration * flightDuration)) /
                flightDuration;
            Vector3 angularVelocity = Random.onUnitSphere * Random.Range(_minSpinSpeed, _maxSpinSpeed);

            StartThrowRpc(
                startPosition,
                initialVelocity,
                angularVelocity,
                flightDuration,
                _relayManager.NetworkManagerEx.ServerTime.Time);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StartThrowRpc(
            Vector3 startPosition,
            Vector3 initialVelocity,
            Vector3 angularVelocity,
            float flightDuration,
            double startServerTime)
        {
            _startPosition = startPosition;
            _initialVelocity = initialVelocity;
            _angularVelocity = angularVelocity;
            _flightDuration = Mathf.Max(flightDuration, MinFlightDuration);
            _startServerTime = startServerTime;
            _startRotation = transform.rotation;
            _isConfigured = true;

            transform.position = startPosition;
        }

        private void ReleaseThrowObject()
        {
            _isConfigured = false;
            _resourcesServices.DestroyObject(gameObject);
        }

        private void ResetThrowState()
        {
            _isConfigured = false;
            _flightDuration = 0f;
            _startServerTime = 0d;
            _startPosition = Vector3.zero;
            _initialVelocity = Vector3.zero;
            _angularVelocity = Vector3.zero;
            _startRotation = Quaternion.identity;
        }
    }
}
