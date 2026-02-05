using GameManagers;
using GameManagers.Skill;
using GameManagers.Target;
using Zenject;

namespace ZenjectContext.ProjectContextInstaller
{
    public class SkillManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SkillManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<TargetManagerProvider>().AsSingle();
        }
    }
}
