using Cysharp.Threading.Tasks;
using DataType;
using DataType.Item;
using GameManagers;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.VFXManager;
using GameManagers.ItamData.Interface;
using GameManagers.RelayManager;
using GameManagers.ResourcesEx;
using GameManagers.Scene;
using GameManagers.UIFactory.SceneUI;
using Scene;
using Stats;
using TMPro;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UICreateItemAndGoldButton : UIScene
    {
        public class UICreateItemAndGoldButtonFactory : SceneUIFactory<UICreateItemAndGoldButton>{}
        
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IItemDataManager _itemDataManager;
        [Inject] private IPlayerSpawnManager _gameManagerEx;
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private SceneManagerEx _sceneManagerEx;
        [Inject] private RelayManager _relayManager;
        [Inject] private IResourcesServices _resourceManager;
        [Inject] private IVFXManagerServices _vfxManager;
        [Inject] private BaseScene _baseScene;
        
        private Button _scoreButton;
        private Button _moveSceneButton;
        private TMP_Text _scoreText;

        private PlayerStats _playerStats;
        public PlayerStats PlayerStats
        {
            get
            {
                if(_playerStats == null)
                {
                    var player = _gameManagerEx.GetPlayer();
                    if(player != null)
                        _playerStats = player.GetComponent<PlayerStats>();
                }
                return _playerStats;
            }
        }

        public ItemGeneratingType itemGeneratingType = ItemGeneratingType.All;

        enum Buttons
        {
            ScoreButton,
            MoveDownTownScene
        }
        enum Texts
        {
            ScoreText,
        }

        public enum ItemGeneratingType
        {
            EquipMent,
            Consumable,
            All
        }
        
        protected override void AwakeInit()
        {
            base.AwakeInit();

            Bind<Button>(typeof(Buttons));
            Bind<TMP_Text>(typeof(Texts));

            _scoreButton = GetButton((int)Buttons.ScoreButton);
            _moveSceneButton = GetButton((int)Buttons.MoveDownTownScene);
            _scoreText = GetText((int)Texts.ScoreText);
        }
        
        protected override void StartInit()
        {
            InitalizeUI_Button();
        }

        public void IninitalizePlayerStats(GameObject player)
        {
            _playerStats = player.GetComponent<PlayerStats>();
        }
        
        public void InitalizeUI_Button()
        {
            _scoreButton.onClick.AddListener(TestButtonClick);
            _moveSceneButton.onClick.AddListener(MoveScene);

            void TestButtonClick()
            {
                TestIteminInventort();
                TestGetGold();
                TestGetExp();
                TestGetDamaged(10000000);
            }
            void MoveScene()
            {
                (_baseScene as IHasSceneMover).SceneMover.MoveScene();
            }
        }
    
        public void TestGetGold() 
        {
            if (PlayerStats != null) PlayerStats.Gold += 5;
        } 
        public void TestGetDamaged(int damage) 
        {
            if (PlayerStats != null) PlayerStats.OnAttacked(_playerStats, damage);
        }
        public void TestGetExp() 
        {
            if (PlayerStats != null) PlayerStats.Exp += 5;
        }

        public void TestGenerateBossSkill1()
        {
            if (_gameManagerEx.GetPlayer() == null) return;
            
            GameObject stone = _resourceManager.InstantiateByKey("Prefabs/Enemy/Boss/AttackPattern/BossSkill1");
            stone.transform.SetParent(_vfxManager.VFXRootNgo, false);
            stone.transform.position = _gameManagerEx.GetPlayer().transform.position + Vector3.up * 5f;
        }

        public void TestIteminInventort()
        {
            var inventory = _uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>();
            if (inventory == null || inventory.gameObject.activeSelf == false)
                return;

            ItemDataSO targetItem = null;
            int maxRetry = 50; 

            for (int i = 0; i < maxRetry; i++)
            {
                ItemDataSO randomItem = _itemDataManager.GetRandomItemData();
                if (randomItem == null) break;

                bool isMatch = false;
                switch (itemGeneratingType)
                {
                    case ItemGeneratingType.All:
                        isMatch = true;
                        break;
                    case ItemGeneratingType.EquipMent:
                        if (randomItem.ItemType == ItemType.Equipment) isMatch = true;
                        break;
                    case ItemGeneratingType.Consumable:
                        if (randomItem.ItemType == ItemType.Consumable) isMatch = true;
                        break;
                }

                if (isMatch)
                {
                    targetItem = randomItem;
                    break;
                }
            }

            if (targetItem != null)
            {
                inventory.AddItem(targetItem);
                Debug.Log($"[TestUI] 아이템 생성됨: {targetItem.dataName}");
            }
            else
            {
                Debug.LogWarning("[TestUI] 조건에 맞는 아이템을 찾지 못했습니다.");
            }
        }

        private async UniTask FindMyJoinCodeAsync()
        {
            Debug.Log($"내 조인코드는 {_relayManager.JoinCode}");
            var lobby = await _lobbyManager.GetCurrentLobby();
            if (lobby != null)
            {
                Debug.Log($"로비의 조인코드는{lobby.Data["RelayCode"].Value}");
            }
        }
    }
}