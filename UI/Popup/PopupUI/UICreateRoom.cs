using System;
using Cysharp.Threading.Tasks;
using GameManagers;
using GameManagers.LobbyManagement;
using GameManagers.SceneManagement;
using GameManagers.UIManagement;
using TMPro;
using UI.Scene.SceneUI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;
using Input = UnityEngine.Input;

namespace UI.Popup.PopupUI
{
    public class UICreateRoom : IDPwPopup, IUIHasCloseButton
    {
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private SceneManagerEx _sceneManagerEx;
        [Inject] private IUIManagerServices _uiManagerServices;
        
        enum InputFields
        {
            RoomNameInputField,
            RoomPwInputField
        }

        enum Buttons
        {
            ButtonClose,
            ButtonConnect
        }

        enum Sliders { UserCountSlider }
        enum Texts { CurrentCount }
        enum Toggles { RoomAvailableToggle }


        private TMP_InputField _roomNameInputField;
        private TMP_InputField _roomPwInputField;
        private Toggle _roomAvailableToggle;

        private Button _buttonClose;
        private Button _buttonConnect;

        private Slider _userCountSlider;
        private TMP_Text _currentCount;
        private UILoadingProgress _uiLoadingProgress;

        
        public override TMP_InputField IdInputField => _roomNameInputField;
        public override TMP_InputField PwInputField => _roomPwInputField;

        public Button CloseButton => _buttonClose;

        private UILoadingProgress UILoadingProgress
        {
            get
            {
                if (_uiLoadingProgress == null)
                {
                    _uiLoadingProgress = _uiManagerServices.GetOrCreateSceneUI<UILoadingProgress>();
                }

                return _uiLoadingProgress;
            }
        }


        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_InputField>(typeof(InputFields));
            Bind<Button>(typeof(Buttons));
            Bind<Slider>(typeof(Sliders));
            Bind<TMP_Text>(typeof(Texts));
            Bind<Toggle>(typeof(Toggles));
            _roomNameInputField = Get<TMP_InputField>((int)InputFields.RoomNameInputField);
            _roomPwInputField = Get<TMP_InputField>((int)InputFields.RoomPwInputField);
            _roomAvailableToggle = Get<Toggle>((int)Toggles.RoomAvailableToggle);
            _buttonConnect = Get<Button>((int)Buttons.ButtonConnect);
            _buttonClose = Get<Button>((int)Buttons.ButtonClose);
            _userCountSlider = Get<Slider>((int)Sliders.UserCountSlider);
            _currentCount = Get<TMP_Text>((int)Texts.CurrentCount);
            _buttonConnect.onClick.AddListener(() =>CreateRoom().Forget());
            _buttonClose.onClick.AddListener(OnClickCloseButton);
            _roomAvailableToggle.onValueChanged.AddListener(SetRoomPasswordInputState);
            _userCountSlider.onValueChanged.AddListener((value) =>
            {
                _currentCount.text = value.ToString();
                _soundManagerServices.PlayUiSfx(gameObject, "OnChangePeopleCntValue");
            });

            _roomNameInputField.onEndEdit.AddListener((value) =>
            {
                string finalText = value;
                if (!string.IsNullOrEmpty(Input.compositionString))
                {
                    finalText += Input.compositionString;
                }
                _roomNameInputField.text = finalText;
            });
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            _roomAvailableToggle.SetIsOnWithoutNotify(false);
            SetRoomPasswordInputState(false);
        }

        private void SetRoomPasswordInputState(bool isAvailable)
        {
            _roomPwInputField.interactable = isAvailable;
            if (isAvailable == false)
            {
                _roomPwInputField.text = "";
            }
        }

        public async UniTaskVoid CreateRoom()
        {
            _buttonConnect.interactable = false;
            try
            {
                if (string.IsNullOrWhiteSpace(_roomNameInputField.text))
                {
                    _uiManagerServices.GetMessageErrorToast().Show("방제목을 입력해주세요",new Vector3(0,10,0),
                        () => _buttonConnect.interactable = true);
                    return;
                }
                
                CreateLobbyOptions option = new CreateLobbyOptions()
                {
                    IsPrivate = false,
                    Data = new System.Collections.Generic.Dictionary<string, DataObject>
                    {
                        {
                            "LobbyType",
                            new DataObject(
                                DataObject.VisibilityOptions.Public,
                                value:"CharactorSelect",
                                index:DataObject.IndexOptions.S1)
                        },
                        { //5.13일 추가 시작한 방을 이후에 플레이어가 못들어오게 막는 방어용 쿼리
                            "RoomState",
                            new DataObject(
                                DataObject.VisibilityOptions.Public,
                                value: "Preparing",
                                index: DataObject.IndexOptions.S2)
                        }
                        
                    }
                };  
                
                
                if (_roomAvailableToggle.isOn)
                {
                    string passWord = _roomPwInputField.text;
                    if (passWord.Length < 8)
                    {
                        if (_uiManagerServices.TryGetPopupDictAndShowPopup(out UIAlertDialog dialog) == true)
                        {
                            dialog.AlertSetText("오류", "비밀번호는 8자리 이상");
                            _buttonConnect.interactable = true;
                        }
                        return;
                    }

                    option.Password = passWord;
                }
                
                
                
                UILoadingProgress.ShowLoading("방 만드는중", "새 방을 준비하고 있습니다. 잠시 후 캐릭터 선택창으로 이동합니다.");
                try
                {
                    await _lobbyManager.CreateLobby(_roomNameInputField.text, int.Parse(_currentCount.text), option);
                }
                finally
                {
                    UILoadingProgress.HideLoading();
                }

                _sceneManagerEx.LoadScene(Define.SceneName.RoomScene);
            }
            catch (Exception e)
            {
                UtilDebug.Log(e);
                _buttonConnect.interactable = true;
            }

        }
        public void OnClickCloseButton()
        {
            _uiManagerServices.ClosePopupUI(this);
        }
        protected override void StartInit()
        {
        }
    }
}
