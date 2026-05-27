using GameManagers.LoginManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.UIManagement;
using NetWork.BaseNGO;
using UI.Scene.SceneUI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Util;
using Zenject;
using ZenjectContext.GameObjectContext;


namespace NetWork.NGO
{
    public class CharacterSelectorNgo : NetworkBehaviourBase
    {
        public class CharacterSelectorNgoFactory : NgoZenjectFactory<CharacterSelectorNgo>
        {
            public CharacterSelectorNgoFactory(DiContainer container, IFactoryManager factoryManager,
                NgoZenjectHandler.NgoZenjectHandlerFactory handlerFactory, IResourcesServices loadService) : base(
                container, factoryManager, handlerFactory, loadService)
            {
                _requestGO = loadService.Load<GameObject>("Prefabs/NGO/NGOUICharacterSelectRect");
            }
        }

        private IPlayerIngameLogininfo _playerIngameLogininfo;
        private IUIManagerServices _uiManagerServices;
        private RelayManager _relayManager;
        private UICharacterSelectRect _uiCharacterSelectRect;

        [Inject]
        public void Construct(IPlayerIngameLogininfo playerIngameLogininfo,
            IUIManagerServices uiManagerServices,
            RelayManager relayManager)
        {
            _playerIngameLogininfo = playerIngameLogininfo;
            _uiManagerServices = uiManagerServices;
            _relayManager = relayManager;
        }

        private readonly Color _playerFrameColor = "#143658".HexCodetoConvertColor();

        private NetworkVariable<FixedString64Bytes> _playerNickname = new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes(""), NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server); // 서버만 수정 가능하도록 설정

        private NetworkVariable<bool> _isReady = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<Vector3> _characterSeletorCamera = new NetworkVariable<Vector3>(
            Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> _playerIndex = new NetworkVariable<int>(
            -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private NetworkVariable<int> _choiceChracter = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        
        public event NetworkVariable<FixedString64Bytes>.OnValueChangedDelegate OnPlayerNicknameChanged
        {
            add { _playerNickname.OnValueChanged += value; }
            remove { _playerNickname.OnValueChanged -= value; }
        }

        public event NetworkVariable<bool>.OnValueChangedDelegate OnIsReadyChanged
        {
            add { _isReady.OnValueChanged += value; }
            remove { _isReady.OnValueChanged -= value; }
        }

        public event NetworkVariable<Vector3>.OnValueChangedDelegate OnCharacterSelectorCameraChanged
        {
            add { _characterSeletorCamera.OnValueChanged += value; }
            remove { _characterSeletorCamera.OnValueChanged -= value; }
        }

        public event NetworkVariable<int>.OnValueChangedDelegate OnPlayerIndexChanged
        {
            add { _playerIndex.OnValueChanged += value; }
            remove { _playerIndex.OnValueChanged -= value; }
        }

        public event NetworkVariable<int>.OnValueChangedDelegate OnChoiceChracterChanged
        {
            add { _choiceChracter.OnValueChanged += value; }
            remove { _choiceChracter.OnValueChanged -= value; }
        }
        
        public string PlayerNickname => _playerNickname.Value.Value;

        public Vector3 CharacterSeletorCamera => _characterSeletorCamera.Value;
        public bool IsReady => _isReady.Value;
        
        public int ChoiceCharacter => _choiceChracter.Value;

        public bool IsHostPlayer => IsOwnedByServer;

        public int PlayerIndex
        {
            get { return _playerIndex.Value; }
            // 서버 전용 값 쓰기 함수로만 사용. UI 로직은 여기서 호출하지 않음.
            set { _playerIndex.Value = value; } 
        }
        


        private PlayerIngameLoginInfo PlayerIngameLoginInfo => _playerIngameLogininfo.GetPlayerIngameLoginInfo();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            //이쪽에 생긴 애랑 바인드해야함.
            AllocatedSlot(_playerIndex.Value);
            
            _characterSeletorCamera.OnValueChanged += OnCameraPositionChanged;
            _isReady.OnValueChanged += OnReadyStateChanged;
        }
        
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
    
            //네트워크 변수 이벤트 해제
            _characterSeletorCamera.OnValueChanged -= OnCameraPositionChanged;
            _isReady.OnValueChanged -= OnReadyStateChanged;
    
            //연결된 UI 슬롯 및 카메라 초기화 지시
            if (_uiCharacterSelectRect != null)
            {
                _uiCharacterSelectRect.ResetAndDisableSlot();
                _uiCharacterSelectRect = null;
            }
        }
        
        
        private void OnCameraPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            if (_uiCharacterSelectRect != null)
            {
                _uiCharacterSelectRect.SelecterCameraOnValueChanged(previousValue, newValue);
            }
        }

        private void AllocatedSlot(int idx)
        {
            if (idx == -1) //슬롯에 대한 동기화 처리가 아직 안되었다면.
            {
                _playerIndex.OnValueChanged += (oldValue, newValue) =>
                {
                    FindSlot(newValue);
                };//동기화 값이 바뀌면 할당
            }
            else // 아직 동기화가 안되었다면,
            {
                FindSlot(idx);
            }
            void FindSlot(int playerIdx)
            {
                UIRoomCharacterSelect uiRoomCharacterSelect =  _uiManagerServices.Get_Scene_UI<UIRoomCharacterSelect>();
                _uiCharacterSelectRect = uiRoomCharacterSelect.GetPlayerFrameTransform(playerIdx).GetComponentInChildren<UICharacterSelectRect>(true);
                _uiCharacterSelectRect.gameObject.SetActive(true);
                _uiCharacterSelectRect.SetCharacterSelect(this);//연결
                OwnerInitialize(_uiCharacterSelectRect,uiRoomCharacterSelect);
            }
        }


        private void OwnerInitialize(UICharacterSelectRect uiCharacterSelectRect, UIRoomCharacterSelect uiRoomCharacterSelect)
        {
            if (IsOwner)
            {
                uiCharacterSelectRect.PreviousButton.gameObject.SetActive(true);
                uiCharacterSelectRect.NextButton.gameObject.SetActive(true);
                uiRoomCharacterSelect.ButtonState(false);
                SetNicknameServerRpc(PlayerIngameLoginInfo.PlayerNickName);
                
                uiRoomCharacterSelect.SetButtonEvent(() => PlayerReadyServerRpc());
                uiRoomCharacterSelect.SetCharacterSelectorNgo(this);
                
                if (IsHost) 
                {
                    uiRoomCharacterSelect.SetHostButton(); // 레디 버튼을 스타트 버튼으로 교체
                    uiRoomCharacterSelect.SetHostStartButton(true); // 방장 혼자 켜졌을 때 즉시 활성화
                }
            }
            
        }
                    
   
        
        protected override void StartInit()
        {
        }
        
        protected override void AwakeInit()
        {
        }
        [Rpc(SendTo.Server)]
        public void SetCameraPositionServerRpc(Vector3 position,int currentIndex,RpcParams rpcParams = default)
        {
            _characterSeletorCamera.Value += position;
            _choiceChracter.Value = currentIndex;
        }
        [Rpc(SendTo.Server)]
        private void SetNicknameServerRpc(string newNickname, RpcParams rpcParams = default)
        {
            _playerNickname.Value = new FixedString64Bytes(newNickname);
        }
        [Rpc(SendTo.Server)]
        public void PlayerReadyServerRpc(RpcParams rpcParams = default)
        {
            _isReady.Value = !_isReady.Value;
            _uiManagerServices.Get_Scene_UI<UIRoomCharacterSelect>().IsCheckAllReadyToPlayers();
        }
        private void OnReadyStateChanged(bool previousValue, bool newValue)
        {
            //공통 처리: 모든 플레이어의 화면에서 이 캐릭터 슬롯의 레디 패널을 켜거나 끔
            if (_uiCharacterSelectRect != null)
            {
                _uiCharacterSelectRect.ReadyPanel.SetActive(newValue);
            }

            //주인 전용 처리 (버튼을 누른 본인 화면에서만 작동)
            if (IsOwner)
            {
                //우측 하단 메인 레디 버튼의 이미지 및 텍스트 상태 변경 (이전 ClientRpc 역할)
                UIRoomCharacterSelect uiRoomCharacterSelect = _uiManagerServices.Get_Scene_UI<UIRoomCharacterSelect>();
                uiRoomCharacterSelect.ButtonState(newValue);

                //슬롯의 좌우 이동 화살표 숨기기/보이기
                if (_uiCharacterSelectRect != null)
                {
                    _uiCharacterSelectRect.PreviousButton.gameObject.SetActive(!newValue);
                    _uiCharacterSelectRect.NextButton.gameObject.SetActive(!newValue);
                }
                if (newValue)
                {
                    Define.PlayerClass selectCharacter = (Define.PlayerClass)ChoiceCharacter; 
                    _relayManager.RegisterSelectedCharacter(_relayManager.NetworkManagerEx.LocalClientId, selectCharacter);
                }
            }
        }

        
    }
}
