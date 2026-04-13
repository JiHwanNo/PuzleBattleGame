# PuzleBattleGame 문서 네비게이션

작업 전 이 문서에서 대상 문서를 확인하고 이동하세요.

---

## 문서 목록

| 문서 | 내용 | 언제 읽을까? |
|------|------|-------------|
| `CLAUDE.md` | 코딩 규칙, 아키텍처 원칙, 게임 흐름, 폴더 구조 | 항상 (기본 규칙) |
| `INGAME.md` | 게임 루프, 보드 상태 머신, 블럭, 매칭, 뷰 동기화, 애니메이션, 리플레이 | 인게임 퍼즐 작업 시 |
| `DATA.md` | JSON 구조, GameSpec, Rule/Stage/Replay 데이터, 추가 방법 | 데이터/설정 작업 시 |
| `UI.md` | 도메인 시스템, 팝업/탭 생명주기, UIButton, 추가 방법 | UI 작업 시 |
| `SCENE.md` | 씬 전환, SharedScene, 매니저, AssetManager, PoolManager | 인프라 작업 시 |
| `SERVER.md` | 서버 통신, 공유 DLL, 네트워크 레이어, API 연동 | 서버 API 작업 시 |
| `CHANGELOG.md` | 전체 변경 이력 | 이전 작업 맥락 파악 시 |

---

## 디버깅/최적화 시 필독

| 상황 | 읽을 곳 |
|------|---------|
| 정렬 변경 후 버그 | `CLAUDE.md` → "주의사항 - 정렬 안정성" |
| 뷰 블럭 미씽/소실 | `INGAME.md` → "ExecuteBatchMovement 처리 순서 규칙" |
| LINQ 제거 최적화 | `CLAUDE.md` → "주의사항 - 최적화 작업 시 체크리스트" |
| 씬 전환 후 에셋 미씽 | `CLAUDE.md` → "주의사항 - Pool/Addressable 생명주기" |
| struct null 비교 에러 | `CLAUDE.md` → "주의사항 - 데이터 타입 (struct vs class)" |
| Model에서 Debug.Log 사용 | `CLAUDE.md` → "주의사항 - Model 레이어 로깅" |
| 데이터 타입 확인 | `DATA.md` → "데이터 타입 주의 (struct vs class)" |

---

## 작업별 빠른 경로

| 하고 싶은 작업 | 읽을 문서 |
|---------------|-----------|
| 새 퍼즐 모드 추가 | `INGAME.md` → `DATA.md` |
| 새 블럭 타입 추가 | `INGAME.md` → `DATA.md` |
| 새 스테이지 추가 | `DATA.md` |
| 매칭/점수 밸런싱 | `INGAME.md` → `DATA.md` |
| 애니메이션 수정 | `INGAME.md` |
| 보드 좌표/레이아웃 | `INGAME.md` |
| 리플레이 기록/재생 | `INGAME.md` → `DATA.md` |
| 팝업 추가 | `UI.md` |
| 탭 추가 | `UI.md` |
| 씬 전환 수정 | `SCENE.md` |
| 매니저 구현 | `SCENE.md` |
| 새 씬 추가 | `SCENE.md` |
| 서버 API 연동 | `SERVER.md` |
| 공유 DTO 추가 | `SERVER.md` |
| 네트워크 레이어 | `SERVER.md` |

---

## 주요 소스 파일 위치

| 영역 | 경로 | 핵심 파일 |
|------|------|----------|
| Model (순수 C#) | `02_Scripts/PuzzleCore/Module/` | `GameSpec.cs`, `PuzzleDefine.cs`, `IPuzzleBoard.cs` |
| Model - 보드 | `02_Scripts/PuzzleCore/Module/Board/` | `ThreeMatchPuzzleBoard.cs`, `LinkPuzzleBoard.cs`, `TapMatchPuzzleBoard.cs` |
| Model - 블럭 | `02_Scripts/PuzzleCore/Module/Block/` | `BaseBlock.cs`, `NormalBlock.cs`, `BombBlock.cs`, `PuzzleBlockFactory.cs` |
| View | `02_Scripts/PuzzleCore/View/` | `PuzzleBoardView.cs`, `PuzzleBlockView.cs`, `PuzzleCellView.cs` |
| Controller | `02_Scripts/PuzzleCore/Controller/` | `PuzzleGameController.cs`, `ReplayController.cs` |
| Manager | `02_Scripts/Manager/` | `Main.cs`, `AssetManager.cs`, `PoolManager.cs`, `DomainManager.cs`, `NetworkManager.cs`, `UserDataManager.cs` |
| Network | `02_Scripts/Manager/Network/` | `HttpNetworkClient.cs` |
| UserData | `02_Scripts/Manager/UserData/` | `IdentityLayer.cs` |
| 공유 DLL | `Plugins/` | `PuzleBattleShared.dll` |
| UI | `02_Scripts/UI/` | `UIButton.cs`, `PopupReady.cs` |
| Data | `05_Table/` | `Rule/*.json`, `Stage/Stage.json`, `Replay/*.json` |
