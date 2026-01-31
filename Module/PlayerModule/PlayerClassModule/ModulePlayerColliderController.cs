using System;
using System.Collections;
using Stats.BaseStats;
using Unity.Netcode;
using UnityEngine;
using Util;

namespace Module.PlayerModule.PlayerClassModule
{
    public class ModulePlayerColliderController : NetworkBehaviour
    {
        Collider[] _aliveColliders = new Collider[2];
        private SphereCollider _headCollider;
        private CapsuleCollider _bodyCollider;

        private BoxCollider _deadCollider;
        private BaseStats _baseStats;

        private void Awake()
        {
            _headCollider = GetComponent<SphereCollider>();
            _bodyCollider = GetComponent<CapsuleCollider>();
            _aliveColliders = new Collider[] { _headCollider, _bodyCollider };

            _deadCollider = GetComponent<BoxCollider>();
            _baseStats = GetComponent<BaseStats>();

        }


        private void OnEnable()
        {
            _baseStats.IsDeadValueChagneEvent += SwitchCollider;
        }

        private void OnDisable()
        {
            _baseStats.IsDeadValueChagneEvent -= SwitchCollider;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SetCollider(_baseStats.IsDead);  
        }

        private void SwitchCollider(bool oldValue, bool newValue)
        {
            SetCollider(newValue);  
        }

        private void SetCollider(bool isDead)
        {
            foreach (Collider colliderComponent in _aliveColliders)
            {
                colliderComponent.enabled = !isDead;
            }
            _deadCollider.enabled = isDead;
        }

   
        
    }
}