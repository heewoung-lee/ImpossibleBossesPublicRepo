using System;
using System.Collections.Generic;
using System.Linq;
using DataType.Skill;
using GameManagers.Interface.DataManager;
using GameManagers.Interface.SkillManager;
using GameManagers.Interface.UIManager;
using Module.PlayerModule.PlayerClassModule;
using Scene.CommonInstaller;
using Skill;
using UI.Scene.SceneUI;
using UnityEngine;
using Util;
using Zenject;

namespace GameManagers
{
    public class SkillManager : IInitializable,ISkillManager
    {
        private List<SkillDataSO> _allSkillDataList = new List<SkillDataSO>();
        public List<SkillDataSO> GetSkillDataList(Define.PlayerClass playerClass)
        {
            return _allSkillDataList
                .Where(data => data.playerClass == playerClass)
                .ToList();
        }
        public void Initialize()
        {
            _allSkillDataList = UnityEngine.Resources.LoadAll<SkillDataSO>("SOData").ToList();
        }
    }
}