# Skill Pipeline

## Background

스킬을 개별 클래스로만 구현하면, 스킬이 늘어날수록 유사한 타겟팅, 애니메이션, VFX, 데미지 처리 코드가 반복됩니다. 이 샘플은 스킬 데이터를 ScriptableObject로 작성하고, 런타임에서는 공통 파이프라인이 데이터를 해석해 실행하도록 만든 구조입니다.

## Goal

- 스킬 실행 흐름을 Trigger / Targeting / Sequence / Decorator / Effect 단계로 분리한다.
- 새 스킬을 만들 때 기존 전략을 조합해 재사용할 수 있게 한다.
- 특정 단계만 새 전략으로 확장할 수 있게 한다.
- 완료된 스킬만 cooldown과 UI 완료 이벤트를 처리한다.

## Design

- `RuntimeSkill`
  - 스킬 사용 가능 여부, 실행 중복 방지, cooldown 적용, 완료 이벤트 발행을 담당합니다.

- `SkillPipeline`
  - targeting selection 이후 sequence를 실행합니다.
  - sequence는 decorator와 effect를 받아 실제 스킬 실행 순서를 구성합니다.
  - 완료/취소를 한 번만 처리하고, module release를 정리합니다.

- `SkillDataSO`
  - 스킬 데이터와 pipeline recipe를 담습니다.
  - trigger, targeting, sequence, decorator, effect를 SerializeReference로 조합합니다.

- `RuntimeSkillFactory`
  - 원본 `SkillDataSO`를 런타임 인스턴스로 복제하고, trigger와 pipeline을 생성합니다.

- `SkillPipelineFactory`
  - 각 def 타입에 맞는 strategy를 찾고 module을 생성합니다.

- Strategy factories
  - `TriggerFactory`, `TargetFactory`, `SequenceFactory`, `DecoratorFactory`, `EffectFactory`는 def type과 strategy를 매핑합니다.

## Review Entry Points

1. `Source/Assets/Scripts/DataType/Skill/RuntimeSkill.cs`
2. `Source/Assets/Scripts/DataType/Skill/SkillPipeline.cs`
3. `Source/Assets/Scripts/DataType/Skill/SkillDataSO.cs`
4. `Source/Assets/Scripts/DataType/Skill/Factory/SkillPipelineFactory.cs`
5. `Source/Assets/Scripts/ScenesScripts/CommonInstaller/InGameInstaller/RuntimeSkillManagerInstaller.cs`

## Included Scripts

- `RuntimeSkill.cs`
- `SkillPipeline.cs`
- `SkillDataSO.cs`
- `RuntimeSkillFactory.cs`
- `SkillPipelineFactory.cs`
- `RuntimeSkillManagerInstaller.cs`
- Trigger interfaces/factory/example
- Targeting interfaces/factory/example
- Sequence interfaces/factory/example
- Decorator interfaces/factory/stack implementation
- Effect interfaces/factory/example
- `SkillNetworkRouter.cs`

