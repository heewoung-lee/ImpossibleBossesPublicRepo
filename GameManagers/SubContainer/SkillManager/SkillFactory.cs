using System;
using GameManagers.Interface.UIManager;
using Skill.BaseSkill;
using Zenject;

namespace GameManagers.SubContainer.SkillManager
{
    public interface ISkillFactory
    {
        public BaseSkill Create(Type skillType);
    }
    
    
    public class SkillFactory : ISkillFactory
    {
        private readonly DiContainer _diContainer;
        public SkillFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }
        public BaseSkill Create(Type skillType)
        {
            BaseSkill skill  = _diContainer.Instantiate(skillType) as BaseSkill;
            return skill;
        } 
        
    }
}
