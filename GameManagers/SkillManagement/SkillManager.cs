using System.Collections.Generic;
using System.Linq;
using DataType.Skill;
using GameManagers.ResourcesExManagement;
using Util;
using Zenject;

namespace GameManagers.SkillManagement
{
    public class SkillManager : IInitializable,ISkillManager
    {
        private readonly IResourcesServices _resourcesServices;
        
        private List<SkillDataSO> _allSkillDataList = new List<SkillDataSO>();

        [Inject]
        public SkillManager(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        public List<SkillDataSO> GetSkillDataList(Define.PlayerClass playerClass)
        {
            return _allSkillDataList
                .Where(data => data.playerClass == playerClass)
                .ToList();
        }
        public void Initialize()
        {
            _allSkillDataList = _resourcesServices.LoadAll<SkillDataSO>("SOData").ToList();
        }
    }
}