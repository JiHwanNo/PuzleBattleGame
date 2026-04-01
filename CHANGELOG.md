# PuzleBattleGame 변경 이력 (Changelog)

---

## 인프라 및 씬 시스템

- **SharedScene 영구 상주 아키텍처**: `Main.cs` 싱글톤, `[RuntimeInitializeOnLoadMethod]`로 자동 로드, 에디터에서 어떤 씬이든 TitleScene 강제 이동
- **씬 전환 시스템**: `CoMoveScene` 코루틴 기반, 로딩 씬 경유 파이프라인, `_isMovingScene` 중복 방지
- **매니저 시스템**: SoundManager, GameDataManager, UserDataManager, UIManager, NetworkManager, LocalizationManager (SharedScene 배치, DontDestroyOnLoad)
- **카메라 관리**: CameraController 싱글톤, 중복 카메라 자동 제거
- **중복 EventSystem 자동 제거**: `Main.OnSceneLoaded`에서 처리
- **타이틀 연출**: DOTween Sequence CI 색상 트윈(검정 → #00FF80) 후 씬 이동

## 에셋 및 데이터

- **AssetManager**: Addressables 기반 싱글톤, 비동기/동기 로드, 캐싱(`_addressablePacket`)
- **StageInjection**: ThreeMatchRule/TapMatchRule/LinkMatchRule + Stage.json → GameSpec 병합, 랜덤 시드 생성, 에셋 주소 보관(`GetRuleAddress()`, `GetStageAddress()`), 리플레이 데이터 전달(`SetReplayData()`, `GetReplayData()`)
- **ReplayStorage**: 리플레이 JSON 저장/로드 유틸리티 (저장 경로: `Assets/05_Table/Replay/`)

## 도메인 시스템 (팝업/탭)

- **DomainManager**: 컨트롤러 등록/해제 방식, URL 경로 추적
- **팝업**: PopupController 추상 베이스 → PopupBase (프리팹) → PopupHandler (개별 핸들러)
- **씬별 팝업 컨트롤러**: SharedPopupController, LobbyPopupController, GamePopupController
- **탭**: TabController 베이스 → TabBase UI, LobbyTabController로 로비 탭 전환

## 퍼즐 코어 — Model

- **IPuzzleBoard 인터페이스**: 3매치/링크/탭 매치 보드를 팩토리 패턴으로 교체
- **블럭 아키텍처**: 비트 플래그 `InputType` + Capability 인터페이스 (`ITouchableBlock`, `ISwappableBlock`, `ILinkableBlock`)
- **PuzzleBlockFactory**: JSON `InputType`에 맞는 블럭(NormalBlock, BombBlock) 동적 생성
- **3매치 코어**: 인접 블럭 스왑, 페이즈 루프 (Matching → Falling → Filling → Cascade Check)
- **자동 셔플**: `HasPossibleMoves` 감지 후 매칭 보장 셔플
- **콤보/피버**: 액티브 콤보 점수 배율 + 피버 타임 2배 득점
- **타임 어택**: `RuleData` 제한 시간 → 시간 종료 시 Finish 상태
- **Module 하위 구조 정리**: Block/, Board/ 폴더 분리

## 퍼즐 코어 — View/Controller

- **뷰 동기화**: Batch Move 도입 (딕셔너리 키 충돌 해결), 액션 처리 순서 보장 (Move > Destroy)
- **물리 충돌**: PuzzleBlockCollider/PuzzleCellCollider → OnMouseDown 이벤트 전달
- **Collider 자동 조절**: 비동기 스프라이트 로드 후 bounds.size 기반 BoxCollider2D 크기 동적 설정
- **디버그**: 씬 뷰 그리드 좌표/블럭 ID 표시, 뷰 객체 상태 우선 (`MISSING VIEW` 감지)
- **PuzzleCellView**: 셀 시각 연출 분리

## UI

- **UI 폴더**: UIButton 공통 버튼 컴포넌트, PopupReady 게임 준비 팝업

## 리플레이 시스템

- **리플레이 기록**: `InputRecord`(프레임+좌표) + `InputEndRecord`(프레임) 자동 기록, 3개 보드 모두 지원
- **리플레이 저장**: 게임 종료(Finish) 시 `ReplayData` 자동 조립 → `ReplayStorage.Save()` → `Assets/05_Table/Replay/` JSON 저장
- **리플레이 재생**: `ReplayController`가 기록된 입력을 프레임 단위로 보드에 주입하여 결정론적 재현
- **랜덤 시드 동적 생성**: `PuzzleRandom(0)` 하드코딩 → `GameSpec.randomSeed` 기반으로 변경, 매 게임마다 새 시드 생성
- **GridPos 직렬화 수정**: get-only 프로퍼티 → public 필드로 변경 (`JsonUtility` 호환)
- **게임 종료 → 로비 이동**: `BoardState.Finish` 감지 시 리플레이 저장 후 자동 LobbyScene 이동
- **PopupReady 리플레이 테스트**: `OnClickReplay` 버튼으로 최근 리플레이 로드 + 상대 보드 재생
- **ReplayController 배치**: 카메라 기준 우측 상단 자동 축소 배치 (`viewScale`, `margin` 설정)
- **PuzzleBoardView 확장**: `skipCameraAlign` 플래그, `DrawBoard(board, boardShape?)` 오버로드

## 안정성

- **초기화**: GameSpec 주입 시 데이터 누락 체크 (Null 참조 방지)
- **입력 복구**: 탭 매치 드래그 시 선택 상태 정상 초기화
- **코드 컨벤션**: Allman 스타일, 한글 주석, XML 문서화 일괄 적용
