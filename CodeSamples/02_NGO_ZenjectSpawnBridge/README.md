# NGO + Zenject Spawn Bridge

## 배경

Unity Netcode for GameObjects와 Zenject의 생성 생명주기가 맞지 않아 의존성 주입 시점 문제가 있었습니다.

NGO는 네트워크 prefab을 자체 생성 경로로 spawn하기 때문에 일반적인 Zenject factory 생성 경로를 거치지 않았고, 그 결과 `MonoBehaviour`에 필요한 의존성이 주입되지 않는 문제가 생겼습니다.

가장 큰 문제는 NGO 오브젝트가 spawn되기 전에 부모 transform이 변경되면 안 되는 상황에서, Zenject 생성 경로가 부모를 설정하면서 네트워크 오브젝트의 spawn 흐름과 충돌한 점이었습니다.

이 샘플은 두 시스템을 연결해주는 역할이 필요해 작성한 생성/파괴 연결 구조입니다. NGO prefab handler에서 생성 직후 주입을 수행하고, 네트워크 spawn 이후 NGO 방식으로 parent를 설정하며, 클라이언트는 씬/주입 초기화 완료 후 ready 신호를 보내 `NetworkShow`로 오브젝트를 표시하도록 구성했습니다.

## 목표

- NGO가 spawn하는 `NetworkObject`에도 Zenject 의존성 주입을 적용한다.
- NGO spawn 이전의 parent 변경 충돌을 피하고, spawn 이후 NGO parent 설정 경로를 사용한다.
- 주입/씬 초기화가 끝난 클라이언트에게만 네트워크 오브젝트를 표시한다.
- 네트워크 오브젝트의 생성/파괴 경로를 프로젝트의 리소스/풀링 정책과 연결한다.

## 설계

- `NgoZenjectHandler`
  - NGO의 `INetworkPrefabInstanceHandler` 구현체입니다.
  - `Instantiate`에서 prefab을 생성한 뒤 `DiContainer.InjectGameObject`를 호출합니다.
  - `Destroy`에서 `IResourcesServices.DestroyObject`로 파괴 경로를 위임합니다.

- `NgoZenjectFactory`
  - 네트워크 prefab을 factory manager에 등록합니다.
  - NGO prefab handler에도 같은 prefab을 등록해 NGO spawn 경로와 Zenject 생성 경로를 연결합니다.

- `NetworkObjectPool`
  - NGO용 object pool입니다.
  - pooled prefab handler를 등록해 spawn/despawn 시 pool get/release가 호출되게 합니다.

- `NgoPoolingInitializeBase`
  - poolable NGO prefab이 가져야 하는 공통 초기화/수명 주기 기반 클래스입니다.

- `NgoPoolManager`
  - NGO pool 등록, pool root 관리, prewarm/expand 흐름을 담당합니다.

## 검토 진입점

1. `Source/Assets/Scripts/NetWork/NGO/NgoZenjectHandler.cs`
2. `Source/Assets/Scripts/ZenjectContext/GameObjectContext/GameObjectContextFactory.cs`
3. `Source/Assets/Scripts/NetWork/NGO/NetworkObjectPool.cs`
4. `Source/Assets/Scripts/GameManagers/NGOPoolManagement/NgoPoolManager.cs`
5. `Source/Assets/Scripts/NetWork/BaseNGO/NgoPoolingInitializeBase.cs`

## 포함된 스크립트

- `NgoZenjectHandler.cs`
- `GameObjectContextFactory.cs`
- `NgoZenjectHandlerInstaller.cs`
- `ISpawnBehavior.cs`
- `NetworkObjectPool.cs`
- `NgoPoolingInitializeBase.cs`
- `NgoPoolManager.cs`
- `INgoPooldata.cs`
- `INetworkObjectGetter.cs`
- `NetworkObjectGetter.cs`
- `DynamicNetworkObjectGetter.cs`
- `NgoPoolManagerInstaller.cs`

