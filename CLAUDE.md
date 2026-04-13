# PuzleBattleGame - Claude Code 개발 가이드라인

Unity 6000.0.38f1 (URP) 기반 퍼즐 배틀 게임 프레임워크.
세부 작업 시 `MAP.md`를 읽고 해당 참고 문서로 이동.

---

## 코딩 규칙

### 파일 및 인코딩
- 모든 소스 파일은 **UTF-8 (BOM 포함)** 형식.
- 코드 내 모든 설명 및 주석은 **한글**로 작성.
- 모든 메서드 및 매개변수에 한글 XML 주석(`///`) 작성 필수.
- 복잡한 로직이나 데이터 구조를 수정할 때는 의도를 명확히 설명하는 주석을 추가.

### 명명 규칙
| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스/메서드/공개 필드/구조체/열거형 | `PascalCase` | `PuzzleGameController` |
| 인터페이스 | `I` + `PascalCase` | `IPuzzleBoard` |
| 비공개 필드 | `_camelCase` | `_board`, `_frameCount` |
| 지역 변수/매개변수 | `camelCase` | `targetPos`, `stageData` |

### 코드 스타일
- **Allman 스타일**: 모든 제어문은 단 한 줄이어도 반드시 중괄호 `{}` 사용, 중괄호는 새 줄에서 시작.
```csharp
if (condition)
{
    DoSomething();
}
```
- 스타일 교정 시 기존 실행 로직/메서드 호출/조건문 결과를 절대 변경하지 않는다.

### 에셋 및 리소스
- 모든 게임 리소스는 **Addressables** 시스템을 통해 관리.
- 에셋 로드 시 반드시 `AssetManager.cs` 경유. 직접 `Addressables.LoadAssetAsync` 호출 금지.
- 인게임 시각 요소는 **`Ingame` Sorting Layer**에서 렌더링.

### 디버깅
- 씬 뷰/GUI 디버그 정보는 **실제 뷰 객체(View Object) 상태**를 우선 반영.
- 씬 이동/데이터 로드 실패 시 명확한 에러 로그 남길 것.

---

## 핵심 아키텍처

### MVC 패턴 (엄격한 분리)
- **Model** (`PuzzleCore/Module/`): 순수 C# 논리. `UnityEngine` 종속성 금지.
- **View** (`PuzzleCore/View/`): Model의 `BoardViewAction` 이벤트를 수신하여 시각적 연출만 담당.
- **Controller** (`PuzzleCore/Controller/`): 유저 입력 감지/큐 저장, 틱 단위로 Model Update 호출.

### 데이터 기반 생성
- 게임 규칙과 스테이지는 JSON → `GameSpec` 객체로 병합 후 Model에 주입. 하드코딩 금지.

### 결정론적 리플레이
- 유저 입력은 프레임(Frame) + 큐(Queue)로 순차 처리.
- `GameSpec.randomSeed` 기반 `PuzzleRandom`, `FixedUpdate`에서만 프레임 전진.
- 매 게임마다 랜덤 시드 생성 → 리플레이 시 동일 시드 적용으로 재현.
- `InputRecord`(입력 좌표) + `InputEndRecord`(입력 종료) 프레임 단위 기록 → JSON 저장.
- `ReplayController`가 기록된 입력을 프레임에 맞춰 보드에 주입하여 재생.

### 인터페이스 기반 확장
- `IPuzzleBoard`로 퍼즐 모드 교체 (ThreeMatch, Link, TapMatch).
- 블럭 능력 인터페이스: `ITouchableBlock`, `ISwappableBlock`, `ILinkableBlock`.

---

## 주의사항 (Known Pitfalls)

### 데이터 타입 (struct vs class)
- JSON 직렬화 데이터 중 **struct**: `RuleData`, `ObjectiveData`, `InputRecord`, `InputEndRecord`, `GridPos`
- JSON 직렬화 데이터 중 **class**: `GameSpec`, `GameRuleContainer`, `BlockData`, `StageData`, `CellData`, `ReplayData`
- struct는 `== null` 비교 불가. `JsonUtility.FromJson` 실패 시 기본값(zero)이 들어가므로, class 컨테이너의 null 체크로 파싱 실패를 감지해야 한다.
- 실제 사례: `StageInjection`에서 `ruleContainer.rule == null` 비교 시 `RuleData`가 struct라 CS0019 컴파일 에러 발생.

### Model 레이어 로깅
- `PuzzleCore/Module/` 하위 클래스는 `UnityEngine` 종속성 금지이므로 `Debug.LogError` 사용 불가.
- 대신 `Action<string> OnLog` 델리게이트 + `Log()` 메서드로 외부에 로그 전달.
- 에러 로깅이 필요하면 반드시 `Log()` 메서드를 사용할 것.

### 정렬 안정성
- `List.Sort()`는 **불안정 정렬**(Unstable Sort)이다. 동일 키 요소의 상대 순서가 보장되지 않음.
- 삽입 순서에 의존하는 로직이 있으면 불안정 정렬로 전환 시 반드시 **처리 순서 분리** 또는 **명시적 타이브레이커**를 추가해야 한다.
- 실제 사례: `FetchActions()`의 LINQ → `List.Sort()` 전환 시, 같은 `orderIndex`를 가진 Fall과 CreateAndFall 순서가 뒤바뀌어 블럭 미씽 버그 발생 → `ExecuteBatchMovement`에서 루프 분리로 해결.

### 뷰 액션 처리 순서
- `ExecuteBatchMovement`에서 **Move/Fall을 반드시 먼저** 처리한 후 CreateAndFall을 처리해야 함.
- CreateAndFall은 `targetPosition`에 기존 뷰가 있으면 `HandleImmediateDestroy`로 파괴하는데, Fall이 먼저 실행되어 해당 위치의 뷰를 제거하지 않으면 이동 예정 블럭이 소실됨.
- 상세: `INGAME.md`의 "ExecuteBatchMovement 처리 순서 규칙" 참고.

### 최적화 작업 시 체크리스트
- LINQ 제거 시: 정렬 안정성, 지연 평가(Lazy Evaluation) 차이 확인.
- 컬렉션 재사용 시: 반환된 참조를 외부에서 보관하는 곳이 없는지 확인 (예: `GetConnectedBlocks`의 `_connectedBuffer`).
- 코루틴에 전달하는 리스트: 코루틴 yield 중 해당 리스트가 외부에서 변경되지 않는지 확인.
- Dictionary foreach 대신 List 기반 병렬 인덱스 패턴 고려 (Enumerator GC 할당 방지).
- `using System.Linq` 제거 후 `.Last()`, `.Any()`, `.Where()` 등 LINQ 확장 메서드가 남아있지 않은지 반드시 확인.

### FetchActions() 리스트 스왑 패턴
- `FetchActions()`는 내부 `_views` 리스트를 **참조 스왑** 방식으로 반환한다.
- `var res = _views; _views = new List<>(); return res;` — 복사 비용 없이 소유권 이전.
- 반환된 리스트는 호출자가 소유하며, 다음 `FetchActions()` 호출과 무관하게 안전하다.
- 코루틴 yield 중에도 반환된 리스트가 변경되지 않음이 보장된다.

### ExecuteBatchMovement 리스트 기반 매핑
- `Dictionary<BoardViewAction, PuzzleBlockView>` 대신 `_batchActions` / `_batchViews` 두 개의 `List`를 병렬 인덱스로 사용.
- Dictionary 열거 시 Enumerator 할당을 방지하며, 처리 순서가 삽입 순서에 의해 보장된다.

### Pool/Addressable 생명주기
- `PoolManager`는 `DontDestroyOnLoad`로 씬 전환 후에도 풀 인스턴스 유지.
- `AssetManager.ReleaseAll()`은 씬 전환 시 `MarkPersistent()` 되지 않은 에셋 해제.
- 풀에 남은 인스턴스가 해제된 에셋을 참조할 수 있으므로, 씬 전환 전후 풀 상태 주의.

---

## 작업별 참고 문서

| 작업 | 문서 |
|------|------|
| 인게임 퍼즐 (보드, 블럭, 매칭, 뷰, 애니메이션) | `INGAME.md` |
| 리플레이 (기록, 저장, 재생) | `INGAME.md` + `DATA.md` |
| UI/팝업/탭 (도메인 시스템, UIButton) | `UI.md` |
| 데이터/설정 (JSON, GameSpec, 추가 방법) | `DATA.md` |
| 씬/매니저/인프라 (씬 전환, AssetManager, Pool) | `SCENE.md` |
| 서버 통신/API/공유 DTO/네트워크 레이어 | `SERVER.md` |
| 변경 이력 | `CHANGELOG.md` |

### 자주 하는 작업 → 빠른 경로

| 하고 싶은 작업 | 읽을 문서 |
|---------------|-----------|
| 새 퍼즐 모드 추가 | `INGAME.md` + `DATA.md` |
| 새 블럭 타입 추가 | `INGAME.md` + `DATA.md` |
| 새 스테이지 추가 | `DATA.md` |
| 팝업 추가 | `UI.md` |
| 탭 추가 | `UI.md` |
| 씬 전환 수정 | `SCENE.md` |
| 매니저 구현 (사운드, 네트워크 등) | `SCENE.md` |
| 서버 API 연동 추가 | `SERVER.md` |
| 공유 DTO 추가 | `SERVER.md` |
| 매칭/점수 밸런싱 | `INGAME.md` + `DATA.md` |
| 애니메이션 수정 | `INGAME.md` |
| 보드 좌표/레이아웃 | `INGAME.md` |
| 리플레이 기록/재생 | `INGAME.md` + `DATA.md` |

---

## 게임 흐름

```
[앱 시작]
  Main (RuntimeInitializeOnLoadMethod) → SharedScene 자동 로드
    → TitleScene (CI 연출)
      → LoadingScene 경유
        → LobbyScene (스테이지 선택)
          → PopupReady 팝업 열기
            → [시작] StageInjection.MakeGameSpec() (JSON → GameSpec + 랜덤 시드 생성)
            → [리플레이] 최근 리플레이 로드 → SetReplayData()
              → LoadingScene 경유
                → GameScene
                  → Board 생성 + Initialize + View 그리기 → 게임 루프
                  → (리플레이 있으면) ReplayController 초기화 → 우측 상단 자동 재생
                    → 게임 종료(Finish) → 리플레이 JSON 저장 → LobbyScene 이동
```

---

## 폴더 구조 (Assets 기준)

```
01_Scenes/    - SharedScene, TitleScene, LoadingScene, LobbyScene, GameScene
02_Scripts/
  PuzzleCore/ - Module(Model) / View / Controller
  Manager/    - 시스템 매니저 전체
  Lobby/      - 로비 로직
  Title/      - 타이틀 로직
  UI/         - UI 컴포넌트
03_Prefab/    - Puzzle/, UI/
04_Resources/ - 이미지, 사운드
05_Table/     - Rule/ (3개 JSON), Stage/ (Stage.json), Replay/ (리플레이 JSON)
```
