# 🗺️ PuzleBattleGame 프로젝트 구조 및 파일 맵핑 (Project Map)

이 문서는 시스템의 각 기능이 실제 프로젝트 폴더와 파일에 어떻게 매핑되어 있는지 설명합니다. 코드를 찾거나 버그를 분석할 때 이 맵을 기준으로 탐색 위치를 즉시 결정하십시오.

---

## 📂 1. 핵심 폴더 구조 (Assets 기준)
- `01_Scenes/`: 게임의 모든 씬 파일 (.unity)
- `02_Scripts/`: C# 소스 코드
- `03_Prefab/`: 유니티 프리팹 (UI, 퍼즐 블럭 등)
- `04_Resources/`: 이미지(Sprite), 사운드 등 실제 에셋
- `05_Table/`: 게임의 규칙과 스테이지를 정의하는 JSON 데이터

---

## 🧩 2. 인게임 (퍼즐 코어) 아키텍처 맵핑
인게임 전투 및 퍼즐 로직은 철저한 **MVC 패턴**으로 분리되어 있으며 `Assets/02_Scripts/PuzzleCore/` 하위에 위치합니다.

### 🧠 Model (순수 C# 논리, 유니티 독립적)
**위치:** `Assets/02_Scripts/PuzzleCore/Module/`
- **역할**: 퍼즐 보드의 상태, 블럭 매칭 로직, 점수 계산, 데이터 관리. 유니티 엔진 객체(`GameObject`, `MonoBehaviour`)를 절대 직접 참조하지 않음.
- **주요 탐색 대상**: 
  - `IPuzzleBoard`, `ThreeMatchPuzzleBoard` 등 보드 로직
  - `PuzzleBlock`, `ISwappableBlock` 등 블럭 데이터 및 인터페이스
  - `BoardViewAction` (상태 변화 후 뷰로 전달되는 연출 데이터 규격)

### 👁️ View (유니티 시각적 연출 및 애니메이션)
**위치:** `Assets/02_Scripts/PuzzleCore/View/`
- **역할**: Model에서 생성된 `BoardViewAction` 이벤트(이동, 파괴 등)를 수신하여 실제 유니티 화면에 애니메이션과 이펙트를 렌더링. 로직 판단은 하지 않음.
- **주요 탐색 대상**: 
  - `PuzzleBoardView`, `PuzzleBlockView`

### 🎮 Controller (사용자 입력 및 게임 루프 관리)
**위치:** `Assets/02_Scripts/PuzzleCore/Controller/`
- **역할**: 유저의 터치/드래그 입력을 감지하고 큐(Queue)에 저장. 틱(Tick) 단위로 Model의 Update를 호출하고 결과를 View로 전달.
- **주요 탐색 대상**: 
  - `PuzzleGameController` (메인 루프 및 상태 머신)
- **물리/입력 감지 (Collider 연동)**: 
  - `Assets/02_Scripts/PuzzleCore/PuzzleBlockCollider.cs` 등 (유니티 `OnMouseDown` 등의 이벤트를 컨트롤러로 전달)

---

## ⚙️ 3. 시스템 및 매니저 맵핑
게임 전반을 관리하는 시스템 코드는 주로 `Assets/02_Scripts/Manager/`에 위치합니다.

- **에셋 및 리소스 로드**: `Assets/02_Scripts/Manager/AssetManager.cs` (Addressables 기반 비동기/동기 로드 및 캐싱)
- **데이터 주입 및 준비**: `Assets/02_Scripts/StageInjection.cs` (JSON 데이터를 파싱하여 인게임 `GameSpec` 객체로 변환)
- **오브젝트 풀링**: `Assets/02_Scripts/Manager/PoolManager.cs` (퍼즐 블럭 및 이펙트 생성/삭제 최적화)
- **팝업 및 UI 관리**: `Assets/02_Scripts/Manager/PopupManager.cs`

---

## 📊 4. 데이터 (JSON Table) 맵핑
게임의 규칙과 맵 디자인은 코드가 아닌 데이터 주도(Data-Driven) 방식으로 설계되었습니다.
**위치:** `Assets/05_Table/`

- **규칙 (Rule)**: `Rule/ThreeMatchRule.json`, `Rule/TapMatchRule.json` (게임 모드별 매칭 규칙 및 특수 설정)
- **스테이지 (Stage)**: `Stage/Stage.json` (맵의 형태, 장애물 초기 위치, 목표 점수, 타임어택 시간 등)

---

## 🏠 5. 로비 및 씬 흐름
- **로비 메인 로직**: `Assets/02_Scripts/Lobby/LobbyMain.cs` (로비 씬 진입 초기화 및 UI 연동)
- **씬 전환 파이프라인**: 사용자가 시작 버튼을 누르면 `LobbyMain`에서 `StageInjection`을 통해 사양서(GameSpec)를 조립 및 검증한 뒤, 문제가 없을 때만 `GameScene`으로 넘어갑니다.