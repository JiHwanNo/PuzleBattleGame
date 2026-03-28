# PuzleBattleGame - Claude Code 개발 가이드라인

이 프로젝트는 Unity 6000.0.38f1 (URP) 기반의 퍼즐 배틀 게임 프레임워크입니다.
3매치, 링크, 육각형 등 다양한 퍼즐 규칙을 JSON 데이터로 유연하게 적용할 수 있도록 설계되었습니다.

---

## 핵심 아키텍처 원칙

### MVC 패턴 (엄격한 분리)
- **Model** (`Assets/02_Scripts/PuzzleCore/Module/`): 순수 C# 논리만 사용. `UnityEngine` 종속성 금지. `IPuzzleBoard`, `PuzzleBlock` 등.
- **View** (`Assets/02_Scripts/PuzzleCore/View/`): Model의 `BoardViewAction` 이벤트를 수신하여 시각적 연출만 담당. 로직 판단 금지.
- **Controller** (`Assets/02_Scripts/PuzzleCore/Controller/`): 유저 입력 감지/큐 저장, 틱 단위로 Model Update 호출 후 결과를 View로 전달.

### 데이터 기반 생성 (GameSpec)
- 스테이지(Stage)와 게임 규칙(Rule)은 JSON 데이터를 로드하여 `GameSpec` 객체로 병합 후 Model에 주입하는 방식.
- 직접 코딩으로 게임 규칙을 하드코딩하지 않는다.

### 결정론적 리플레이 시스템
- 모든 유저 입력은 발생한 프레임(Frame)과 함께 큐(Queue)에 저장되어 순차 처리.
- 난수 생성은 Unity 기본 Random이 아닌 **단일 공유 난수 클래스** 사용 (시드 기반 결정론).
- 논리 프레임(`_frameCount`)은 반드시 `FixedUpdate`에서만 전진.
- 게임 로직(`_board.Update`)은 `Update`에서 실행.

### 인터페이스 기반 확장
- `IPuzzleBoard` 인터페이스 및 능력(Capability) 기반 블럭 아키텍처 사용.
- 블럭 조작 방식은 비트 플래그(`[Flags] enum InputType`)로 다중 조작 지원.
- 새로운 퍼즐 규칙 추가 시 인터페이스/데이터 구조의 확장성을 고려할 것.

---

## 코딩 규칙

### 파일 및 인코딩
- 모든 소스 파일은 **UTF-8 (BOM 포함)** 형식.
- 코드 내 모든 설명 및 주석은 **한글**로 작성.
- 모든 메서드 및 매개변수에 한글 XML 주석(`///`) 작성 필수.

### 명명 규칙
- 클래스/메서드/공개 필드/구조체/열거형: `PascalCase`
- 인터페이스: `I` 접두사 + `PascalCase` (예: `IPuzzleBoard`)
- 비공개 필드: `_camelCase` (언더바 접두사, 예: `_board`, `_frameCount`)
- 지역 변수 및 매개변수: `camelCase`

### 코드 스타일
- **Allman 스타일**: 모든 제어문은 단 한 줄이어도 반드시 중괄호 `{}` 사용, 중괄호는 새 줄에서 시작.
- 스타일 교정 시 기존 실행 로직/메서드 호출/조건문 결과를 절대 변경하지 않는다.

---

## 에셋 및 리소스 관리
- 모든 게임 리소스는 **Addressables** 시스템을 통해 관리.
- 에셋 로드 시 반드시 `AssetManager.cs`의 `LoadAsset<T>`, `LoadAssetAsync<T>`, `LoadGameObject` 등 사용.
- 직접적인 `Addressables.LoadAssetAsync` 호출 금지 (래핑된 매니저 기능 우선).

---

## 프로젝트 구조 (Assets 기준)

```
01_Scenes/          - 게임 씬 파일 (.unity)
02_Scripts/         - C# 소스 코드
  PuzzleCore/
    Module/         - Model (순수 C# 퍼즐 로직)
    View/           - View (유니티 시각 연출)
    Controller/     - Controller (입력/게임 루프)
  Manager/          - 시스템 매니저 (AssetManager, PoolManager, PopupManager)
  Lobby/            - 로비 로직 (LobbyMain)
  StageInjection.cs - JSON -> GameSpec 변환
03_Prefab/          - 유니티 프리팹
04_Resources/       - 이미지, 사운드 등
05_Table/           - JSON 데이터
  Rule/             - 게임 모드별 규칙 (ThreeMatchRule.json, TapMatchRule.json)
  Stage/            - 스테이지 데이터 (Stage.json)
```

## 씬 흐름
TitleScene -> LoadingScene -> LobbyScene -> GameScene (+ PopupScene)
- LobbyScene에서 `StageInjection`을 통해 `GameSpec` 조립/검증 후 GameScene으로 이동.

---

## 디버깅 원칙
- 씬 뷰/GUI 디버그 정보는 모델 데이터보다 **실제 뷰 객체(View Object) 상태**를 우선 반영.
- 씬 이동/데이터 로드 실패 시 명확한 에러 로그 남길 것.

---

## 참조 문서
- `CONVENTIONS.md`: 상세 코딩 규칙
- `MAP.md`: 아키텍처 파일 맵핑 (코드 탐색 시 먼저 확인)
- `CHANGELOG.md`: 개발 히스토리 및 변경점
- `README.md`: 프로젝트 개요
