# Skill Pipeline

## 배경

스킬을 개별 클래스로만 구현하면, 스킬이 늘어날수록 유사한 타겟팅, 애니메이션, VFX, 데미지 처리 코드가 반복됩니다. 이 샘플은 스킬 데이터를 ScriptableObject로 작성하고, 런타임에서는 공통 파이프라인이 데이터를 해석해 실행하도록 만든 구조입니다.

## 목표

- 스킬 실행 흐름을 Trigger / Targeting / Sequence / Decorator / Effect 단계로 분리한다.
- 새 스킬을 만들 때 기존 전략을 조합해 재사용할 수 있게 한다.
- 특정 단계만 새 전략으로 확장할 수 있게 한다.
- 완료된 스킬만 cooldown과 UI 완료 이벤트를 처리한다.

## 설계

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

## 검토 진입점

1. `Source/Assets/Scripts/DataType/Skill/RuntimeSkill.cs`
2. `Source/Assets/Scripts/DataType/Skill/SkillPipeline.cs`
3. `Source/Assets/Scripts/DataType/Skill/SkillDataSO.cs`
4. `Source/Assets/Scripts/DataType/Skill/Factory/SkillPipelineFactory.cs`
5. `Source/Assets/Scripts/ScenesScripts/CommonInstaller/InGameInstaller/RuntimeSkillManagerInstaller.cs`

## 포함된 스크립트

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

## 제작 과정

| Part | 주제 | 링크 |
| --- | --- | --- |
| 1 | **개요** | [보러가기](https://blog.naver.com/hiwoong12/224167895583) |
| 2 | **스킬 데이터 보관함 만들기** | [보러가기](https://blog.naver.com/hiwoong12/224170411314) |
| 3 | **스킬 실행** | [보러가기](https://blog.naver.com/hiwoong12/224170589821) |
| 4 | **RuntimeSkill 조립과 등록** | [보러가기](https://blog.naver.com/hiwoong12/224171700092) |
| 5 | **SerializeReference로 레시피(SO) 만들기** | [보러가기](https://blog.naver.com/hiwoong12/224171823727) |
| 6 | **Trigger (어떻게 시작할까?)** | [보러가기](https://blog.naver.com/hiwoong12/224171872345) |
| 7 | **Targeting (무엇을 모을까?)** | [보러가기](https://blog.naver.com/hiwoong12/224172868239) |
| 8 | **Sequence (언제/어떤 순서로 실행할까?)** | [보러가기](https://blog.naver.com/hiwoong12/224172947169) |
| 9 | **Decorator: 연출 (무엇을 보여줄까?)** | [보러가기](https://blog.naver.com/hiwoong12/224173048136) |
| 10 | **Effect (어떤 효과를 입힐까?)** | [보러가기](https://blog.naver.com/hiwoong12/224173083292) |

