using System.Threading.Tasks;
using Data.DataType.ItemType;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.GameManagerEx;
using GameManagers.Interface.ItemDataManager;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.UIFactoryManager.SceneUI;
using GameManagers.Interface.UIManager;
using GameManagers.Interface.VFXManager;
using Scene;
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
        public class UICreateItemAndGoldButtonFactory : SceneUIFactory<UICreateItemAndGoldButton>{}
        
        [Inject] private IUIManagerServices _uiManagerServices;
        [Inject] private IItemGetter _itemGetter;
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
                    _playerStats = _gameManagerEx.GetPlayer().GetComponent<PlayerStats>();
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
                TestGetDamaged();
                //await _lobbyManager.ShowUpdatedLobbyPlayers();
               // _ = FindMyJoinCodeAsync();
                Debug.Log(_gameManagerEx.GetPlayer().transform.position+"플레이어 좌표");
                //_gameManagerEx.GetPlayer().transform.position = Vector3.zero;
            }
            void MoveScene()
            {
                (_baseScene as IHasSceneMover).SceneMover.MoveScene();
            }

            //void AllLevelup()
            //{
            //    int layerMask = LayerMask.GetMask("Player", "AnotherPlayer");
            //    Collider[] hitColliders = Physics.OverlapSphere(_gameManagerEx.GetPlayer().transform.position, 10f, layerMask);
            //    foreach (var collider in hitColliders)
            //    {
            //      Managers.VFX_Manager.GenerateParticle("Prefabs/Player/SkillVFX/Level_up", collider.transform);
            //    }
            //}
        }
    
        public void TestGetGold() => PlayerStats.Gold += 5;
        public void TestGetDamaged() => PlayerStats.OnAttacked(_playerStats,2);
        public void TestGetExp() => PlayerStats.Exp += 5;

        public void TestGenerateBossSkill1()
        {
            GameObject stone = _resourceManager.InstantiateByKey("Prefabs/Enemy/Boss/AttackPattern/BossSkill1");
            stone.transform.SetParent(_vfxManager.VFXRootNgo, false);
            stone.transform.position = _gameManagerEx.GetPlayer().transform.position + Vector3.up * 5f;
        }

        public void TestIteminInventort()
        {
            if (_uiManagerServices.GetImportant_Popup_UI<UIPlayerInventory>().gameObject.activeSelf == false)
                return;

            switch (itemGeneratingType)
            {
                case ItemGeneratingType.EquipMent:
                    IItem equipmentitem = _itemGetter.GetRandomItem(typeof(ItemEquipment)).MakeInventoryItemComponent(_uiManagerServices);
                    break;
                case ItemGeneratingType.Consumable:
                    IItem consumableitem = _itemGetter.GetRandomItem(typeof(ItemConsumable)).MakeInventoryItemComponent(_uiManagerServices);
                    break;
                case ItemGeneratingType.All:
                    IItem item = _itemGetter.GetRandomItemFromAll().MakeInventoryItemComponent(_uiManagerServices);
                    break;
            }
        }

        private async Task FindMyJoinCodeAsync()
        {
            Debug.Log($"내 조인코드는 {_relayManager.JoinCode}");
            Debug.Log($"로비의 조인코드는{(await _lobbyManager.GetCurrentLobby()).Data["RelayCode"].Value}");
        }
    }
}
