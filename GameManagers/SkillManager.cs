using System;
using System.Collections.Generic;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.SkillManager;
using GameManagers.Interface.UIManager;
using GameManagers.SubContainer.SkillManager;
using Skill.BaseSkill;
using UI.Scene.SceneUI;
using Util;
using Zenject;

namespace GameManagers
{
    public class SkillManager : IInitializable,ISkillManager
    {
        private readonly IUIManagerServices _uiManagerServices;
        private readonly IRequestDataType _requestDataType;
        private readonly ISkillFactory _skillFactory;
            
        [Inject]
        public SkillManager(IUIManagerServices uiManagerServices,IRequestDataType requestDataType,ISkillFactory skillFactory)
        {
            _uiManagerServices = uiManagerServices;
            _requestDataType = requestDataType;
            _skillFactory = skillFactory;
        }
        
        private Dictionary<string, BaseSkill> _allSKillDict = new Dictionary<string, BaseSkill>();
        private Action _doneUISkillBarInitEvent;
        private IList<Type> _skillType = new List<Type>();
        private UISkillBar _uiSkillBar;
        
        public IDictionary<string, BaseSkill> GetSkills()
        {
            return _allSKillDict;
        }

        public event Action DoneUISkilBarInitEvent
        {
            add => UniqueEventRegister.AddSingleEvent(ref _doneUISkillBarInitEvent,value);
            remove => UniqueEventRegister.RemovedEvent(ref _doneUISkillBarInitEvent, value);
        }

        public UISkillBar GetUISkillBar()
        {
            if (_uiSkillBar == null)
            {
                if(_uiManagerServices.Try_Get_Scene_UI(out UISkillBar skillbar))
                {
                    _uiSkillBar = skillbar;
                }
            }
            return _uiSkillBar;
        }

        public void Invoke_Done_UI_SKilBar_Init_Event()
        {
            _doneUISkillBarInitEvent?.Invoke();
        }
        public void Initialize()
        {
            //Skill/AllofSkill에 있는 타입들을 가져온다.
            _skillType = _requestDataType.LoadSerializableTypesFromFolder("Assets/Scripts/Skill/AllofSkills", GetAllofSkill);
            foreach (Type type in _skillType)
            {
           
                BaseSkill skill = _skillFactory.Create(type);
                
                _allSKillDict.Add(skill.SkillName, skill);
            }
        }
        private void GetAllofSkill(Type type, List<Type> typeList)
        {
            if (typeof(BaseSkill).IsAssignableFrom(type))
            {
                typeList.Add(type);
            }
        }
    }
}