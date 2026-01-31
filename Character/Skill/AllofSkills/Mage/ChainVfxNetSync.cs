using System;
using DataType.Skill.Factory.Decorator.Strategy;
using GameManagers.ResourcesEx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Character.Skill.AllofSkills.Mage
{
    [RequireComponent(typeof(IGetChainVfxLineRenderer))]
    public sealed class ChainVfxNetSync : NetworkBehaviour
    {
        [SerializeField]
        private NetworkVariable<ulong> _startId =
            new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField]
        private NetworkVariable<ulong> _endId =
            new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField]
        private  NetworkVariable<Vector3> _startOffset =
            new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField]
        private NetworkVariable<Vector3> _endOffset =
            new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private IGetChainVfxLineRenderer _chainRenderer;
        
        private void Awake()
        {
            _chainRenderer = GetComponent<IGetChainVfxLineRenderer>();
        }

        public override void OnNetworkSpawn()
        {

            // 값이 나중에 들어올 수도 있으니 변경 콜백 걸어둠
            _startId.OnValueChanged += OnAnyChanged;
            _endId.OnValueChanged += OnAnyChanged;
            _startOffset.OnValueChanged += OnAnyChanged;
            _endOffset.OnValueChanged += OnAnyChanged;
            TryApply();
        }

        public override void OnNetworkDespawn()
        {
            _startId.OnValueChanged -= OnAnyChanged;
            _endId.OnValueChanged -= OnAnyChanged;
            _startOffset.OnValueChanged -= OnAnyChanged;
            _endOffset.OnValueChanged -= OnAnyChanged;
            
            _chainRenderer.Clear();
        }

        private void OnAnyChanged<T>(T prev, T cur)
        {
            TryApply();
        }


        public void InitializeChainData(
            ulong startNetworkObjectId,
            ulong endNetworkObjectId,
            Vector3 startOffset,
            Vector3 endOffset
            )
        {
            if (IsServer == false) return;

            _startId.Value = startNetworkObjectId;
            _endId.Value = endNetworkObjectId;
            _startOffset.Value = startOffset;
            _endOffset.Value = endOffset;
        }

        private void TryApply()
        {
            if (_chainRenderer == null) return;
            if (_startId.Value == 0) return;
            if (_endId.Value == 0) return;
            
            NetworkObject startNo;
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(_startId.Value, out startNo) == false)
                return;

            NetworkObject endNo;
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(_endId.Value, out endNo) == false)
                return;
            
            _chainRenderer.VFXStartObject = startNo.gameObject;
            _chainRenderer.VFXEndObject = endNo.gameObject;

            _chainRenderer.VFXStartOffSetPosition = _startOffset.Value;
            _chainRenderer.VFXEndOffsetPosition = _endOffset.Value;

        }
    }
}
