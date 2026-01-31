using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIManager;
using GameManagers.ResourcesEx;
using GameManagers.Scene;
using NetWork.NGO.UI;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;

namespace NetWork.NGO.Scene_NGO
{
    public readonly struct TimeValue
    {
        public float VillageStayTime { get; }
        public float BossRoomStayTime { get; }
        public float AllPlayerInPortalStayTime { get; }

        public TimeValue(float villageStayTime, float bossRoomStayTime, float allPlayerInPortalCount)
        {
            VillageStayTime = villageStayTime;
            BossRoomStayTime = bossRoomStayTime;
            AllPlayerInPortalStayTime = allPlayerInPortalCount;
        }
    }


    public class NgoStageTimerController : NetworkBehaviour
    {
        public class NgoStageTimerControllerFactory : NgoZenjectFactory<NgoStageTimerController>
        {
            public NgoStageTimerControllerFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/Scene_NGO/NgoStageTimerController");
            }
        }


        [Inject]
        public void Construct(SceneManagerEx sceneManagerEx, IUIManagerServices uiManagerServices, TimeValue timeValue)
        {
            _sceneManagerEx = sceneManagerEx;
            _uiManagerServices = uiManagerServices;
            _timeValue = timeValue;
        }

        private SceneManagerEx _sceneManagerEx;
        private IUIManagerServices _uiManagerServices;
        private TimeValue _timeValue;

        private Color _normalClockColor = "FF9300".HexCodetoConvertColor();
        private Color _allPlayerInPortalColor = "0084FF".HexCodetoConvertColor();


        private float _totalTime = 0;
        private float _currentTime = 0;

        private UIStageTimer _uiStageTimer;

        public UIStageTimer UIStageTimer
        {
            get
            {
                if (_uiStageTimer == null)
                {
                    _uiStageTimer = _uiManagerServices.GetOrCreateSceneUI<UIStageTimer>();
                }

                return _uiStageTimer;
            }
        }

        public float TotalTime
        {
            get
            {
                if (Mathf.Approximately(_totalTime, default))
                {
                    Define.Scene currentScene = _sceneManagerEx.CurrentScene;
                    _totalTime = currentScene == Define.Scene.GamePlayScene
                        ? _timeValue.VillageStayTime
                        : _timeValue.BossRoomStayTime;
                }

                return _totalTime;
            }
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SetHostTimer();
            if (IsHost == false) //클라이언트가 서버에 도는 시간을 가져와야 한다.
            {
                RequestTimeFromServerRpc();
            }
        }

        private void SetHostTimer()
        {
            if (IsHost == false)
                return;

            UIStageTimer.SetTimer(TotalTime, _normalClockColor);
        }


        [Rpc(SendTo.Server)]
        private void RequestTimeFromServerRpc(RpcParams rpcParams = default)
        {
            float currentCount = _uiManagerServices.Get_Scene_UI<UIStageTimer>().CurrentTime;
            ulong clientId = rpcParams.Receive.SenderClientId;

            SendTimeRpcToSpecificClientRpc(currentCount, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SendTimeRpcToSpecificClientRpc(float currentCount, RpcParams rpcParams = default)
        {
            _currentTime = currentCount;
            UIStageTimer.SetTimer(TotalTime, _currentTime);
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void SetPortalInAllPlayersCountRpc()
        {
            _currentTime = UIStageTimer.CurrentTime;
            UIStageTimer.SetTimer(_timeValue.AllPlayerInPortalStayTime, _allPlayerInPortalColor);
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void SetNormalCountRpc()
        {
            UIStageTimer.SetTimer(TotalTime, _currentTime, _normalClockColor);
        }
    }
}