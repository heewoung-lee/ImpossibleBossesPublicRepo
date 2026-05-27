# Impossible Bosses - 코드 샘플

Unity / C# / Zenject / Netcode for GameObjects 기반 협동 보스전 프로젝트에서 직접 설계하고 구현한 코드 샘플입니다.

이 폴더는 전체 Unity 프로젝트 중 창작자 개인이 생각하는 중요한 코드들을 발췌해서 기재했습니다.
각 샘플은 원본 경로 구조를 `Source/` 아래에 유지해서, 실제 프로젝트에서 어떤 위치와 역할을 가졌는지 확인할 수 있게 구성했습니다.

## 샘플

### 1. Resource Lifecycle Facade

프로젝트 전체의 리소스 로드, 오브젝트 생성, 오브젝트 파괴 경로를 하나의 Facade API로 통합한 구조입니다.

초기에는 기능 코드에서 `Resources.Load`, `Instantiate`, `Destroy`를 직접 호출했지만, 일반 오브젝트, 풀링 오브젝트, 네트워크 오브젝트, Zenject 주입 오브젝트의 생명주기 정책이 섞이면서 책임이 분산되었습니다. 이를 해결하기 위해 외부 소비자는 `IResourcesServices`만 사용하고, 내부 구현은 Loader / Instantiator / Releaser로 분리했습니다.

### 2. NGO + Zenject Spawn Bridge

Unity Netcode for GameObjects와 Zenject의 생성 생명주기가 맞지 않아 의존성 주입 시점 문제가 있었고, 그로 인해 두 시스템을 연결해주는 역할이 필요해 작성한 생성/파괴 연결 구조입니다.

특히 NGO 오브젝트는 spawn 전에 부모 transform이 변경되면 안 되는데, Zenject 생성 경로가 부모를 설정하면서 네트워크 오브젝트의 spawn 흐름과 충돌하는 문제가 있었습니다.

이를 해결하기 위해 NGO prefab handler에서 생성 직후 주입을 수행하고, 네트워크 spawn 이후 NGO 방식으로 parent를 설정했습니다. 또한 클라이언트는 씬/주입 초기화 완료 후 ready 신호를 보내고, 서버는 준비된 클라이언트에게 `NetworkShow`로 오브젝트를 표시하도록 구성했습니다.

### 3. Skill Pipeline

ScriptableObject 기반 스킬 데이터를 런타임에서 Trigger / Targeting / Sequence / Decorator / Effect 단계로 실행하는 파이프라인입니다.

스킬마다 별도 클래스를 계속 늘리는 대신, 데이터 조합과 전략 구현체를 통해 스킬 실행 흐름을 재사용할 수 있도록 만들었습니다.

## 검토 순서

1. `01_ResourceLifecycleFacade`
2. `02_NGO_ZenjectSpawnBridge`
3. `03_SkillPipeline`

각 샘플의 세부 설명과 추천 진입점은 해당 폴더의 `README.md`에 정리되어 있습니다.

