using DataType.Skill.Factory;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence;
using DataType.Skill.Factory.Sequence.GetLength.Strategy;
using DataType.Skill.Factory.Target;
using DataType.Skill.Factory.Trigger;
using GameManagers;
using Zenject;

namespace Scene.CommonInstaller.InGameInstaller
{
    /// <summary>
    /// 1.13일 부로 SceneContext로 이관
    /// TargetManager의 SceneContext고정이라서,
    /// SkillManager도 전부 SceneContext로 내릴 수 밖에 없었음.
    /// 안그러면 TargetManager를 빈 깡통으로 받는 TargetManager Provider를 만들어야하는데
    /// 그건 너무 복잡해질 것같아 아예 스킬매니저를 내리는것으로 함
    /// </summary>
    public class RuntimeSkillManagerInstaller : Installer<RuntimeSkillManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RuntimeSkillFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<SkillPipelineFactory>().AsSingle();

            Container.BindInterfacesAndSelfTo<TriggerFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<TargetFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<DecoratorFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<EffectFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<SequenceFactory>().AsSingle();

            
            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<ISkillTriggerStrategy>()).AsSingle();

            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<ITargetingStrategy>()).AsSingle();

            Container.Bind<ISequenceStrategy>()
                .To(x => x.AllNonAbstractClasses().DerivingFrom<ISequenceStrategy>())
                .AsSingle();
            
            
            Container.Bind<IDecoratorStrategy>()
                .To<DecoratorStackStrategy>()
                .AsSingle(); //데코레이션 스택 전략만 바인딩

            Container.Bind<IStackElementDecoratorStrategy>()
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IStackElementDecoratorStrategy>())
                .AsSingle(); //데코레이션 요소 바인딩

            Container.Bind<IMeleeComboLengthStrategy>()
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IMeleeComboLengthStrategy>())
                .AsSingle(); //밀리 콤보의 구간을 가져오는 전략 바인딩


            Container.Bind<IMeleeComboLengthResolver>()
                .To<MeleeComboLengthResolver>().AsSingle(); //밀리 콤보의 시간을 계산해주는 클래스

            Container.Bind(x => x.AllInterfaces())
                .To(x => x.AllNonAbstractClasses().DerivingFrom<IEffectStrategy>()).AsSingle();

            
            
            // //Test//
            // Container.BindInterfacesAndSelfTo<ImmediateTriggerStrategy>().AsSingle();
            // Container.BindInterfacesAndSelfTo<NoSelectionTargetingStrategy>().AsSingle();
            // Container.BindInterfacesAndSelfTo<ImmediateDecoratorStrategy>().AsSingle();
            // Container.BindInterfacesAndSelfTo<DebugLogEffectStrategy>().AsSingle();
        }
    }
}