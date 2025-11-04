
<img width="1536" height="1024" alt="1titleImage" src="https://github.com/user-attachments/assets/66f55318-9bdb-498f-847a-7edc3d3aa09d" />

## 📘 목차
- **[프로젝트 소개](#-프로젝트-소개)**
- **[사용 기술](#-사용-기술)**
- **[핵심 로직](#-핵심-로직)**

<br/>

---

## 🔍 프로젝트 소개
**ImpossibleBosses**는 **Unity 6** 기반으로 개발된 **멀티플레이 PvE 보스 레이드 게임**입니다.  
플레이어는 최대 **8명**까지 팀을 이루어 거대한 보스 몬스터에 도전하며,  
**아처(Archer)**, **파이터(Fighter)**, **메이지(Mage)**, **몽크(Monk)**, **네크로맨서(Necromancer)** 중  
총 **5개 클래스** 중 하나를 선택할 수 있습니다.

보스의 다양한 공격 패턴을 팀원 간 **협동과 전략**으로 공략하며,  
전투를 통해 **아이템을 획득**하고, **장비를 교체**하면서  
플레이어는 점차 강해져 **최종 보스를 처치하는 것**이 목표입니다.

<br/>

---

## 🔧 사용 기술
| 구분 | 기술명 |
| :-- | :-- |
| **Engine** | `Unity 6` |
| **Networking** | `Unity Netcode` |
| **Voice & Chat** | `Vivox Service` |
| **Auth / DB** | `Google OAuth 2`, `Google Spreadsheet` |
| **AI Behavior** | `Behaviour Tree Designer` |
| **Test Tool** | `Unity Play Mode Scenarios` |
| **Frame work** | `Zenject` |
| **Inspector Customization** | `Odin Inspector` |

<br/>

---

## 🗝 핵심 로직


### 💉 의존성 주입 (Zenject)

> 프로젝트 전반의 객체 의존성을 체계적으로 관리하고, 코드의 결합도를 낮춰 유지보수성과 확장성을 높이기 위해 **Zenject DI 프레임워크**를 사용했습니다.


**주요 구현 방식:**

1.  **Installer를 통한 의존성 설정**:
    * `ProjectContext`, `SceneContext`, `GameObjectContext` 등 계층별로 어떤 의존성을 주입할지 정의합니다. 이를 통해 의존성 주입 설정이 한곳에 모여있어 전체적인 구조 파악이 용이합니다.
    
2.  **Unity Netcode 호환성을 위한 커스텀 팩토리 및 핸들러 구현**:
    * 프로젝트 초기, 젠젝트의 기본 객체 생성(스폰) 방식은 유니티 넷코드(Unity Netcode)의 동작 방식과 충돌을 일으켰습니다.
    * 젠젝트는 의존성 주입 과정에서 객체의 부모 트랜스폼을 네트워크 스폰이 완료되기 전에 변경하여, 넷코드의 '스폰 전까지 부모를 수정하면 안 된다'는 규칙을 위반하는 문제가 있었습니다.
    * 이 문제를 해결하기 위해, 젠젝트의 커스텀 팩토리(Custom Factory)와 유니티 네트워크 오브젝트의 `INetworkPrefabInstanceHandler`를 함께 사용하는 방식을 구현했습니다.



**기대 효과:**

* **유지보수성 향상**: 객체 간의 결합도가 낮아져 하나의 코드를 수정했을 때 다른 코드에 미치는 영향이 최소화됩니다.
* **테스트 용이성**: 인터페이스 기반으로 의존성을 주입하는게 용이 하므로, 실제 객체 대신 테스트용 **모의 객체(Mock Object)** 를 쉽게 주입하여 단위 테스트를 진행할 수 있습니다.
* **작업 속도 개선**: 의존성 주입 경로가 명확해지고, 객체 생성 및 연결에 대한 고민이 줄어들어 개발자가 핵심 로직 구현에 더 집중할 수 있습니다.
<br/>
<br/>

### 🔐 로그인 (Google 계정 연동)

> Google 계정을 통해 간편하게 로그인하고, 게임 데이터를 스프레드시트로 관리합니다.

1.  **로그인 화면 로드**: 게임 시작 시 로그인 또는 회원가입을 할 수 있는 화면이 표시됩니다.
2.  **회원가입 시**:
    * 사용자로부터 희망 ID와 비밀번호를 입력받습니다.
    * 입력된 ID가 기존에 사용 중인지 데이터베이스(Google 스프레드시트의 `UserAuthenticateData` 시트)에서 확인 후, 사용 가능하면 새 계정 정보를 저장합니다.
    * 이후 닉네임 설정 화면을 통해 플레이어가 사용할 고유한 닉네임을 입력받아 저장합니다.
  
<br/>

<table style="width:100%; border:0;">
  <tr>
    <td align="center" valign="top" style="width:50%;">
      <figure style="margin:0;">
        <img src="https://github.com/user-attachments/assets/fb894553-6fd5-42c1-bb28-56304c32567f" alt="이미 가입된 ID" height="300">
        <figcaption>
          <br/>
          <strong>&lt;이미 가입된 ID&gt;</strong><br>
        </figcaption>
      </figure>
    </td>
    <td align="center" valign="top" style="width:50%;">
      <figure style="margin:0;">
        <img src="https://github.com/user-attachments/assets/0cf5fc9e-3f29-4c95-a0f2-cbecd03b1551" alt="회원가입 성공" height="300">
        <figcaption>
          <br/>
          <strong>&lt;회원가입 성공&gt;</strong><br>
        </figcaption>
      </figure>
    </td>
  </tr>
</table>

<br/>



3.  **로그인 시**:
    * 입력된 ID와 비밀번호를 데이터베이스(Google 스프레드시트의 `UserAuthenticateData` 시트)에 저장된 정보와 비교하여 인증을 시도합니다.
    * 이 과정에서 Google의 인증 방식(OAuth 2.0) 및 스프레드시트 접근 기술이 데이터 관리 시스템과 연동되어 사용됩니다.
4.  **인증 결과에 따른 처리**:
    * **성공**: 플레이어의 계정 정보가 게임 내에 임시 저장됩니다. 만약 해당 계정에 닉네임이 설정되어 있지 않다면, 닉네임 설정 화면으로 안내합니다. 모든 정보가 확인되면 로비 화면으로 이동합니다.
    * **실패**: ID 또는 비밀번호 불일치, 인터넷 연결 문제 등의 오류 발생 시 알림창을 통해 사용자에게 상황을 안내합니다.

**주요 기술:** Google 계정 인증(OAuth 2.0), Google 스프레드시트를 활용한 데이터 관리.

<br/>
<p align="center">
  <img src="https://github.com/user-attachments/assets/331d4e07-1ed3-4bfa-9e96-e09b0e97b034" alt="로그인 흐름" width="70%"/>
  <br/>
  <sub><strong>&lt;로그인 및 회원가입 처리 흐름도&gt;</strong></sub>
</p>
<br/>

---


### 📥 데이터 불러오기
> ImpossibleBosses의 데이터 관리는 Managers.DataManager를 중심으로 이루어집니다. 이 매니저는 Google 스프레드시트에서 게임에 필요한 각종 데이터(플레이어 정보, 몬스터 정보, 아이템 목록 등)를 로드하고, 애플리케이션 내에서 효율적으로 사용할 수 있도록 처리합니다.

<p align="center">
  <strong>&lt;데이터 시트&gt;</strong>
</p>
<p align="center">
  <img src="https://github.com/user-attachments/assets/e48f121f-4644-4756-897d-ed95fe725d19" alt="데이터 시트 이미지 1" width="70%"/>
  <img src="https://github.com/user-attachments/assets/07f5d705-665a-4e44-9cb9-4da816dc005d" alt="데이터 시트 이미지 2" width="70%"/>
  <img src="https://github.com/user-attachments/assets/7b68f474-5819-414b-a1e7-6140bab15e9a" alt="데이터 시트 이미지 3" width="70%"/>
</p>


**데이터 로딩 절차:**

1.  **초기화 및 타입 스캔**:
    <ul>
      <li><code>Managers.DataManager.Init()</code> 메서드가 데이터 로딩을 시작합니다.</li>
      <li><code>LoadSerializableTypesFromFolder</code> 메서드는 지정된 경로에서 <code>[Serializable]</code> 어트리뷰트를 가진 클래스들을 리플렉션으로 스캔합니다. 이 클래스들은 스프레드시트의 각 시트 데이터 구조와 매핑됩니다.</li>
    </ul>

<p align="center">
  <strong>&lt;DataManger의 타입확인&gt;</strong>
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/0c779137-163d-42ea-9b53-06abfecbeba5" alt="타입 스캔 이미지 1" width="70%"/>
</div>
<br>


2.  **Google 스프레드시트 연동**:
    <ul>
      <li><code>DatabaseStruct</code>는 Google OAuth 2.0 인증 정보(클라이언트 ID, 시크릿 코드, 애플리케이션 이름, 스프레드시트 ID)를 관리합니다.</li>
      <li><code>GetGoogleSheetData()</code> 메서드는 이 정보를 사용하여 Google Sheets API 인증 후, 지정된 스프레드시트 데이터를 가져옵니다.</li>
    </ul>

<p align="center">
  <strong>&lt;구글 스프레드시트 불러오기&gt;</strong>
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/fc9c65e6-c48b-4b4b-ae1d-600aae8696a6" alt="스프레드시트 연동" width="70%"/>
</div>
<br>


3.  **데이터 파싱 및 구조화**:
    * `LoadDataFromGoogleSheets()`는 인증된 서비스와 스프레드시트 ID로 각 시트의 데이터를 요청합니다.
    * `ParseSheetData()`는 시트 데이터를 JSON 형식 문자열로 변환합니다.
    * `AddAllDataDictFromJsonData()`는 JSON 문자열을 C# 객체로 역직렬화합니다.
        * `GetTypeNameFromFileName()`은 시트 이름에서 데이터 타입을 결정합니다.
        * `FindGenericKeyType()`은 데이터 타입이 `Ikey<TKey>` 인터페이스를 구현했는지 확인하여 딕셔너리 키 타입을 결정합니다.
        * `DataToDictionary<TKey, TStat>` 클래스는 로드된 데이터 리스트를 `Dictionary<TKey, TStat>` 형태로 변환하여 `AllDataDict`에 저장합니다.

<p align="center">
  <strong>&lt;JSON 문자열 역직렬화&gt;</strong>
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/26519a6e-e5ef-41a9-901d-af0590b3070f" alt="데이터 파싱" width="70%"/>
</div>
<br>

4.  **데이터 캐싱 및 접근**:
    * 처리된 데이터는 `DataManager.AllDataDict` (`Dictionary<Type, object>` 타입)에 데이터 타입별로 캐싱되어, 게임 내 다른 시스템에서 사용됩니다.
    * `ItemDataManager`는 `DataManager.AllDataDict`에서 아이템 관련 타입의 데이터를 가져와 관리합니다.
<p align="center">
  <strong>&lt;아이템 클래스의 데이터 캐싱&gt;</strong>
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/d0669e1f-c4c7-407f-a049-a5153eda783a" alt="데이터 캐싱" width="70%"/>
</div>
<br>

5.  **로컬 데이터 활용**:
    * Google 스프레드시트 접근 불가 시, `LoadAllDataFromLocal()` 메서드가 로컬에 JSON 파일로 저장된 데이터를 로드합니다.
    * 스프레드시트에서 새 데이터를 가져오면, `SaveDataToFile()` 메서드가 기존 로컬 데이터와 비교 후 변경된 경우 최신 데이터로 덮어씁니다. `BinaryCheck<T>()`가 데이터 변경 여부를 확인합니다.
<p align="center">
  <strong>&lt;데이터 변경 확인(바이너리비교)&gt;</strong>
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/151fc2ba-32ac-4d26-a628-5fdf6439ea1d" alt="데이터 변경확인" width="30%"/>
</div>
<br>

---


### 🏠 로비 (Lobby)

> 플레이어는 계정 인증 후 **로비 화면**으로 이동하여, 다른 플레이어와 소통하고 함께 게임을 즐길 **방을 찾거나 만들 수 있습니다.**

<br/>

<p align="center">
  <strong>&lt;로비화면&gt;</strong>
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/cb47f1e7-d994-4a03-8962-1257cdcb8bc7" alt="로비화면" width="70%"/>
</div>

<br/>

#### 🚪 로비 입장 및 준비 과정

게임에 접속하면 가장 먼저 로비로 입장하기 위한 준비를 시작합니다.

* **서비스 연결 및 인증**: Unity에서 제공하는 온라인 서비스에 연결하고, 플레이어마다 고유한 ID를 받아옵니다.
* **중복 접속 확인**: 혹시 이미 다른 곳에서 같은 계정으로 접속 중인지 확인하여, 중복 접속을 막습니다.
* **대기 로비 참가**: 모든 준비가 끝나면, 다른 플레이어들과 함께 머무르며 방을 탐색하거나 생성할 수 있는 '대기 로비' 공간에 자동으로 들어가게 됩니다. 만약 아무도 없는 첫 접속이라면, 새로운 대기 로비가 만들어집니다.

<br/>

#### 💬 플레이어 간 소통 (채팅)

로비에서는 다른 플레이어들과 실시간으로 대화할 수 있는 채팅 기능이 제공됩니다.

* Vivox 서비스를 이용하여 텍스트 채팅을 지원합니다.
* 이를 통해 함께 게임 할 파티원을 구하거나, 게임에 대한 정보를 나누는 등 다양한 상호작용이 가능합니다.

<p align="center">
  <img src="https://github.com/user-attachments/assets/7efb6baa-1ff3-4d0f-a9ba-d496d7999fae" alt="로비 채팅" width="80%"/>
  <br/>
  <sub><strong>&lt;로비 채팅창&gt;</strong><br/>다른 플레이어와 텍스트로 대화할 수 있습니다. (Vivox 연동)</sub>
</p>

<br/>

#### 🔍 게임 방 탐색 및 참가

다른 플레이어가 만들어 놓은 게임 방을 찾아 참여할 수 있습니다.

* 현재 생성되어 있는 공개 게임 방들의 목록이 실시간으로 표시됩니다.
* 새로고침 버튼을 통해 언제든지 최신 방 목록을 불러올 수 있습니다.
* 목록에서 원하는 방을 선택하고 '참가' 버튼을 누르면 해당 방으로 입장합니다.
* 만약 선택한 방에 비밀번호가 설정되어 있다면, 올바른 비밀번호를 입력해야만 들어갈 수 있습니다.

<br/>

<table style="width:100%; border:0;">
  <tr>
    <td align="center" valign="top" style="width:50%;">
      <figure style="margin:0;">
        <img src="https://github.com/user-attachments/assets/6c405fff-d585-4e5c-9c8e-433363d4fe30" alt="방 목록" height="300">
        <figcaption>
          <br/>
          <strong>&lt;게임 방 목록&gt;</strong><br>
          현재 참여 가능한 방들의 이름,
           <br/>
          인원수 등의 정보를 보여줍니다.
        </figcaption>
      </figure>
    </td>
    <td align="center" valign="top" style="width:50%;">
      <figure style="margin:0;">
        <img src="https://github.com/user-attachments/assets/0322b79d-21ea-40c1-b096-1965cb316663" alt="비밀번호 입력" height="300">
        <figcaption>
          <br/>
          <strong>&lt;비밀번호 입력창&gt;</strong><br>
          비공개 방에 참여하기 위해 비밀번호를 입력하는 화면입니다.
        </figcaption>
      </figure>
    </td>
  </tr>
</table>

<br/>


#### ➕ 게임 방 생성

원한다면 직접 새로운 게임 방을 만들 수도 있습니다.

* 방 만들기 화면에서 만들고 싶은 방의 이름과 최대 참가 가능 인원 수를 설정합니다.
* 다른 플레이어들이 함부로 들어오지 못하도록 비밀번호를 설정할 수도 있습니다.
* 설정을 완료하고 방을 만들면, 이 방은 다른 플레이어들의 방 목록에도 나타나 함께 플레이할 팀원을 모을 수 있습니다.

<p align="center">
  <img src="https://github.com/user-attachments/assets/8a3ca177-2405-4a81-b735-8c58dca66a54" alt="방 생성" width="30%"/>
  <br/>
  <sub><strong>&lt;게임 방 생성 설정&gt;</strong><br/>새로운 방의 이름, 최대 인원, 비밀번호 등을 설정합니다.</sub>
</p>

<br/>

#### ⚔️ 캐릭터 선택 및 게임 준비

성공적으로 게임 방에 들어가면, 플레이어는 자신이 플레이할 캐릭터를 선택하고 게임 시작을 준비합니다.

* 다양한 클래스 중 원하는 캐릭터를 선택합니다.
* 같은 방에 있는 다른 플레이어들이 어떤 캐릭터를 골랐는지, 게임을 시작할 준비가 되었는지 실시간으로 확인할 수 있습니다.
* 모든 플레이어가 "준비 완료" 상태가 되면, 방을 만든 방장이 게임을 시작할 수 있습니다.

<p align="center">
  <img src="https://github.com/user-attachments/assets/efe05ffa-fa0c-42ff-b05d-7f6aebfa46e8" alt="캐릭터 선택" width="80%"/>
  <br/>
  <sub><strong>&lt;캐릭터 선택 및 준비 완료&gt;</strong><br/>방에 참가한 플레이어들이 각자 플레이할 캐릭터를 고르고 "준비" 상태를 표시합니다.</sub>
</p>

<br/>

**로비 시스템의 안정성 유지**:

* 방을 만든 플레이어(호스트)는 방이 갑자기 사라지지 않도록 주기적으로 서버에 "방이 아직 살아있음!"이라는 신호(하트비트)를 보냅니다.
* 플레이어가 방에 새로 들어오거나 나가는 등의 변화는 즉시 모든 참가자에게 알려져 화면이 업데이트됩니다.
  
---

<br/>

### 🔗 릴레이 서버 (Relay Server)

> 플레이어 간 직접적인 P2P 연결의 어려움을 해결하고 안정적인 멀티플레이 환경을 제공하기 위해 Unity Relay 서비스를 사용합니다. 이를 통해 별도의 서버 구축 없이도 원활한 게임 연결을 지원합니다.

#### 🛠️ 주요 구현 내용

* **호스트 마이그레이션 (Host Migration)**:
    * 로비 시스템과 연동하여, 기존 호스트가 게임에서 나가면 새로운 호스트가 릴레이 서버의 할당정보를 이어받아 게임 세션을 계속 유지할 수 있도록 설계되었습니다.
    * 이는 로비 매니저에서 호스트 변경 이벤트를 감지하고, 새로운 호스트에게 릴레이 서버 재설정 권한을 부여하는 방식으로 처리됩니다.
      
<p align="center">
  <img src="https://github.com/user-attachments/assets/5cd90617-aae4-4045-84e2-939418a27c05" alt="캐릭터 선택" width="80%"/>
  <br/>
  <sub><strong>&lt;호스트 이전(호스트 마이그레이션)&gt;</strong><br/>호스트가 방을 떠나면 다른 플레이어가 호스트를 위임 받습니다.</sub>
</p>

* **릴레이 데이터와 로비 데이터 연동**:
    * 호스트가 릴레이 서버에 성공적으로 방을 할당받으면, 생성된 참여 코드(Join Code)는 로비 데이터의 일부로 저장됩니다.
    * 이를 통해 다른 클라이언트들이 로비에서 방 정보를 보고, 해당 참여 코드를 사용하여 릴레이 서버에 접속할 수 있도록 합니다.
      
<br/>

* **오브젝트 동기화**:
   * 릴레이 서버는 게임 로직을 직접 처리하기보다 데이터 중계에 집중합니다. 따라서 게임 내 오브젝트의 상태 동기화는 호스트(방장)가 주도하며, 관련된 모든 데이터는 릴레이 서버를 거쳐 각 클라이언트에게 전달됩니다.

   * 동기화는 기본적으로 방을 만든 사람(호스트)이 게임 내 대부분의 중요한 결정, 예를 들어 캐릭터의 움직임이나 특정 사건의 발생 등을 내리고, 그 결과를 다른 참여자들에게 릴레이 서버를 통해 전달하는 방식으로 이루어집니다.

   * 전달되는 정보에는 캐릭터나 중요 물체들의 실시간 위치와 방향, 현재 취하고 있는 행동이나 모습, 그리고 체력이나 점수 같은 핵심 데이터들이 포함됩니다. 또한, 게임 도중에 새롭게 나타나거나 사라지는 요소들(예: 몬스터의 등장, 마법 효과의 시작과 끝) 역시 방장의 통제 하에 모든 참여자에게 일관되게 반영되며, 순간적인 기술 사용 같은 특별한 행동들도 이 통로를 통해 즉시 공유됩니다.

   * 모든 참여자는 네트워크 환경의 차이나 지연에도 불구하고 최대한 동일한 게임 상황을 경험하게 됩니다. 방장이 게임의 중심 상태를 관리하고 릴레이 서버가 이를 효율적으로 전달함으로써, 함께 플레이하는 경험의 일관성을 높이고 혼란을 최소화합니다.

<br/>



<p align="center">
  <img src="https://github.com/user-attachments/assets/05cd0d58-a25e-4a4f-9438-1c7f441d54b4" alt="이동 동기화" width="80%"/>
  <br/>
  <sub><strong>&lt;이동동기화: 캐릭터의 실시간 위치, 방향, 상태를 공유합니다.&gt</strong></sub>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/525b5789-ae1a-4c74-8a25-51425d417f59" alt="오브젝트 동기화" width="80%"/>
  <br/>
  <sub><strong>&lt;오브젝트 동기화: 게임 내 중요 객체의 생성, 소멸, 상태 변화를 모든 플레이어에게 실시간으로 동일하게 반영합니다.&gt</strong></sub>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/1132884a-fedc-4d55-b9f7-8535dd485021" alt="스킬 동기화" width="80%"/>
  <br/>
  <sub><strong>&lt;이펙트 동기화: 모든 스킬의 발동, 시각적 표현을 실시간으로 동일하게 반영합니다.&gt</strong></sub>
</p>


<br/>


---
<br/>

### 🚀 문제 해결 및 기술 개선 사례

> 프로젝트를 진행하면서 다양한 기술적 문제에 직면했으며, 이를 해결하고 시스템을 개선하기 위해 다음과 같은 노력을 기울였습니다.

<br/>

#### 1. 상태 관리의 유연성 확보: 유한 상태 머신에서 전략 패턴으로

* 🤔 문제점:
    * 초기 플레이어 캐릭터의 상태(이동, 공격, 정지 등)를 유한 상태 머신(FSM) 방식으로 구현했으나, 새로운 상태를 추가하거나 기존 상태의 로직을 변경할 때 코드 수정 범위가 넓어지고 복잡도가 증가했습니다. 특히, 각 상태에 따른 애니메이션 전환 로직이 강하게 결합되어 유지보수가 어려웠습니다.

* 💡 해결 과정:
    * 이를 해결하고자 전략 패턴(Strategy Pattern)을 도입하여 각 상태를 독립적인 클래스(`IState` 인터페이스를 구현하는 형태)로 분리했습니다.
    * `BaseController` 클래스는 현재 상태 객체(`CurrentStateType`)를 통해 해당 상태의 로직을 실행하고, `StateAnimationDict`를 통해 상태 변경 시 적절한 애니메이션을 호출하도록 설계했습니다.
    * 이를 통해 각 상태의 행동 로직과 애니메이션 전환 로직을 캡슐화하고, 새로운 상태 추가 시 기존 코드에 미치는 영향을 최소화했습니다.

* ✨ 개선 결과:
    * 코드의 가독성과 확장성이 크게 향상되었습니다. 새로운 플레이어 스킬이나 행동 상태를 추가할 때, `IState`를 구현하는 새 클래스를 만들고 `BaseController`에 등록하는 것만으로 확장이 가능해졌습니다.
    * 각 상태 로직이 분리되어 테스트와 디버깅이 용이해졌습니다.

<div align="center">
   
<table style="border:0;">
  <tr>
    <td align="center" valign="top" style="width:50%;">
      <figure style="margin:0;">
        <img src="https://github.com/user-attachments/assets/74a769e2-88da-498b-ae74-19f7687137f4"
             alt="FSM(유한상태머신)" height="300">
        <figcaption>
          <br/>
          &lt;<strong>FSM(유한상태머신) 다이어그램</strong>&gt;<br>
        </figcaption>
      </figure>
    </td>
    <td align="center" valign="top" style="width:50%;">
      <figure style="margin:0;">
        <img src="https://github.com/user-attachments/assets/e342dd54-8c9d-43ad-90fb-b1b3c7602416"
             alt="전략패턴" height="300">
        <figcaption>
          <br/>
          &lt;<strong>전략패턴 다이어그램</strong>&gt;<br>
        </figcaption>
      </figure>
    </td>
  </tr>
</table>

</div>

#### 2. 보스 AI 확장성 개선: 유한 상태 머신에서 비헤이비어 트리로 전환

* 🤔 문제점:
    * 초기 보스 몬스터의 AI를 플레이어와 마찬가지로 유한 상태 머신(FSM)으로 구현했으나, 보스의 행동 패턴이 다양해지고 복잡해짐에 따라 상태 추가 및 전환 로직 관리가 어려워졌습니다. FSM은 복잡한 조건 분기나 병렬적인 행동 표현에 한계가 있었습니다.

* 💡 해결 과정:
    * 보스 AI 구현을 위해 **비헤이비어 트리(Behavior Tree)**를 도입했습니다. 보스의 다양한 행동(이동, 기본 공격, 스킬 사용, 특정 조건에 따른 패턴 변화 등)을 모듈화된 노드 형태로 설계했습니다.
    * 비헤이비어 트리를 통해 조건 확인, 행동 실행, 흐름 제어(시퀀스, 셀렉터 등)를 직관적으로 구성할 수 있게 되었습니다.
    * 각 행동 노드는 `BossGolemController` 및 `BossGolemNetworkController`와 상호작용하여 애니메이션, 네트워크 동기화 등을 처리합니다.

* ✨ 개선 결과:
    * 보스 AI의 복잡한 행동 패턴을 보다 체계적이고 시각적으로 관리할 수 있게 되었습니다.
    * 새로운 스킬이나 행동 패턴을 추가하거나 기존 패턴을 수정하는 작업이 훨씬 용이해졌으며, 다양한 조건에 따른 AI 반응을 쉽게 구현할 수 있게 되어 보스전의 깊이가 더해졌습니다.
    * AI 로직과 실제 행동 실행 코드가 분리되어 가독성과 유지보수성이 향상되었습니다.

<br/>

<p align="center">
  &lt;<strong>비헤이비어 트리</strong>&gt;
</p>
<div align="center">
  <img src="https://github.com/user-attachments/assets/e3d468e1-3853-4536-b5f1-bd971a6796f6" alt="비헤이비어 트리" width="70%"/>
</div>

<br/>

#### 3. 로비 콜백 및 데이터 동기화 문제 해결: Unity Lobby SDK 버그 식별 및 공식 해결

* 🤔 문제점:
    * Unity Netcode 기반 멀티플레이 게임 개발 중, Unity Lobby 서비스에서 특정 시나리오(호스트 이전 후 이전 호스트 재접속)에서 플레이어의 로비 참가 및 행동 변화에 따른 이벤트 콜백이 정상적으로 호출되지 않는 오류가 발생했습니다.
    * 이로 인해 다른 플레이어의 로비 내 활동이 실시간으로 반영되지 않고 로비 데이터가 불일치하는 등, 로비 시스템의 핵심 기능에 문제가 발생하여 사용자 경험을 저해했습니다.

* 💡 해결 과정:
    * **초기 검증 및 원인 분석:** 처음에는 턴→ 젠젝트(Zenject) DI 프레임워크로 전환

* 🤔 문제점:
    * 초기에 싱글톤으로 빠르게 기능을 붙였지만, 시간이 지나며 싱글톤 의존도가 커지고 객체가 비대화됨에 따라 하나를 수정하면 다른 곳에서 문제가 터지는 도미노 현상이 발생했습니다.
      이를 줄이기 위해 싱글톤을 분해하여 일반 DI 모듈을 만들고, 나머지는 컴포넌트 패턴으로 채워 갔으나, 의존성 주입 컨트롤러가 곳곳에 흩어져 주입 경로 추적과 테스트 교체가 어렵고 불안정했습니다.

* 💡 해결 과정:
    * Zenject를 도입하여 주입 지점을 Installer로 일원화하고(의존 모듈에 대한 가시성 향상), ProjectContext → SceneContext → GameObjectContext의 상향 단일 흐름으로 컴포지션 루트를 표준화했습니다.
    * 인터페이스 중심 바인딩과 환경별(프로덕션/테스트) Installer 분리로, 동일 코드에 Real/Mock을 손쉽게 교체할 수 있게 했습니다.

* ✨ 개선 결과:
    * 의존성 가시화: “무엇이 어디서 주입되는지”가 한눈에 보이며, 변경 영향 범위가 명확해졌습니다.
    * 유지보수성 향상: 결합도가 낮아져 수정·확장이 수월해졌고, 도미노 이슈가 크게 감소했습니다.
    * 작업속도 개선: 테스트에서는 Installer만 바꿔 끼우면 되므로 세팅 시간이 단축되고, 기능 개발과 디버깅 사이클이 빨라졌습니다.
    * 초기 학습곡선을 넘기기 어려웠지만 이후 생산성과 안정성이 눈에 띄게 향상되었습니다.

---


#### 7. 테스트 효율성 증대: 인스펙터 커스텀을 통한 Play Mode Scenario 제어
* 🤔 문제점:
    * Unity 6에서 도입된 Play Mode Scenario 기능은 멀티플레이 테스트를 매우 편리하게 만들어주었지만, 테스트 환경(클라이언트 수, 역할 태그 등)을 변경할 때마다 매번 Configure play mode Scenario 메뉴에 들어가 수동으로 수정해야 하는 불편함이 있었습니다.

* 💡 해결 과정:

   * 목표 설정: 인스펙터 내에서 직접 테스트 클라이언트 수와 각 클라이언트의 태그를 설정하고, 시나리오 모드 사용 여부를 토글할 수 있는 커스텀 에디터를 구현하는 것을 목표로 삼았습니다.
   * 구현 : Scenario 에셋 접근 및 수정: SerializedObject API를 사용하여 Scenario 에셋 파일의 직렬화된 필드(m_MainEditorInstance, m_EditorInstances 등)에 접근했습니다. 이를 통해 인스펙터에서 입력한 태그 값을 에셋에 직접 반영하는 기능을 구현했습니다.

* ✨ 개선 결과:

   * 멀티플레이 테스트 환경 설정을 위해 더 이상 메뉴를 찾아다닐 필요 없이, 인스펙터 내에서 모든 조작이 가능해졌습니다.
      클라이언트 수를 늘리거나 각 클라이언트에 'Host', 'Client' 등의 태그를 부여하는 작업이 매우 직관적이고 빨라져, 테스트 준비 시간이 획기적으로 단축되었습니다.
      이러한 워크플로우 개선은 디버깅 효율을 높이고, 개발자가 핵심 로직 개발에 더 집중할 수 있는 환경을 만들어주었습니다.

<br/>

<p align="center">
  <img src="https://github.com/user-attachments/assets/5cff0332-b284-4424-a231-e48705b2b16e" alt="커스텀 인스펙터 결과물" width="70%"/>
  <br/>
  <sub><strong>&lt; 인스펙터에서 테스트 환경 제어&gt;</strong></sub>
</p>
<p align="center">
  <a href=https://blog.naver.com/hiwoong12/223972484977">[개발일지] 커스텀 인스펙터 제작</a>
</p>
<br/>
