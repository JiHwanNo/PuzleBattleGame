# PuzleBattleGame 인게임 퍼즐 참고 문서

보드, 블럭, 매칭, 게임 루프, 뷰 동기화 작업 시 참고.
데이터/JSON 구조는 `DATA.md` 참고.

---

## 게임 루프

### Controller (PuzzleGameController)
- `Update()`: 마우스/터치 입력 → `Physics2D.OverlapPoint` → `PuzzleBlockCollider` → `board.Input(gridPos)`
- 포인터 릴리즈 시 `board.InputEnd()` 호출
- `FixedUpdate()`: `board.FixedUpdate()` (논리 프레임 전진)
- `boardView.IsAnimating`이 true이면 입력 차단

### Board 상태 머신 (IPuzzleBoard.Update)
```
Waiting: 유저 입력 수신 대기
  ↓ InputEnd() → 스왑 시도
Matching: FindMatches() → 3+ 연속 블럭 탐색 → 파괴 → AddView(Destroy)
  ↓
Falling: 빈 칸으로 블럭 낙하 + Generator 셀에서 새 블럭 생성 → AddView(Fall, CreateAndFall)
  ↓ 다시 Matching으로 (연쇄 반응)
Waiting: 매칭 없으면 복귀, HasPossibleMoves() 실패 시 자동 셔플
  ↓ 목표 달성 또는 시간 종료
Finish
```

---

## IPuzzleBoard 인터페이스

| 메서드 | 용도 |
|--------|------|
| `Initialize(GameSpec)` | 보드 초기화 (셀/블럭 생성) |
| `Input(GridPos)` | 유저 입력 큐에 추가 |
| `InputEnd()` | 큐 소비 → 스왑 시도 |
| `Update()` | 상태 머신 실행 (Matching/Falling/Filling) |
| `FixedUpdate()` | 논리 프레임 전진 + 타이머 갱신 |
| `Pause(bool)` | 일시정지 |
| `AddView(BoardViewAction)` | 뷰 액션 기록 |
| `FetchActions()` | 기록된 뷰 액션 반환 후 초기화 |

**프로퍼티**: `State`, `Random`, `Objective`, `Cells`, `Width`, `Height`

**구현체**: ThreeMatchPuzzleBoard, LinkPuzzleBoard, TapMatchPuzzleBoard

---

## 블럭 아키텍처

### 상속 구조
```
BaseBlock (추상) — State: Idle, Selected, Moving, Matched, Falling, None
├─ NormalBlock (ISwappableBlock, ILinkableBlock)
└─ BombBlock (ITouchableBlock + ISwappableBlock)
```

### 능력 인터페이스
| 인터페이스 | 메서드 | 용도 |
|-----------|--------|------|
| ITouchableBlock | `OnTouched(board, myPos)` | 터치 시 실행 |
| ISwappableBlock | `OnSwapped(board, myPos, targetPos)` | 스왑 시 실행 |
| ILinkableBlock | `CanLink(board, myPos, previousPos)` | 링크 연결 판정 |

### 팩토리 (PuzzleBlockFactory.Create)
- Touch + Swap → BombBlock
- Swap only → NormalBlock
- 기타 → NormalBlock

### 새 블럭 추가 시
1. `BaseBlock` 상속 클래스 작성 (`Module/Block/`)
2. 필요한 능력 인터페이스 구현
3. `PuzzleBlockFactory.Create()`에 분기 추가
4. `DATA.md` 참고하여 BlockData JSON 추가

---

## 입력 처리 방식

### 3매치 (ThreeMatch)
1. **탭-탭**: 첫 블럭 선택 → 인접 블럭 선택 → 스왑
2. **드래그**: 홀드 후 드래그 → 첫-마지막 인접 블럭 스왑

### 링크 (Link)
- 드래그로 같은 종류 블럭 연결 → 릴리즈 시 경로상 블럭 파괴
- `LineRenderer`로 경로 시각화

### 탭 매치 (TapMatch)
- 블럭 터치 시 즉시 파괴 로직 실행

---

## 점수 및 콤보 (ObjectiveManager)

| 항목 | 값 |
|------|-----|
| 기본 점수 | 블럭당 10점 |
| 콤보 배율 | 1.0 + (combo - 1) × 0.2, 최대 3.0x |
| 피버 조건 | 7콤보 이상 |
| 피버 효과 | 7초간 (350프레임) 2.0x 추가 배율 |
| 콤보 유지 | 3초 (150프레임) 이내 재매칭 |

**목표 종류**: `Score` (점수 달성), `CollectBlock` (특정 블럭 수집), `ClearCell` (셀 클리어)

---

## 뷰 동기화 (PuzzleBoardView)

### 액션 처리 흐름
```
board.FetchActions()
  → List<BoardViewAction> (frame + orderIndex 순서)
    → 그룹화 (같은 frame+order는 동시 실행)
      → ProcessActionQueue 코루틴
        → ExecuteBatchMovement (Move, Fall, CreateAndFall)
        → ExecuteSingleAction (Destroy, Create)
```

### BoardViewAction 구조
| 필드 | 용도 |
|------|------|
| `frame` | 논리 프레임 번호 |
| `orderIndex` | 시각 순서 (같은 frame 내 정렬) |
| `type` | ViewType: Destroy, Create, Move, Fall, CreateAndFall, Land |
| `position` | 원본 좌표 |
| `targetPosition` | 이동 대상 좌표 |
| `blockData` | 블럭 정보 (생성 시) |

### 애니메이션 시간
| 종류 | 시간 | Ease |
|------|------|------|
| 클릭 | 스케일 1.1x, 2회 yoyo | — |
| 이동 (Move) | 0.132초 | OutBack |
| 낙하 (Fall) | 0.132초 | OutQuad |
| 파괴 (Destroy) | 스케일→0, 0.132초 | InBack |
| 생성 (Create) | 스케일 0→1, 0.132초 | OutBack |

### 좌표 변환 (GetLocalPos)
- 사각형: 보드 중앙 기준 `(X - width/2, Y - height/2) × cellSize`
- 육각형 (Even-Q Flat-Top): 짝수 열은 Y에 `cellSize × 0.5f` 오프셋
- 카메라 orthographicSize를 보드 높이에 맞게 자동 설정

---

## 핵심 열거형 (PuzzleDefine.cs)

| 열거형 | 값 |
|--------|-----|
| PuzzleType | ThreeMatch, Link, TapMatch |
| BoardShape | Quadrangle, Hexagon |
| CellType | Close, Normal, Lock, Generator |
| InputType | Swap(1), Link(2), Touch(4) — Flags |
| BoardState | Waiting, Matching, Falling, Filling, Finish |
| ViewType | Destroy, Create, Move, Land, Fall, CreateAndFall |
| BlockState | Idle, Selected, Moving, Matched, Falling, None |

### GridPos
- `int X, Y` + 정적 방향 (Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight)
- `IsAdjacentSquare(a, b)`: 4방향 인접 판정
- `IsAdjacentHexagon(a, b)`: Even-Q Flat-Top 6방향 인접 판정
