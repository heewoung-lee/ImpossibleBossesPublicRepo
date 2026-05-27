# Resource Lifecycle Facade

## Background

이 프로젝트에서 가장 핵심이 되는 샘플입니다.

초기에는 기능 코드에서 `Resources.Load`, `Instantiate`, `Destroy`를 직접 호출했습니다. 하지만 프로젝트가 커지면서 일반 오브젝트, 풀링 오브젝트, 네트워크 오브젝트, Zenject 주입 오브젝트가 섞였고, 생성/파괴 정책이 코드 곳곳에 흩어졌습니다.

이 문제를 해결하기 위해 외부에는 단순한 `IResourcesServices` API만 제공하고, 내부 구현은 역할별로 분리했습니다. 이 구조를 만들면서 Facade 패턴이 단순히 클래스를 감싸는 방식이 아니라, 소비자와 내부 복잡도를 분리하는 경계라는 점을 배웠습니다.

## Goal

- 소비자는 `IResourcesServices`만 의존한다.
- 로드, 생성, 파괴 책임은 내부 구현체로 분리한다.
- Zenject factory, pooling, network despawn 같은 세부 정책은 소비자 코드 밖에 둔다.
- 나중에 로딩 방식이나 생성 정책을 바꾸더라도 영향 범위를 줄인다.

## Design

- `ResourceManager`
  - Facade 역할만 담당합니다.
  - `IResourcesLoader`, `IInstantiate`, `IDestroyObject`, `IFactoryManager`에 실제 작업을 위임합니다.

- `ResourcesLoader`
  - 리소스 로드 책임만 담당합니다.

- `Instantiator`
  - key 기반 prefab 생성, 직접 prefab 생성, component 추가를 담당합니다.
  - 등록된 Zenject factory가 있으면 factory 경로를 사용하고, 없으면 기본 생성 경로를 사용합니다.
  - 외부 코드는 생성 방식의 차이를 몰라도 됩니다.

- `ObjectReleaser`
  - 일반 `GameObject`, `Poolable`, `NetworkObject`의 반환/파괴 경로를 중앙에서 결정합니다.
  - 지연 파괴 요청이 다시 들어왔을 때 이전 예약을 취소하고 최신 요청을 기준으로 처리합니다.

- `ZenjectFactoryManager`
  - prefab과 factory creator를 매핑합니다.
  - `Instantiator`가 prefab별 생성 전략을 선택할 수 있게 합니다.

- `ResourcesLoaderInstaller`
  - Facade와 내부 구현체를 Zenject로 바인딩합니다.

## Review Entry Points

1. `Source/Assets/Scripts/GameManagers/ResourcesExManagement/IResourcesServices.cs`
2. `Source/Assets/Scripts/GameManagers/ResourcesExManagement/ResourceManager.cs`
3. `Source/Assets/Scripts/GameManagers/ResourcesExManagement/implementation/Instantiator.cs`
4. `Source/Assets/Scripts/GameManagers/ResourcesExManagement/implementation/ObjectReleaser.cs`
5. `Source/Assets/Scripts/ZenjectContext/ProjectContextInstaller/ResourcesLoaderInstaller.cs`

## Included Scripts

- `ResourceManager.cs`
- `IResourcesServices.cs`
- `IResourcesLoader.cs`
- `IInstantiate.cs`
- `IDestroyObject.cs`
- `IFactoryCreator.cs`
- `IDefaultGameObjectFactory.cs`
- `ICachingObjectDict.cs`
- `IRegisteredFactoryObject.cs`
- `CachingObjectDictManager.cs`
- `ResourcesLoader.cs`
- `Instantiator.cs`
- `ObjectReleaser.cs`
- `ZenjectFactoryManager.cs`
- `ResourcesLoaderInstaller.cs`

