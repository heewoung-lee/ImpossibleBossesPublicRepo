using GameManagers;
using GameManagers.GameManagerExManagement;
using GameManagers.UIFactoryManagement.SceneUI;
using Stats;
using Stats.BaseStats;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace UI.Scene.SceneUI
{
    public class UIPlayerInfo : UIScene
    {
        public class UIPlayerInfoFactory : SceneUIFactory<UIPlayerInfo>{}

        [Inject] IPlayerSpawnManager _gameManagerEx;

        private Slider _hpSlider;
        private Slider _requiredXpSlider;
        private TMP_Text _hpText;
        private TMP_Text _levelText;
        private TMP_Text _playerNameText;
        private PlayerStats _playerStats;

        enum Sliders
        {
            PlayerHpSlider,
            RequiredXP
        }

        enum UserText
        {
            HpText,
            LevelText,
            PlayerNameText
        }

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<Slider>(typeof(Sliders));
            Bind<TMP_Text>(typeof(UserText));

            _hpSlider = Get<Slider>((int)Sliders.PlayerHpSlider);
            _requiredXpSlider = Get<Slider>((int)Sliders.RequiredXP);
            _hpText = GetText((int)UserText.HpText);
            _levelText = GetText((int)UserText.LevelText);
            _playerNameText = GetText((int)UserText.PlayerNameText);
        }

        protected override void StartInit()
        {
            if (_gameManagerEx.GetPlayer() == null)
            {
                _gameManagerEx.OnPlayerSpawnEvent += InitalizePlayerInfo;
                _gameManagerEx.OnPlayerSpawnEvent += UpdateUI;
            }
            else
            {
                _playerStats = _gameManagerEx.GetPlayer().gameObject.GetComponent<PlayerStats>();
                InitalizePlayerInfo(_playerStats);
                UpdateUI(_playerStats);
            }
        }

        private void OnDestroy()
        {
            _gameManagerEx.OnPlayerSpawnEvent -= InitalizePlayerInfo;
            _gameManagerEx.OnPlayerSpawnEvent -= UpdateUI;
            UnBindPlayerInfo();
        }

        private void UpdateUI(PlayerStats stats)
        {
            UpdateHpInfo(stats);

            if (stats.CharacterBaseStats.Equals(default(CharacterBaseStat)))
                return;

            UpdatePlayerInfo(stats);
        }

        private void InitalizePlayerInfo(PlayerStats stats)
        {
            _gameManagerEx.OnPlayerSpawnEvent -= InitalizePlayerInfo;
            _gameManagerEx.OnPlayerSpawnEvent -= UpdateUI;
            UnBindPlayerInfo();

            _playerStats = stats;
            stats.CurrentHpValueChangedEvent += UpdateCurrentHpValue;
            stats.MaxHpValueChangedEvent += UpdateCurrentMaxHpValue;
            stats.DoneBaseStatsLoading += UpdateBaseStatsInfo;
            stats.PlayerExpChangedEvent += UpdateExpFill;

            UpdateUI(stats);
        }

        private void UnBindPlayerInfo()
        {
            if (_playerStats == null)
                return;

            _playerStats.CurrentHpValueChangedEvent -= UpdateCurrentHpValue;
            _playerStats.MaxHpValueChangedEvent -= UpdateCurrentMaxHpValue;
            _playerStats.DoneBaseStatsLoading -= UpdateBaseStatsInfo;
            _playerStats.PlayerExpChangedEvent -= UpdateExpFill;
            _playerStats = null;
        }

        private void UpdateBaseStatsInfo(CharacterBaseStat stat)
        {
            UpdateUI(_playerStats);
        }

        private void UpdateExpFill(int currentExp)
        {
            int requiredExp = _playerStats.RequiredExpForNextLevel;
            _requiredXpSlider.value = requiredExp <= 0
                ? 1f
                : (float)currentExp / requiredExp;
        }

        private void UpdateCurrentMaxHpValue(int preCurrentMaxHp, int currentMaxHp)
        {
            UpdateHpInfo(_playerStats);
        }

        private void UpdateCurrentHpValue(int preCurrentHp, int currentHp)
        {
            UpdateHpInfo(_playerStats);
        }

        private void UpdateHpInfo(PlayerStats stat)
        {
            if (stat == null || stat.MaxHp <= 0)
                return;

            _hpText.text = $"{stat.Hp}/{stat.MaxHp}";
            _hpSlider.value = (float)stat.Hp / stat.MaxHp;
        }

        private void UpdatePlayerInfo(PlayerStats stat)
        {
            _levelText.text = stat.Level.ToString();
            _playerNameText.text = stat.Name.ToString();
            UpdateExpFill(stat.Exp);
        }
    }
}
