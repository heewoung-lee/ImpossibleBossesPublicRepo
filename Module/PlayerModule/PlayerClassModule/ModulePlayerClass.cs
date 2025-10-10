using System.Collections.Generic;
using System.Linq;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.Interface.SkillManager;
using Scene;
using Skill;
using Skill.BaseSkill;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Zenject;

namespace Module.PlayerModule.PlayerClassModule
{
    public abstract class ModulePlayerClass : MonoBehaviour
    {
        private IResourcesServices _resourcesServices;
        private ISkillManager _skillManager;
        private SceneManagerEx _sceneManagerEx;
        private RelayManager _relayManager;

        public abstract Define.PlayerClass PlayerClass { get; }
        private Dictionary<string, BaseSkill> _playerSkill;

        [Inject]
        public void Construct(
            IResourcesServices resourcesServices,
            ISkillManager skillManager,
            SceneManagerEx sceneManagerEx,
            RelayManager relayManager
            )
        {
            _resourcesServices = resourcesServices;
            _skillManager = skillManager;
            _sceneManagerEx = sceneManagerEx;
            _relayManager = relayManager;
        }
        
        
        public virtual void InitializeOnAwake()
        {
        
        }
        public void OnEnable()
        {
            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted += ChangeLoadScene;
        }
        public void OnDisable()
        {
            _relayManager.NetworkManagerEx.SceneManager.OnLoadEventCompleted -= ChangeLoadScene;
        }
        public virtual void InitializeOnStart()
        {
            InitializeSkillsFromManager();
        }
  
        private void ChangeLoadScene(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (_sceneManagerEx.GetCurrentScene is not ISkillInit)
                return;

            if (clientsCompleted.Contains(_relayManager.NetworkManagerEx.LocalClientId) is false)
                return;

            InitializeSkillsFromManager();
        }

        private void InitializeSkillsFromManager()
        {
            if (GetComponent<NetworkObject>().IsOwner == false)
                return;

            _playerSkill = _skillManager.GetSkills()
                .Where(skill => skill.Value.PlayerClass == PlayerClass)
                .ToDictionary(skill => skill.Key, skill => skill.Value);//각 클래스에 맞는 스킬들을 추린다

            if (_skillManager.GetUISkillBar() == null)
            {
                _skillManager.DoneUISkilBarInitEvent += AssignSkillsToUISlots;
            }
            else
            {
                AssignSkillsToUISlots();
            }
        }


        public void AssignSkillsToUISlots()
        {
            foreach (BaseSkill skill in _playerSkill.Values)
            {
                GameObject skillPrefab = _resourcesServices.InstantiateByKey("Prefabs/UI/Skill/UI_SkillComponent");
                SkillComponent skillcomponent = _resourcesServices.GetOrAddComponent<SkillComponent>(skillPrefab);
                skillcomponent.SetSkillComponent(skill);
                Transform skillLocation = _skillManager.GetUISkillBar().SetLocationSkillSlot(skillcomponent);
                skillcomponent.AttachItemToSlot(skillcomponent.gameObject, skillLocation);
            }
        }
        private void Awake()
        {
            _playerSkill = new Dictionary<string, BaseSkill>();
            InitializeOnAwake();
        }

        private void Start()
        {
            InitializeOnStart();
        }
    }
}
