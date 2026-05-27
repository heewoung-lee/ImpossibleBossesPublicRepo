using System.Collections.Generic;
using DataType.Skill;
using Util;

namespace GameManagers.SkillManagement
{
    public interface ISkillManager
    {
        List<SkillDataSO> GetSkillDataList(Define.PlayerClass playerClass);
    }
}
