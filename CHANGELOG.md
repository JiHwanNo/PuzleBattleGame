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

## 인게임 UI

- **타이머 UI**: `PuzzleGameController`에 `TMP_Text _timerText` 연동, 초 단위 변경 시에만 갱신 (GC 최소화)
- **점수 UI**: `PuzzleGameController`에 `TMP_Text _scoreText` 연동, 점수 변경 시에만 갱신

## 애니메이션

- **연출 속도 1.75배 향상**: 블럭 이동/낙하/파괴/생성 0.132s → 0.075s, 클릭 0.066s → 0.038s, 액션 간 대기 0.033s → 0.019s

## 최적화

- **PuzzleBoardView LINQ 전면 제거**: Update 내 `GroupBy/OrderBy/Where` → 수동 그룹화/분류로 매 프레임 GC 할당 완전 제거, `System.Linq` using 삭제
- **FetchActions() LINQ 제거**: 3개 보드(ThreeMatch, Link, TapMatch) 모두 `OrderBy().ThenBy().ToList()` → `List.Sort()` 인플레이스 정렬로 변경
- **TapMatchPuzzleBoard Flood Fill 최적화**: `GetConnectedBlocks()`에서 매 호출 `new HashSet/Queue` → 멤버 필드 재사용 + `Clear()`
- **LinkPuzzleBoard.GetCurrentLinkPath() 최적화**: 매 프레임 `new List` 복사 → `IReadOnlyList` 원본 참조 반환
- **AssetManager 메모리 관리**: `AsyncOperationHandle` 보관 + `ReleaseAll()` 씬 전환 시 에셋 캐시 일괄 해제, `MarkPersistent()` 공용 에셋 유지
- **GameSpec.GetBlock() O(1) 캐싱**: `List.Find()` O(n) → `Dictionary<string, BlockData>` 최초 호출 시 구축
- **FindMatches() GC 제거**: 호출마다 `new HashSet` → 필드 `_matchBuffer` 재사용 + `Clear()`
- **ContactFilter2D 캐싱**: 매 프레임 `new ContactFilter2D().NoFilter()` → `static readonly` 필드
- **LineRenderer 갱신 최적화**: 경로 변경 시에만 `SetPosition`/마커 업데이트 (이전 경로 캐싱)
- **PoolManager 풀 크기 제한**: 최대 50개 초과 시 Destroy 처리, `_instanceToPrefab` 중복 키 안전 처리
- **DomainManager.CurrentPath 캐싱**: StringBuilder 재사용 + dirty 플래그로 불필요한 문자열 재구성 방지
- **Main.OnSceneLoaded 탐색 범위 축소**: `FindObjectsByType` 전체 탐색 → `scene.GetRootGameObjects()` 해당 씬만 탐색
- **LobbyTabController 빈 콜백 제거**: 빈 `Start()`/`Update()` 삭제로 Unity 콜백 오버헤드 제거

## 버그 수정

- **PuzzleGameController.GetPointerPosition() null 크래시 수정**: `Mouse.current`가 null일 때 `position.ReadValue()` 호출 시 NullReferenceException → null 체크 추가
- **ReplayStorage 모바일 저장 경로 수정**: `Application.dataPath` (읽기 전용) → 빌드 시 `Application.persistentDataPath` 사용, 에디터에서는 기존 경로 유지
- **Main.cs 이벤트 릭 수정**: `OnSceneLoaded` 구독 → `OnDestroy`에서 해제 추가
- **Main.cs 씬 전환 중복 방지**: `_isMovingScene` 플래그를 코루틴 시작 전 즉시 설정
- **UIButton null 체크**: `_root` 미할당 시 `SendMessage()` 호출 전 null 체크 추가
- **PopupBase 이벤트 정리**: `OnDestroy`에서 `OnOpened`/`OnClosed` 이벤트 null 초기화
- **TabBase 정리 누락**: `OnDestroy` 추가 — `StopAllCoroutines()` + 이벤트 초기화
- **LinkPuzzleBoard 상태 루프**: Falling 후 자기 자신으로 재설정 → Waiting으로 즉시 전환
- **DomainManager 경고 로그**: `RemoveDomainAt`에서 컨트롤러 null 시 경고 로그 추가
- **StageInjection 반환값**: `MakeGameSpec()` void → bool 반환, 로드 실패 시 `_gameSpec = null` + `false` 반환

## 문서

- **MAP.md 생성**: 문서 네비게이션 허브, 작업별 빠른 경로 + 주요 소스 파일 위치 매핑
- **INGAME.md 애니메이션 시간 갱신**: 0.132s → 0.075s (코드와 동기화)
- **SCENE.md AssetManager API 정확도 개선**: `AssetArguments<T>` 구조체 기반 시그니처, `MarkPersistent`/`ReleaseAll` 추가
- **DATA.md 리플레이 경로 설명 갱신**: 에디터/빌드 경로 분리 명시

## 안정성

- **초기화**: GameSpec 주입 시 데이터 누락 체크 (Null 참조 방지)
- **입력 복구**: 탭 매치 드래그 시 선택 상태 정상 초기화
- **코드 컨벤션**: Allman 스타일, 한글 주석, XML 문서화 일괄 적용
