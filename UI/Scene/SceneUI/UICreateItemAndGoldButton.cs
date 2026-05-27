using Character.Skill.AllofSkills.BossMonster.DarkWizard;
using Cysharp.Threading.Tasks;
using DataType;
using DataType.Item;
using DataType.Strategies.Item;
using GameManagers.GameManagerExManagement;
using GameManagers.ItemDataManagement.Interface;
using GameManagers.LobbyManagement;
using GameManagers.RelayManagement;
using GameManagers.ResourcesExManagement;
using GameManagers.SceneManagement;
using GameManagers.UIFactoryManagement.SceneUI;
using GameManagers.UIManagement;
using GameManagers.VFXManagement;
using ScenesScripts;
using Stats;
using TMPro;
using UI.Popup.PopupUI;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.Scene.SceneUI
{
    
    public class UICreateItemAndGoldButton : UIScene
    {
        public const int TestDamage = 10;
        
        
        public class UICreateItemAndGoldButtonFactory : SceneUIFactory<UICreateItemAndGoldButton>{}
        
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IItemDataManager _itemDataManager;
        [Inject] private GameManagerEx _gameManagerEx;
        [Inject] private LobbyManager _lobbyManager;
        [Inject] private SceneManagerEx _sceneManagerEx;
        [Inject] private RelayManager _relayManager;
        [Inject] private IResourcesServices _resourceManager;
        [Inject] private IVFXManagerServices _vfxManager;
        [Inject] private BaseScene _baseScene;
        
        private Button _scoreButton;
        private Button _moveSceneButton;
        private Button _testButton;
        
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
            MoveDownTownScene,
            TestButton
        }
        enum Texts
        {
            ScoreText,
        }

        public enum ItemGeneratingType
        {
            EquipMent = 0,
            Consumable = 1,
            All = 2,
        }
        
        protected override void AwakeInit()
        {
            base.AwakeInit();

            Bind<Button>(typeof(Buttons));
            Bind<TMP_Text>(typeof(Texts));

            _scoreButton = GetButton((int)Buttons.ScoreButton);
            _moveSceneButton = GetButton((int)Buttons.MoveDownTownScene);
            _testButton = GetButton((int)Buttons.TestButton);
            
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

        private void HideButton()
        {
            _scoreButton.gameObject.SetActive(false);
            _moveSceneButton.gameObject.SetActive(false);
        }
        
        public void InitalizeUI_Button()
        {
            _scoreButton.onClick.AddListener(TestButtonClick);
            _moveSceneButton.onClick.AddListener(MoveScene);
            _testButton.onClick.AddListener(SpawnBossProjectile);

            void TestButtonClick()
            {
                TestIteminInventort();
                TestGetGold();
                TestGetExp(0);
                TestGetDamaged(TestDamage);
            }
            void MoveScene()
            {
                (_baseScene as IHasSceneMover).SceneMover.MoveScene();
            }

            void SpawnBossProjectile()
            {
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
        public void TestGetExp(int exp) 
        {
            if (PlayerStats != null) PlayerStats.Exp += exp;
        }

        public void TestGenerateBossSkill1()
        {
            if (_gameManagerEx.GetPlayer() == null) return;

            _vfxManager.InstantiateParticleInArea(
                "Prefabs/Enemy/Boss/AttackPattern/StoneGolem/NgoBossSkill1AttackHit",
                _gameManagerEx.GetPlayer().transform.position + Vector3.up * 5f);
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
                        if (randomItem.ItemType == ItemType.Equipment || randomItem.ItemType == ItemType.Consumable)
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
                UtilDebug.Log($"[TestUI] 아이템 생성: {targetItem.dataName}");
            }
            else
            {
                UtilDebug.LogWarning("[TestUI] 조건에 맞는 아이템을 찾지 못했습니다");
            }
        }

        private async UniTask FindMyJoinCodeAsync()
        {
            UtilDebug.Log($"내 조인코드: {_relayManager.JoinCode}");
            var lobby = await _lobbyManager.GetCurrentLobby();
            if (lobby != null)
            {
                UtilDebug.Log($"로비 조인코드: {lobby.Data["RelayCode"].Value}");
            }
        }
    }
}
