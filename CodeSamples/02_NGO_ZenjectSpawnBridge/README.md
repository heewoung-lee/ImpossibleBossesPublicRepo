# NGO + Zenject Spawn Bridge

## Background

Unity Netcode for GameObjects는 네트워크 prefab을 자체 생성 경로로 spawn합니다. 이때 일반적인 Zenject factory 생성 경로를 거치지 않으면, `MonoBehaviour`에 필요한 의존성이 주입되지 않는 문제가 생겼습니다.

이 샘플은 NGO의 prefab 생성/파괴 흐름과 Zenject DI를 연결하기 위해 작성한 구조입니다.

## Goal

- NGO가 spawn하는 `NetworkObject`에도 Zenject 의존성 주입을 적용한다.
- 네트워크 오브젝트의 생성/파괴 경로를 프로젝트의 리소스/풀링 정책과 연결한다.
- 반복 생성되는 네트워크 오브젝트는 NGO pool을 통해 재사용한다.

## Design

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

## Review Entry Points

1. `Source/Assets/Scripts/NetWork/NGO/NgoZenjectHandler.cs`
2. `Source/Assets/Scripts/ZenjectContext/GameObjectContext/GameObjectContextFactory.cs`
3. `Source/Assets/Scripts/NetWork/NGO/NetworkObjectPool.cs`
4. `Source/Assets/Scripts/GameManagers/NGOPoolManagement/NgoPoolManager.cs`
5. `Source/Assets/Scripts/NetWork/BaseNGO/NgoPoolingInitializeBase.cs`

## Included Scripts

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

