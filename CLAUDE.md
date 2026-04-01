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

## 작업별 참고 문서

| 작업 | 문서 |
|------|------|
| 인게임 퍼즐 (보드, 블럭, 매칭, 뷰, 애니메이션) | `INGAME.md` |
| 리플레이 (기록, 저장, 재생) | `INGAME.md` + `DATA.md` |
| UI/팝업/탭 (도메인 시스템, UIButton) | `UI.md` |
| 데이터/설정 (JSON, GameSpec, 추가 방법) | `DATA.md` |
| 씬/매니저/인프라 (씬 전환, AssetManager, Pool) | `SCENE.md` |
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
