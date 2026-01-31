using System;
using System.Collections.Generic;
using DataType.Skill;
using Skill;
using UI.Scene.SceneUI;
using Util;

namespace GameManagers.Interface.SkillManager
{
    public interface ISkillManager
    {
        List<SkillDataSO> GetSkillDataList(Define.PlayerClass playerClass);
    }
}
