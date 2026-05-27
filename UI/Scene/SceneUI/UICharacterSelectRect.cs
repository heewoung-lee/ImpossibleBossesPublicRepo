using System.Collections;
using Module.CameraModule;
using Module.UI_Module;
using NetWork.NGO;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.Scene.SceneUI
{
    public class UICharacterSelectRect : UIBase
    {
        private readonly Color _playerFrameColor = "#143658".HexCodetoConvertColor();

        enum RawImages
        {
            SelectPlayerRawImage
        }

        enum Cameras
        {
            SelectCamara
        }

        enum Images
        {
            Bg,
            HostImage
        }

        enum GameObjects
        {
            NickNamePanel,
            ReadyPanel,
        }

        enum Buttons
        {
            PreviousPlayerBtn,
            NextPlayerBtn,
        }


        private Image _bg;
        private Image _hostImage;
        private Button _previousButton;
        private Button _nextButton;
        private GameObject _playerNickNameObject;
        private GameObject _readyPanel;

        private TMP_Text _playerNickNameText;
        private UIRoomCharacterSelect _uiRoomCharacterSelect;
        private Camera _playerChooseCamera;
        private RawImage _selectPlayerRawImage;
        private bool _isRunnningCoroutine = false;
        private Coroutine _cameraMoveCoroutine;
        private bool _isInitCameraPosition = false;
        private ModuleChooseCharacterMove _moduleChooseCharacterMove;

        private CharacterSelectorNgo _characterSelectorNgo;

        public GameObject ReadyPanel => _readyPanel;


        protected override void StartInit()
        {
        }

        public Button PreviousButton => _previousButton;
        public Button NextButton => _nextButton;

        protected override void AwakeInit()
        {
            //Debug.Log("여기 호출됨");
            
            Bind<Image>(typeof(Images));
            Bind<Button>(typeof(Buttons));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Camera>(typeof(Cameras));
            Bind<RawImage>(typeof(RawImages));

            _bg = Get<Image>((int)Images.Bg);
            _hostImage = Get<Image>((int)Images.HostImage);
            _previousButton = Get<Button>((int)Buttons.PreviousPlayerBtn);
            _nextButton = Get<Button>((int)Buttons.NextPlayerBtn);
            _playerNickNameObject = Get<GameObject>((int)GameObjects.NickNamePanel);
            _readyPanel = Get<GameObject>((int)GameObjects.ReadyPanel);
            _playerChooseCamera = Get<Camera>((int)Cameras.SelectCamara);
            _selectPlayerRawImage = Get<RawImage>((int)RawImages.SelectPlayerRawImage);
            
            
            _hostImage.gameObject.SetActive(false);
            SetActiveCharacterSelectionArrow(false);
            _readyPanel.gameObject.SetActive(false);

            _playerNickNameText = _playerNickNameObject.GetComponentInChildren<TMP_Text>();
            _uiRoomCharacterSelect = GetComponentInParent<UIRoomCharacterSelect>();
            _moduleChooseCharacterMove = GetComponent<ModuleChooseCharacterMove>();
        }


        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            //여기에 초기화 함수 들어가야함 카메라 위치 등
            _characterSelectorNgo = null;
        }

        public void SetCharacterSelect(CharacterSelectorNgo selectorNgo)
        {
            _bg.color = _playerFrameColor;
            _characterSelectorNgo = selectorNgo;
            SetPlayerNickName(selectorNgo);
            DisPlayHostMarker(selectorNgo);
            SetPlayerReadySync(selectorNgo);
            SetPlayerChoiceCamera(selectorNgo);
        }


        private void SetPlayerChoiceCamera(CharacterSelectorNgo selectorNgo)
        {
            _playerChooseCamera = _uiRoomCharacterSelect.ChooseCameraController.AllocatedCamera(selectorNgo.PlayerIndex);
            
            var rtCreator = _playerChooseCamera.GetComponent<ModuleRenderTextureCreator>();
            if (rtCreator != null && _selectPlayerRawImage != null)
            {
                _selectPlayerRawImage.texture = rtCreator.RenderTexture;
            }//렌더 텍스쳐 할당

            _playerChooseCamera.gameObject.SetActive(true);
            ConnectPlayerSlot(selectorNgo);
            PlayerRenderSync(selectorNgo);
        }

        private void PlayerRenderSync(CharacterSelectorNgo selectorNgo)
        {
            if (selectorNgo.ChoiceCharacter != 0)
            {
                _playerChooseCamera.transform.localPosition = selectorNgo.CharacterSeletorCamera;
            }
            
        }
        
        private void DisPlayHostMarker(CharacterSelectorNgo selectorNgo)
        {
            _hostImage.gameObject.SetActive(selectorNgo.IsHostPlayer);
        }

        private void SetPlayerNickName(CharacterSelectorNgo selectorNgo)
        {
            if (string.IsNullOrEmpty(selectorNgo.PlayerNickname))
            {
                selectorNgo.OnPlayerNicknameChanged -= OnChangePlayerNickName;
                selectorNgo.OnPlayerNicknameChanged += OnChangePlayerNickName;
            }
            else
            {
                _playerNickNameText.text = selectorNgo.PlayerNickname;
            }
        }

        private void SetPlayerReadySync(CharacterSelectorNgo selectorNgo)
        {
            if (selectorNgo.IsReady)
            {
                _readyPanel.SetActive(true);
            }
        }

        private void OnChangePlayerNickName(FixedString64Bytes oldValue, FixedString64Bytes newValue)
        {
            _playerNickNameText.text = newValue.ToString();
        }


        private void SetActiveCharacterSelectionArrow(bool state)
        {
            _previousButton.gameObject.SetActive(state);
            _nextButton.gameObject.SetActive(state);
        }

        IEnumerator MoveCameraLinear(Vector3 moveDirection)
        {
            float elapseTime = 0f;
            float durationTime = 1f;
            _isRunnningCoroutine = true;
            while (elapseTime < durationTime)
            {
                elapseTime += Time.deltaTime;
                _playerChooseCamera.transform.localPosition = Vector3.Lerp(_playerChooseCamera.transform.localPosition,
                    moveDirection, elapseTime);
                yield return null;
            }

            _isRunnningCoroutine = false;
            _playerChooseCamera.transform.localPosition = moveDirection;
        }

       
        public void ConnectPlayerSlot(CharacterSelectorNgo playerinfo)
        {
            if (playerinfo.IsOwner == false) 
                return;

            _moduleChooseCharacterMove.OnCameraMoveRequested -= RequestCameraMove;
            _moduleChooseCharacterMove.OnCameraMoveRequested += RequestCameraMove;
        }
        private void RequestCameraMove(int direction, int moveValue)
        {
            Vector3 moveVector = direction * Vector3.right * moveValue;
            _characterSelectorNgo.SetCameraPositionServerRpc(moveVector,_moduleChooseCharacterMove.PlayerChooseIndex);
        }
        
        
        public void SelecterCameraOnValueChanged(Vector3 oldValue, Vector3 newValue)
        {
            if (_isInitCameraPosition == false)
            {
                _playerChooseCamera.transform.localPosition = newValue;
                _isInitCameraPosition = true;
                return;
            }

            if (_characterSelectorNgo.IsOwner)
            {
                if (_isRunnningCoroutine == true)
                {
                    StopCoroutine(_cameraMoveCoroutine);
                    _playerChooseCamera.transform.localPosition = oldValue;
                }

                _cameraMoveCoroutine = StartCoroutine(MoveCameraLinear(newValue));
            }
            else
            {
                _playerChooseCamera.transform.localPosition = newValue;
            }
        }
        
        public void ResetAndDisableSlot()
        {
            //이벤트 구독 해제
            if (_characterSelectorNgo != null)
            {
                _characterSelectorNgo.OnPlayerNicknameChanged -= OnChangePlayerNickName;
            }
    
            //코루틴 정지 및 변수 초기화
            if (_isRunnningCoroutine && _cameraMoveCoroutine != null)
            {
                StopCoroutine(_cameraMoveCoroutine);
                _isRunnningCoroutine = false;
            }
            _isInitCameraPosition = false;

            //UI 텍스트 및 상태 초기화
            _playerNickNameText.text = "";
            _hostImage.gameObject.SetActive(false);
            _readyPanel.SetActive(false);
            SetActiveCharacterSelectionArrow(false);

            //RawImage 렌더 텍스처 해제
            if (_selectPlayerRawImage != null)
            {
                _selectPlayerRawImage.texture = null;
            }

            // 할당된 카메라 반환 및 초기화
            if (_characterSelectorNgo != null)
            {
                _uiRoomCharacterSelect.ChooseCameraController.ReleaseCamera(_characterSelectorNgo.PlayerIndex);
            }

            //연결 객체 해제 및 슬롯 비활성화
            _characterSelectorNgo = null;
            gameObject.SetActive(false);
        }
        
    }
}
