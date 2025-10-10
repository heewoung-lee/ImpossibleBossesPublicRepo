using System;
using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI.Popup.PopupUI;
using UI.Scene;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Test.TestUI
{
    public class LogInTestToggle : UIScene
    {
        [Inject]private IUIManagerServices _uiManager; 
        enum Buttons
        {
            TestButton
        }
        enum Toggles 
        {
            LogInTestToggle
        }

        enum Players
        {
            Player1,
            Player2,
            Player3,
            Player4,
            None
        }


        Button _testButton;
        Toggle _testToggle;
        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Button>(typeof(Buttons));
            Bind<Toggle>(typeof(Toggles));
            _testButton = Get<Button>((int)Buttons.TestButton);
            _testToggle = Get<Toggle>((int)Toggles.LogInTestToggle);
            _testButton.interactable = _testToggle.interactable;
            _testToggle.onValueChanged.AddListener((ison) =>
            {
                _testButton.gameObject.SetActive(ison);
            });
            _testButton.onClick.AddListener(ClickLogin);
        }

        private async void ClickLogin()
        {

            if (_uiManager.TryGetPopupDictAndShowPopup(out UILoginPopup loginPopup) == true)
            {
                Players currentPlayer = Players.Player1;

                string[] tagValue = CurrentPlayer.ReadOnlyTags();
                if (tagValue.Length > 0 && Enum.TryParse(typeof(Players), tagValue[0], out var parsedEnum))
                {
                    currentPlayer = (Players)parsedEnum;
                    Debug.Log($"Current player: {currentPlayer}");
                }
                switch (currentPlayer)
                {
                    case Players.Player1:
                       await  loginPopup.AuthenticateUser("hiwoong123", "123123");
                        break;
                    case Players.Player2:
                       await loginPopup.AuthenticateUser("hiwoong12", "123123");
                        break;
                    case Players.Player3:
                        await loginPopup.AuthenticateUser("hiwoo12", "123123");
                        break;
                    case Players.Player4:
                        await loginPopup.AuthenticateUser("hiwoong1234", "123123");
                        break;
                    case Players.None:
                        break;
                }
            }
        }

        protected override void StartInit()
        {
            base.AwakeInit();
        }

    }
}
