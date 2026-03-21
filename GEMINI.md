# 🤖 GEMINI.md - PuzleBattleGame 개발 가이드라인

이 파일은 Gemini CLI가 이 프로젝트에서 코드를 생성, 수정, 분석할 때 반드시 준수해야 하는 핵심 원칙을 담고 있습니다.

---

## 📝 1. 언어 및 주석 규칙 (Language & Comments)
- **주석**: 모든 코드의 설명 및 주석은 **한글**로 작성합니다.
- **인코딩**: 파일 저장 시 반드시 **UTF-8 (BOM 포함)** 형식을 유지하여 한글 깨짐을 방지합니다.
- **상세 주석**: 모든 **메서드(Method)** 및 **매개변수(Parameter)**에는 해당 기능이 어떤 용도로 쓰이는지, 매개변수가 무엇을 의미하는지에 대한 상세 주석을 반드시 작성합니다. (XML 주석 형식 `///` 권장)
- **설명**: 복잡한 로직이나 데이터 구조를 수정할 때는 의도를 명확히 설명하는 주석을 추가합니다.

## 🏗️ 2. 핵심 아키텍처 원칙 (Core Architecture)
- **다양한 퍼즐 지원**: 3매치, 링크, 육각형 등 여러 형태의 퍼즐 게임을 유연하게 수용할 수 있도록 `IPuzzleBoard` 인터페이스 및 능력(Capability) 기반의 블럭 아키텍처를 사용합니다.
- **데이터 기반 생성 (GameSpec)**: 스테이지 구성(Stage)과 게임 규칙(Rule)은 직접 코딩하지 않고 JSON 데이터를 로드하여 `GameSpec` 객체로 병합한 뒤, 이를 Model에 주입(`Injection`)하는 방식으로 게임을 생성합니다.
- **엄격한 MVC 분리**: 
  - `Model` (PuzzleBoard, PuzzleBlock 등): 유니티 엔진(`UnityEngine`)에 대한 종속성 없이 순수 C# 논리로만 작성됩니다.
  - `View` (PuzzleBoardView 등): Model의 상태 변화(`BoardViewAction`)를 전달받아 시각적 연출과 애니메이션만을 담당합니다.
  - `Controller` (PuzzleGameController): 유저의 입력을 감지하고 큐에 담아 Model에 전달하며, 전체적인 게임 루프를 제어합니다.
- **결정론적 리플레이(Replay) 시스템**:
  - 완벽한 리플레이 기능을 지원하기 위해 모든 유저의 **조작(Input)은 발생한 프레임(Frame)과 함께 큐(Queue)에 저장**되어 순차적으로 처리됩니다.
  - **공유 난수(Random) 클래스**: 난수 생성은 유니티 기본 랜덤이 아닌 단일 난수 클래스를 공유하여 사용합니다. 시드(Seed)와 프레임을 일정하게 유지함으로써 동일한 입력에 대해 항상 동일한 결과(결정론적 작동)를 보장합니다.

## 📛 3. 명명 규칙 (Naming Convention)
- **클래스/메서드/공개 필드**: `PascalCase` (예: `PuzzleGameController`, `Initialize`)
- **비공개 필드(Private)**: `_camelCase` (예: `_board`, `_gameSpec`)
- **변수/매개변수**: `camelCase` (예: `stageData`, `rulePath`)

## 📦 4. 에셋 및 리소스 관리
- **Addressables 기반**: 모든 게임 리소스(프리팹, 사운드, JSON 등)는 Addressables 시스템을 통해 관리됩니다.
- **AssetManager 활용**: 에셋을 로드할 때는 반드시 `AssetManager.cs`에 구현된 `LoadAsset<T>`, `LoadAssetAsync<T>`, `LoadGameObject` 등의 메서드를 통해 에셋을 확보합니다. 직접적인 `Addressables.LoadAssetAsync` 호출보다는 기존 매니저의 래핑된 기능을 우선 사용합니다.

## 🛠️ 5. 기타 준수 사항
- **자동 커밋 금지**: Gemini CLI는 어떠한 경우에도 사용자의 명시적인 요청 없이 스스로 커밋(`git commit`)을 수행하지 않습니다. 모든 변경사항은 사용자가 검토한 후 직접 커밋하거나, 커밋 요청이 있을 때만 수행합니다.
- **확장성**: 새로운 퍼즐 규칙(예: 육각형 매치)이 추가될 수 있음을 고려하여 인터페이스나 데이터 구조를 설계합니다.
- **안정성**: 씬 이동이나 데이터 로드 실패 시 에러 로그를 명확히 남겨 디버깅이 용이하도록 합니다.

## 🚀 6. 현재까지의 개발 진행 상황 (Summary of Work Done)
Gemini CLI가 다음 작업을 이어가거나 컨텍스트를 파악할 때 참고하기 위한 지금까지의 작업 내역입니다.

- **에셋 관리 시스템 (AssetManager)**: 
  - `Addressables`를 기반으로 한 싱글톤 매니저 구현.
  - 비동기(`LoadAssetAsync`, `LoadGameObjectAsync`) 및 동기(`LoadAsset`, `LoadGameObject`) 로드 메서드 제공.
  - 로드된 에셋 및 프리팹 캐싱 기능(`_addressablePacket`) 적용 완료.
- **스테이지 및 규칙 데이터 주입 (StageInjection)**:
  - `GameRule.json`과 `Stage.json`을 읽어 게임 시작 전 전체 설정값인 `GameSpec`을 완성하는 구조 구축.
  - JSON 데이터를 파싱하여 코어 모델(Model)에 전달할 준비 완료.
- **로비 및 씬 흐름 연동 (LobbyMain)**:
  - 로비 씬 진입 시 초기화 로직 구현.
  - '스테이지 시작' 시 `StageInjection`을 통해 사양서를 만들고 문제가 없을 시 `GameScene`으로 이동하도록 씬 플로우 연결.
- **퍼즐 뷰 및 물리 충돌 연동 (PuzzleCore - View/Collider)**:
  - `PuzzleBlockCollider` 및 `PuzzleCellCollider`를 통해 유니티 `OnMouseDown` 이벤트를 감지하여 논리 영역으로 전달하는 구조 완성.
  - Addressables로 스프라이트가 비동기 로드된 후, `SpriteRenderer`의 실제 이미지 크기(`bounds.size`)를 읽어와 `BoxCollider2D`의 크기를 동적으로 자동 조절하는 로직 적용.
- **인터페이스 기반 보드 아키텍처 도입 (IPuzzleBoard)**:
  - 단일 구체 클래스에 의존하던 보드 모델을 `IPuzzleBoard`로 추상화하여, 3매치, 링크 등 다양한 게임 모드를 유연하게 교체(팩토리 패턴)할 수 있는 기반 마련.
  - 시각적 연출 요청(Action) 데이터를 공통 규격인 `BoardViewAction` 클래스로 분리.
- **다중 입력(Capability) 기반 블럭 아키텍처 (Strategy Pattern)**:
  - 블럭의 조작 방식을 비트 플래그(`[Flags] enum InputType`)로 변경하여 하나의 블럭이 다중 조작(예: 터치+스왑)을 지원할 수 있도록 설계.
  - `PuzzleBlock`을 추상 클래스로 두고, 조작 능력에 따라 `ITouchableBlock`, `ISwappableBlock`, `ILinkableBlock` 등의 인터페이스를 선택적으로 상속받아 구현.
  - `PuzzleBlockFactory`를 통해 JSON 데이터의 `InputType` 속성에 맞는 구체적인 블럭(예: `NormalBlock`, `BombBlock`)을 동적으로 생성.
- **3매치(ThreeMatch) 코어 로직 기초 구현**:
  - `ThreeMatchPuzzleBoard.cs`를 생성하여 첫 번째 클릭과 두 번째 클릭을 통한 인접 블럭 판별 및 스왑(Swap) 로직 구현.
  - 하드코딩된 로직 대신 블럭이 가진 능력(인터페이스)을 확인(`is ISwappableBlock`)하여 블럭 내부의 스왑 로직을 실행하도록 유연하게 개선.
  - Controller 루프에서 큐(Queue)에 쌓인 입력을 소모(`InputEnd()`)하고 변경 사항이 있으면 뷰를 동기화(`RefreshBlocks()`)하도록 연결 완료.
- **게임 루프 리팩토링 및 안정화 (Phase-based Logic)**:
  - 보드 업데이트 루프를 페이즈 방식(**Matching -> Falling -> Filling -> Cascade Check**)으로 재설계하여 안정적인 연쇄 반응과 결정론적 동작을 보장.
  - 유저 조작이 끝나고 보드가 완전히 정지(Waiting 상태)될 때까지 입력을 차단하는 안정화 로직 적용.
- **뷰(View) 관리 및 동기화 최적화**:
  - **Batch Move 도입**: 스왑 시 딕셔너리 키 충돌로 인한 블럭 유실 문제를 해결하기 위해 이동 액션을 일괄 처리하도록 개선.
  - **액션 처리 순서 보장**: 모델에서 발생한 순서대로 액션을 처리하고, 특히 이동(Move)을 파괴(Destroy)보다 우선하여 시각적 정합성 확보.
- **디버그 시스템 및 시각화 강화**:
  - 씬 뷰에서 그리드 좌표와 블럭 ID를 표시하는 기능 추가.
  - 모델 데이터가 아닌 **실제 생성된 뷰 객체 데이터**를 우선 표시하여 동기화 오류(`MISSING VIEW`)를 즉시 감지할 수 있도록 개선.
- **코드 컨벤션 수립 및 일괄 적용**:
  - `CONVENTIONS.md`를 통해 Allman 스타일 중괄호, 한글 주석, XML 문서화 등 프로젝트 표준 정립.
  - 전체 소스 코드에 대해 로직 손실 없이 컨벤션을 일괄 적용하는 리팩토링 완료.
