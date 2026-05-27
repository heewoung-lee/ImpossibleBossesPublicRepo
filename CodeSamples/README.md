# Impossible Bosses - Code Samples

Unity / C# / Zenject / Netcode for GameObjects 기반 협동 보스전 프로젝트에서 직접 설계하고 구현한 코드 샘플입니다.

이 폴더는 전체 Unity 프로젝트가 아니라 기업 제출용으로 발췌한 코드입니다. 각 샘플은 원본 경로 구조를 `Source/` 아래에 유지해서, 실제 프로젝트에서 어떤 위치와 역할을 가졌는지 확인할 수 있게 구성했습니다.

## Samples

### 1. Resource Lifecycle Facade

프로젝트 전체의 리소스 로드, 오브젝트 생성, 오브젝트 파괴 경로를 하나의 Facade API로 통합한 구조입니다.

초기에는 기능 코드에서 `Resources.Load`, `Instantiate`, `Destroy`를 직접 호출했지만, 일반 오브젝트, 풀링 오브젝트, 네트워크 오브젝트, Zenject 주입 오브젝트의 생명주기 정책이 섞이면서 책임이 분산되었습니다. 이를 해결하기 위해 외부 소비자는 `IResourcesServices`만 사용하고, 내부 구현은 Loader / Instantiator / Releaser로 분리했습니다.

### 2. NGO + Zenject Spawn Bridge

Unity Netcode for GameObjects가 직접 생성하는 `NetworkObject`에 Zenject DI를 적용하기 위한 생성/파괴 연결 구조입니다.

NGO의 `INetworkPrefabInstanceHandler`를 사용해 네트워크 prefab의 생성과 파괴를 가로채고, 생성 시 `DiContainer.InjectGameObject`를 적용합니다. 이후 네트워크 오브젝트 풀과 연결해 반복 생성되는 네트워크 오브젝트의 비용도 줄였습니다.

### 3. Skill Pipeline

ScriptableObject 기반 스킬 데이터를 런타임에서 Trigger / Targeting / Sequence / Decorator / Effect 단계로 실행하는 파이프라인입니다.

스킬마다 별도 클래스를 계속 늘리는 대신, 데이터 조합과 전략 구현체를 통해 스킬 실행 흐름을 재사용할 수 있도록 만들었습니다.

## Review Order

1. `01_ResourceLifecycleFacade`
2. `02_NGO_ZenjectSpawnBridge`
3. `03_SkillPipeline`

각 샘플의 세부 설명과 추천 진입점은 해당 폴더의 `README.md`에 정리되어 있습니다.

