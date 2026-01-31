using Controller;
using DataType.Skill.Factory.Decorator;
using DataType.Skill.Factory.Effect;
using DataType.Skill.Factory.Sequence;
using DataType.Skill.Factory.Target;
using Skill;
using UnityEngine;

namespace DataType.Skill.Factory
{
    public interface ISkillPipelineFactory
    {
        ISkillPipeline Create(SkillDataSO data, BaseController owner);
    }

   public sealed class SkillPipelineFactory : ISkillPipelineFactory
    {
        private readonly ITargetingFactory _targetingFactory;
        private readonly ISequenceFactory _sequenceFactory;
        private readonly IDecoratorFactory _decoratorFactory;
        private readonly IEffectFactory _effectFactory;

        public SkillPipelineFactory(
            ITargetingFactory targetingFactory,
            ISequenceFactory sequenceFactory,
            IDecoratorFactory decoratorFactory,
            IEffectFactory effectFactory)
        {
            _targetingFactory = targetingFactory;
            _sequenceFactory = sequenceFactory;
            _decoratorFactory = decoratorFactory;
            _effectFactory = effectFactory;
        }

        public ISkillPipeline Create(SkillDataSO data, BaseController owner)
        {
            if (data.targeting == null || data.sequence == null || data.decorator == null || data.effect == null)
            {
                Debug.LogError($"[PipelineFactory] Def is null. Skill: {data.name}");
                return null;
            }

            ITargetingStrategy targetingStrategy = _targetingFactory.GetTargeting(data.targeting);
            ISequenceStrategy sequenceStrategy = _sequenceFactory.GetSequence(data.sequence);
            IDecoratorStrategy decoratorStrategy = _decoratorFactory.GetDecorator(data.decorator);
            IEffectStrategy effectStrategy = _effectFactory.GetEffect(data.effect);

            if (targetingStrategy == null || sequenceStrategy == null || decoratorStrategy == null || effectStrategy == null)
            {
                Debug.LogError($"[PipelineFactory] Strategy missing. Skill: {data.name}");
                return null;
            }

            ITargetingModule targeting = targetingStrategy.Create(data.targeting, owner);
            ISequenceModule sequence = sequenceStrategy.Create(data.sequence, owner);
            IDecoratorModule decorator = decoratorStrategy.Create(data.decorator, owner);
            IEffectModule effect = effectStrategy.Create(data.effect, owner);

            return new SkillPipeline(targeting, sequence, decorator, effect);
        }
    }
}