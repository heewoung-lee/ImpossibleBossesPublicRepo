using System;
using System.Collections.Generic;
using CustomEditor.Interfaces;
using CustomEditor.Multiplay;
using Scene;
using Scene.CommonInstaller;
using Scene.CommonInstaller.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using Util;


namespace CustomEditor
{

    public interface IMultiPlayModeScenarioController
    {
        public void SetUseMultiMode(bool useMultiMode, IMultiTestScene multiTestscene);
        public void UpdateTag(IMultiTestScene multiTestscene);
        public void SyncScenarioFromInspector(IMultiTestScene multiTestscene);
    }

    public class ScenarioSerializeController : IMultiPlayModeScenarioController
    {
        public void SetUseMultiMode(bool useMultiMode, IMultiTestScene multiTestscene)
            => MultiPlayScenarioSerializeCustom.SetUseMultiMode(useMultiMode, multiTestscene);

        public void UpdateTag(IMultiTestScene multiTestscene)
            => MultiPlayScenarioSerializeCustom.UpdateTag(multiTestscene);

        public void SyncScenarioFromInspector(IMultiTestScene multiTestscene)
            => MultiPlayScenarioSerializeCustom.SyncScenarioFromInspector(multiTestscene);
    }

    public class ScenarioReflectionController : IMultiPlayModeScenarioController
    {
        public void SetUseMultiMode(bool useMultiMode, IMultiTestScene multiTestscene)
            => MultiPlayScenarioReflectionCustom.SetUseMultiMode(useMultiMode, multiTestscene);

        public void UpdateTag(IMultiTestScene multiTestscene)
            => MultiPlayScenarioReflectionCustom.UpdateTag(multiTestscene);

        public void SyncScenarioFromInspector(IMultiTestScene multiTestscene)
            => MultiPlayScenarioReflectionCustom.SyncScenarioFromInspector(multiTestscene);
    }

    
    
    
    public class TestSceneEditor : MonoBehaviour, ISceneTestMode, ISceneMultiMode, ISceneSelectCharacter, IMultiTestScene
    {
        [SerializeField] private ScriptableObject _playSceneSenario;
        private ScenarioSerializeController _scenarioSerializeController;
        private ScenarioReflectionController _scenarioReflectionController;

        public ScenarioReflectionController ReflectionController
        {
            get
            {
                if (_scenarioReflectionController == null)
                {
                    _scenarioReflectionController = new ScenarioReflectionController();
                }
                return _scenarioReflectionController;
            }
        }

        public ScenarioSerializeController SerializeController
        {
            get
            {
                if (_scenarioSerializeController == null)
                {
                    _scenarioSerializeController = new ScenarioSerializeController();
                }
                return _scenarioSerializeController;
            }
        }
        
        
        private IMultiPlayModeScenarioController _scenarioController;

        public IMultiPlayModeScenarioController ScenarioController
        {
            get
            {
                if (_scenarioController == null)
                {
                    return SerializeController;
                }
                return _scenarioController;
            }

            private set => _scenarioController = value;
        }
        
        
        public TestMode GetTestMode() => testMode;
        public MultiMode GetMultiTestMode() => multiMode;

        public Define.PlayerClass GetPlayerableCharacter()
        {
            switch (TestMultiUtil.GetPlayerTag())
            {
                case PlayersTag.Player1:
                    return playerableCharacter1;
                case PlayersTag.Player2:
                    return playerableCharacter2;
                case PlayersTag.Player3:
                    return playerableCharacter3;
                case PlayersTag.Player4:
                    return playerableCharacter4;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private List<MultiTestPlayerInfo> _multiTestPlayerInfo = new List<MultiTestPlayerInfo>(4);

        public List<MultiTestPlayerInfo> MultiTestPlayerInfo
        {
            get
            {
                if (_multiTestPlayerInfo.Count == 0)
                {
                    for (int i = 0; i < 4; i++) _multiTestPlayerInfo.Add(new MultiTestPlayerInfo());
                    
                    _multiTestPlayerInfo[0].SetPlayerInfo(playerableCharacter1, playableCharacter1Tag);
                    _multiTestPlayerInfo[1].SetPlayerInfo(playerableCharacter2, playerableCharacter2Tag);
                    _multiTestPlayerInfo[2].SetPlayerInfo(playerableCharacter3, playerableCharacter3Tag);
                    _multiTestPlayerInfo[3].SetPlayerInfo(playerableCharacter4, playerableCharacter4Tag);
                }
                return _multiTestPlayerInfo;
            }
        }
        

        // 상단 공통 옵션
        [Title("테스트모드"), LabelWidth(90)] [SerializeField, EnumToggleButtons, LabelText("Test Mode")]
        [PropertySpace(SpaceAfter = 16)]
        private TestMode testMode = TestMode.Local;

        [ShowIfGroup("Multi", Condition = "@testMode == Scene.TestMode.Multi")]
        [LabelWidth(90)]
        [PropertySpace(SpaceAfter = 16)]
        [SerializeField, EnumToggleButtons, LabelText("Multi Mode"), Space(6)]
        [OnValueChanged("OnModeChanged")]
        private MultiMode multiMode = MultiMode.Solo;


        private int _tempMultiCounter = 1;
        private void OnModeChanged()
        {

            if (multiMode == MultiMode.Solo)
            {
                _tempMultiCounter = _playableCharacterCount;
                _playableCharacterCount = 1;
            }
            else
            {
                _playableCharacterCount = _tempMultiCounter;
            }
            LoadMultiInfo();
        }
        
        
        [ShowIfGroup("Multi")]
        [ShowIf("@multiMode == MultiMode.Multi")]
        [PropertySpace(SpaceAfter = 24)]
        [SerializeField, Range(1, 4), LabelText("Playable Count")]
        [OnValueChanged("OnPlayerCountChanged")]
        private int _playableCharacterCount = 1;

        private void OnPlayerCountChanged()
        {
            LoadMultiInfo();
        }
        

// === Players ===
// P1 : 항상 노출 (상위 Multi 그룹에 들어가면 보임)
        [ShowIfGroup("Multi")]
        [BoxGroup("Multi/Players")] // 행을 박스로 감싸 안정감
        [HorizontalGroup("Multi/Players/P1", Width = 0.55f)]
        [SerializeField, EnumToggleButtons, LabelText("Playable 1")]
        [PropertySpace(SpaceAfter = 16)]
        [OnValueChanged("OnPlayer1ClassChanged")]
        private Define.PlayerClass playerableCharacter1 = Define.PlayerClass.Archer;

        [HorizontalGroup("Multi/Players/P1")] [SerializeField, EnumPaging, LabelText("Tag"), LabelWidth(30)]
        [PropertySpace(SpaceAfter = 16)]
        [OnValueChanged("OnPlayer1ClassChanged")]
        private PlayersTag playableCharacter1Tag = PlayersTag.Player1;


        private void OnPlayer1ClassChanged()
        {
            MultiTestPlayerInfo[0].SetPlayerInfo(playerableCharacter1,playableCharacter1Tag);
            Debug.Log("OnPlayer1ClassChanged");
            ScenarioController.UpdateTag(this);
        }
        
        
// P2
        [ShowIfGroup("Multi")]
        [ShowIfGroup("Multi/Players/P2", Condition = "@multiMode == MultiMode.Multi && _playableCharacterCount >= 2")]
        [HorizontalGroup("Multi/Players/P2/Row", Width = 0.55f)]
        [SerializeField, EnumToggleButtons, LabelText("Playable 2")]
        [PropertySpace(SpaceAfter = 16)]
        [OnValueChanged("OnPlayer2ClassChanged")]
        private Define.PlayerClass playerableCharacter2 = Define.PlayerClass.Archer;

        [HorizontalGroup("Multi/Players/P2/Row")] [SerializeField, EnumPaging, LabelText("Tag"), LabelWidth(30)]
        [PropertySpace(SpaceAfter = 16)]
        [OnValueChanged("OnPlayer2ClassChanged")]
        private PlayersTag playerableCharacter2Tag = PlayersTag.Player2;
        
        private void OnPlayer2ClassChanged()
        {
            MultiTestPlayerInfo[1].SetPlayerInfo(playerableCharacter2,playerableCharacter2Tag);
            Debug.Log("OnPlayer2ClassChanged");
            ScenarioController.UpdateTag(this);
        }

// P3
        [ShowIfGroup("Multi")]
        [ShowIfGroup("Multi/Players/P3", Condition = "@multiMode == MultiMode.Multi && _playableCharacterCount >= 3")]
        [HorizontalGroup("Multi/Players/P3/Row", Width = 0.55f)]
        [SerializeField, EnumToggleButtons, LabelText("Playable 3")]
        [PropertySpace(SpaceAfter = 16)]
        [OnValueChanged("OnPlayer3ClassChanged")]
        private Define.PlayerClass playerableCharacter3 = Define.PlayerClass.Archer;

        [HorizontalGroup("Multi/Players/P3/Row")] [SerializeField, EnumPaging, LabelText("Tag"), LabelWidth(30)]
        [PropertySpace(SpaceAfter = 16)]
        [OnValueChanged("OnPlayer3ClassChanged")]
        private PlayersTag playerableCharacter3Tag = PlayersTag.Player3;

        private void OnPlayer3ClassChanged()
        {
            MultiTestPlayerInfo[2].SetPlayerInfo(playerableCharacter3,playerableCharacter3Tag);
            Debug.Log("OnPlayer3ClassChanged");
            ScenarioController.UpdateTag(this);
        }
// P4
        [ShowIfGroup("Multi")]
        [ShowIfGroup("Multi/Players/P4", Condition = "@multiMode == MultiMode.Multi && _playableCharacterCount >= 4")]
        [HorizontalGroup("Multi/Players/P4/Row", Width = 0.55f)]
        [SerializeField, EnumToggleButtons, LabelText("Playable 4")]
        [OnValueChanged("OnPlayer4ClassChanged")]
        private Define.PlayerClass playerableCharacter4 = Define.PlayerClass.Archer;

        [HorizontalGroup("Multi/Players/P4/Row")] [SerializeField, EnumPaging, LabelText("Tag"), LabelWidth(30)]
        [OnValueChanged("OnPlayer4ClassChanged")]
        private PlayersTag playerableCharacter4Tag = PlayersTag.Player4;
        
        private void OnPlayer4ClassChanged()
        {
            MultiTestPlayerInfo[3].SetPlayerInfo(playerableCharacter4,playerableCharacter4Tag);
            Debug.Log("OnPlayer4ClassChanged");
            ScenarioController.UpdateTag(this);
        }

        
        // === UseMultiMode ===
        [ShowIfGroup("Multi")]
        [PropertySpace(SpaceAfter = 12)]
        [HorizontalGroup("Multi/Row", Width = 0.55f)]
        [SerializeField, ToggleLeft, LabelText("UseScenario")]
        [OnValueChanged("OnUseMultiModeChanged")]
        private bool useMultiMode = false;

        private void OnUseMultiModeChanged()
        {
            ScenarioController.SetUseMultiMode(useMultiMode,this);
        }
        
        [ShowIfGroup("Multi")]
        [PropertySpace(SpaceAfter = 12)]
        [HorizontalGroup("Multi/Row")]
        [SerializeField, ToggleLeft, LabelText("UseSerializeVersion")]
        [OnValueChanged("OnUseSerializeVersion")]
        private bool useSerializeVersion = false;

        private void OnUseSerializeVersion()
        {
           if (useSerializeVersion == true)
           {
               ScenarioController = SerializeController;
           }
           else
           {
               ScenarioController = ReflectionController;
           }
        }
        
        
        private void LoadMultiInfo()
        {
            ScenarioController.SyncScenarioFromInspector(this);
        }
        
        public List<MultiTestPlayerInfo> GetMultiTestPlayers()
        {
            return MultiTestPlayerInfo.GetRange(0, _playableCharacterCount);
        }

        public ScriptableObject GetPlayScenarioSO()
        {
            if (_playSceneSenario == null)
            {
                Debug.Log("GetPlayScenarioSO is null");
                return null;
            }
            return _playSceneSenario;
        }
    }
}
