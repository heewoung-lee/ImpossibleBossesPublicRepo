using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers;
using GameManagers.SubContainer.SkillManager;
using Skill.BaseSkill;
using UnityEngine;
using Zenject;

namespace ProjectContextInstaller
{
    [DisallowMultipleComponent]
    public class SkillManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        { 
            
            Container.BindInterfacesAndSelfTo<SkillManager>().AsSingle();
            
            Container.Bind<ISkillFactory>()   
                .FromSubContainerResolve()
                .ByInstaller<SkillFactoryInstaller>()
                .AsSingle();
            
        }
    }



    public class SkillFactoryInstaller : Installer<SkillFactoryInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SkillFactory>().AsSingle();

            IEnumerable<Type> skillTypes = typeof(BaseSkill).Assembly.GetTypes()
                .Where(alltype => typeof(BaseSkill).IsAssignableFrom(alltype) //자손 확인
                                  && alltype.IsAbstract == false //추상클래스는 걸러냄
                                  && alltype.IsInterface == false); //인터페이스도 걸러냄

            foreach (Type skilltype in skillTypes)
            {
                Container.Bind(skilltype).AsSingle();
            }
        }
    }
}
