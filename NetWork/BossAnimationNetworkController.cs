using System.Collections.Generic;
using Controller;
using Controller.BossState.BossGolem;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using UI.Scene.SceneUI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.ProjectContextInstaller;
using IState = Controller.ControllerStats.IState;

namespace NetWork
{
    public class BossAnimationNetworkController : NetworkBehaviour
    {
        private const float DefaultAnimationSpeed = 1f;

        private IUIManagerServices _uiManager;
        private IResourcesServices _resourcesServices;
        private RelayManager _relayManager;
        private SignalBus _signalBus;
        private bool _hasFiredReadySignal;
        private bool _hasCurrentAnimationSpeedOverride;
        private float _currentAnimationSpeed = DefaultAnimationSpeed;

        [Inject]
        public void Construct(
            IUIManagerServices uiManager,
            IResourcesServices resourcesServices,
            RelayManager relayManager,
            SignalBus signalBus)
        {
            _uiManager = uiManager;
            _resourcesServices = resourcesServices;
            _relayManager = relayManager;
            _signalBus = signalBus;
        }

        BossController _bossController;
        private Dictionary<string, IState> _bossStateDict = new Dictionary<string, IState>();
        public Dictionary<string, IState> BossStateDict => _bossStateDict;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _bossController = GetComponent<BossController>();

            foreach (IState istate in _bossController.StateAnimDict.StateDict.Keys)
            {
                string istateName = istate.GetType().Name;
                _bossStateDict.Add(istateName, istate);
            }

            if (IsClient && !IsServer)
            {
                RequestSyncAnimationRpc();
                return;
            }

            FireBossAnimationReadySignal();
        }

        public void SyncAnimationState(string stateName)
        {
            if (_relayManager.NetworkManagerEx.IsHost == false)
            {
                return;
            }

            CacheAnimationSpeedOverride(false, DefaultAnimationSpeed);
            SetAnimationStateRpc(stateName, false, DefaultAnimationSpeed);
        }

        public void SyncCurrentAnimationSpeedOverride(float animationSpeed)
        {
            if (_relayManager.NetworkManagerEx.IsHost == false || _bossController.CurrentStateType == null)
            {
                return;
            }

            string currentStateName = _bossController.CurrentStateType.GetType().Name;
            CacheAnimationSpeedOverride(true, animationSpeed);
            SetAnimationStateRpc(currentStateName, true, animationSpeed);
        }

        [Rpc(SendTo.Server)]
        public void RequestSyncAnimationRpc(RpcParams rpcParams = default)
        {
            if (_bossController.CurrentStateType == null)
                return;

            string currentStateName = _bossController.CurrentStateType.GetType().Name;

            ReplySyncAnimationRpc(
                currentStateName,
                _hasCurrentAnimationSpeedOverride,
                _currentAnimationSpeed,
                RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void ReplySyncAnimationRpc(
            FixedString512Bytes stateName,
            bool hasSpeedOverride,
            float animationSpeed,
            RpcParams rpcParams = default)
        {
            CacheAnimationSpeedOverride(hasSpeedOverride, animationSpeed);
            ChangeState(stateName, hasSpeedOverride, animationSpeed);
            FireBossAnimationReadySignal();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetAnimationStateRpc(FixedString512Bytes stateName, bool hasSpeedOverride, float animationSpeed)
        {
            CacheAnimationSpeedOverride(hasSpeedOverride, animationSpeed);

            if (_relayManager.NetworkManagerEx.IsHost)
                return;

            ChangeState(stateName, hasSpeedOverride, animationSpeed);
        }

        private void ChangeState(FixedString512Bytes stateName, bool hasSpeedOverride, float animationSpeed)
        {
            string stateNameString = stateName.ToString();

            if (_bossController.CurrentStateType != null &&
                _bossController.CurrentStateType.GetType().Name == stateNameString)
            {
                _bossController.Anim.speed = hasSpeedOverride ? animationSpeed : DefaultAnimationSpeed;
                return;
            }

            if (_bossStateDict.TryGetValue(stateNameString, out IState syncState))
            {
                _bossController.CurrentStateType = syncState;
                _bossController.Anim.speed = hasSpeedOverride ? animationSpeed : DefaultAnimationSpeed;
            }
            else
            {
                UtilDebug.LogError("Don't know how to change state Check your Dictionary");
            }
        }

        private void CacheAnimationSpeedOverride(bool hasSpeedOverride, float animationSpeed)
        {
            _hasCurrentAnimationSpeedOverride = hasSpeedOverride;
            _currentAnimationSpeed = hasSpeedOverride ? animationSpeed : DefaultAnimationSpeed;
        }

        private void FireBossAnimationReadySignal()
        {
            if (_hasFiredReadySignal)
            {
                return;
            }

            _hasFiredReadySignal = true;
            _signalBus.Fire(new BossAnimationNetworkReadySignal
            {
                BossMonster = gameObject,
                AnimationController = this
            });
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void RemoveBossHpBarRpc()
        {
            if (_uiManager.Try_Get_Scene_UI(out UIBossHp bossHp))
            {
                _resourcesServices.DestroyObject(bossHp.gameObject);
            }
        }
    }
}
