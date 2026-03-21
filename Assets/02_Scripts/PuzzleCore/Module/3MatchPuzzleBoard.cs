using System;
using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Core
{
    /// <summary>
    /// 3매치(Three Match) 퍼즐 게임의 보드 상태와 핵심 로직을 관리하는 Model 클래스입니다.
    /// IPuzzleBoard 인터페이스를 구현하며, 입력 처리, 스왑, 매칭 판정 기능을 포함합니다.
    /// </summary>
    public class ThreeMatchPuzzleBoard : IPuzzleBoard
    {
        /// <summary> 보드의 현재 논리적 상태 </summary>
        public BoardState State { get; private set; } = BoardState.Waiting;

        /// <summary> 보드 내부 로직에서 발생하는 로그를 외부(View/Controller)로 전달합니다. </summary>
        public Action<string> OnLog { get; set; }
        
        /// <summary> 로그 중복 방지를 위한 이전 상태 기록 </summary>
        private BoardState _lastLoggedState = BoardState.Waiting;

        /// <summary> 게임 내 공용 난수 생성기 </summary>
        public PuzzleRandom Random { get; private set; }

        /// <summary> 스테이지 목표 관리자 </summary>
        public ObjectiveManager Objective { get; private set; }

        /// <summary> 좌표별 셀 데이터를 저장하는 딕셔너리 </summary>
        public Dictionary<GridPos, PuzzleCell> Cells { get; private set; }

        /// <summary> 보드의 가로 너비 </summary>
        public int Width { get; private set; }

        /// <summary> 보드의 세로 높이 </summary>
        public int Height { get; private set; }

        /// <summary> 유저 입력(클릭 좌표) 대기열 </summary>
        private Queue<GridPos> _inputQueue = new Queue<GridPos>();

        /// <summary> 화면에 요청할 시각적 연출 액션 리스트 </summary>
        private List<BoardViewAction> _views = new List<BoardViewAction>();

        /// <summary> 지금까지의 모든 유저 조작 기록 (리플레이용) </summary>
        private List<InputRecord> _recordedInputs = new List<InputRecord>();

        /// <summary> 현재 보드 프레임 번호 </summary>
        private ulong _frameCount;

        /// <summary> 단일 클릭 조작 시 선택된 첫 번째 좌표 </summary>
        private GridPos? _selectedPos = null;

        /// <summary> 현재 보드에 적용된 게임 사양서 </summary>
        internal GameSpec gameSpec;

        /// <summary>
        /// 보드 내부 로직을 수행하며 로그를 전달합니다.
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        /// <summary>
        /// 전달된 사양서를 바탕으로 보드 및 목표를 초기화합니다.
        /// </summary>
        /// <param name="spec">게임 설정 정보</param>
        public void Initialize(GameSpec spec)
        {
            _inputQueue = new Queue<GridPos>();
            Cells = new Dictionary<GridPos, PuzzleCell>();
            _views = new List<BoardViewAction>();
            _recordedInputs = new List<InputRecord>();
            _frameCount = 0;
            _selectedPos = null;
            gameSpec = spec;
            Random = new PuzzleRandom(0);
            Objective = new ObjectiveManager(gameSpec?.rule.objectives);

            if (gameSpec?.stageData != null)
            {
                Width = gameSpec.stageData.stage_width;
                Height = gameSpec.stageData.stage_height;
                foreach (var cellData in gameSpec.stageData.cells)
                {
                    GridPos pos = new GridPos(cellData.x, cellData.y);
                    PuzzleCell cell = new PuzzleCell(pos) 
                    { 
                        CellType = (CellType)cellData.cell_type 
                    };

                    if (cell.CellType == CellType.Generator && cellData.generator_block_ids != null)
                    {
                        cell.generatorBlockIds.AddRange(cellData.generator_block_ids);
                    }

                    if (!string.IsNullOrEmpty(cellData.block_id))
                    {
                        BlockData bData = gameSpec.GetBlock(cellData.block_id);
                        if (bData != null)
                        {
                            cell.Block = PuzzleBlockFactory.Create(bData);
                        }
                    }
                    Cells[pos] = cell;
                }
            }
            State = BoardState.Matching;
        }

        /// <summary>
        /// 외부에서 입력 좌표를 전달받아 처리 대기열에 추가합니다.
        /// </summary>
        /// <param name="input">입력된 그리드 좌표</param>
        public void Input(GridPos input)
        {
            if (State != BoardState.Waiting)
            {
                return;
            }

            // 드래그 중 중복 좌표 방지
            if (_inputQueue.Count > 0 && _inputQueue.Last() == input)
            {
                return;
            }
            
            _inputQueue.Enqueue(input);
            _recordedInputs.Add(new InputRecord(_frameCount, input));
        }

        /// <summary>
        /// 입력 종료 시 호출되어 대기열의 좌표들을 바탕으로 스왑 로직을 처리합니다.
        /// </summary>
        /// <returns>조작이 유효했는지 여부</returns>
        public bool InputEnd()
        {
            if (State != BoardState.Waiting || _inputQueue.Count == 0)
            {
                _inputQueue.Clear();
                return false;
            }

            List<GridPos> path = _inputQueue.ToList();
            _inputQueue.Clear();

            if (path.Count > 1)
            {
                // [Case 1] 드래그 조작
                GridPos first = path[0];
                foreach (var pos in path.Skip(1))
                {
                    if (IsAdjacent(first, pos))
                    {
                        ProcessSwapInput(first, pos);
                        _selectedPos = null;
                        return true;
                    }
                }
            }
            else if (path.Count == 1)
            {
                // [Case 2] 단일 클릭 조작
                GridPos clickedPos = path[0];

                if (_selectedPos == null)
                {
                    _selectedPos = clickedPos;
                    Log($"[ThreeMatchBoard] 블럭 선택됨: {clickedPos}");
                }
                else
                {
                    GridPos first = _selectedPos.Value;
                    if (first == clickedPos)
                    {
                        _selectedPos = null;
                        Log($"[ThreeMatchBoard] 선택 취소");
                    }
                    else if (IsAdjacent(first, clickedPos))
                    {
                        ProcessSwapInput(first, clickedPos);
                        _selectedPos = null;
                    }
                    else
                    {
                        _selectedPos = clickedPos;
                        Log($"[ThreeMatchBoard] 선택 변경: {clickedPos}");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 두 블럭을 스왑하고 매칭 여부를 판별합니다. 매칭 실패 시 원상복구합니다.
        /// </summary>
        /// <param name="first">첫 번째 좌표</param>
        /// <param name="second">두 번째 좌표</param>
        private void ProcessSwapInput(GridPos first, GridPos second)
        {
            var cellA = GetCell(first);
            var cellB = GetCell(second);
            if (cellA?.Block == null || cellB?.Block == null)
            {
                return;
            }

            SwapBlocks(first, second);

            if (FindMatches().Count > 0)
            {
                State = BoardState.Matching;
            }
            else
            {
                Log("[ThreeMatchBoard] 매칭 실패. 원상복구.");
                SwapBlocks(first, second);
            }
        }

        /// <summary>
        /// 매 프레임마다 보드의 논리 상태를 업데이트합니다. (Logic Frame)
        /// </summary>
        public void Update()
        {
            _frameCount++;
            if (_lastLoggedState != State)
            {
                Log($"[ThreeMatchBoard] State Changed: {_lastLoggedState} -> {State} (Frame: {_frameCount})");
                _lastLoggedState = State;
            }

            switch (State)
            {
                case BoardState.Waiting:
                    if (Objective != null && Objective.IsAllObjectivesCleared())
                    {
                        State = BoardState.Finish;
                    }
                    break;

                case BoardState.Matching:
                    Log($"[ThreeMatchBoard] Entering ProcessMatching (Frame: {_frameCount})");
                    if (ProcessMatching()) 
                    {
                        State = BoardState.Falling;
                    }
                    else 
                    {
                        State = HasEmptyCell() ? BoardState.Falling : BoardState.Waiting;
                    }
                    break;

                case BoardState.Falling:
                    if (ProcessFalling())
                    {
                        State = BoardState.Filling;
                    }
                    break;

                case BoardState.Filling:
                    if (ProcessFilling())
                    {
                        State = BoardState.Falling;
                    }
                    else
                    {
                        State = BoardState.Matching;
                    }
                    break;
            }
        }

        /// <summary>
        /// 보드 상에 비어있는 셀이 있는지 확인합니다.
        /// </summary>
        private bool HasEmptyCell()
        {
            return Cells.Values.Any(c => (c.CellType == CellType.Normal || c.CellType == CellType.Generator) && c.Block == null);
        }

        /// <summary>
        /// 현재 매칭된 블럭들을 찾아 제거하고 점수를 업데이트합니다.
        /// </summary>
        /// <returns>매칭이 발생했는지 여부</returns>
        private bool ProcessMatching()
        {
            var matches = FindMatches();
            if (matches.Count == 0)
            {
                Log($"[ThreeMatchBoard] No matches found (Frame: {_frameCount})");
                return false;
            }

            Log($"[ThreeMatchBoard] Found {matches.Count} blocks to destroy (Frame: {_frameCount})");
            foreach (var pos in matches)
            {
                var cell = GetCell(pos);
                if (cell?.Block != null)
                {
                    Log($"[ThreeMatchBoard] Destroying block '{cell.Block.GetBlockId()}' at {pos}");
                    Objective.OnBlockDestroyed(cell.Block.GetBlockId());
                    cell.Block = null;
                    AddView(new BoardViewAction 
                    { 
                        type = ViewType.Destroy, 
                        frame = (uint)_frameCount, 
                        position = pos 
                    });
                }
            }
            return true;
        }

        /// <summary>
        /// 가로 및 세로 방향으로 3개 이상 연속된 블럭들을 찾습니다.
        /// </summary>
        /// <returns>매칭된 좌표들의 집합</returns>
        private HashSet<GridPos> FindMatches()
        {
            HashSet<GridPos> matches = new HashSet<GridPos>();
            
            // 가로 탐색
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width - 2; x++)
                {
                    string id = GetBlockIdAt(new GridPos(x, y));
                    if (id != null && id == GetBlockIdAt(new GridPos(x + 1, y)) && id == GetBlockIdAt(new GridPos(x + 2, y)))
                    {
                        Log($"[ThreeMatchBoard] Horizontal match found at Y:{y}, X:{x}~{x+2} (ID:{id})");
                        matches.Add(new GridPos(x, y)); 
                        matches.Add(new GridPos(x + 1, y)); 
                        matches.Add(new GridPos(x + 2, y));

                        int nx = x + 3; 
                        while (nx < Width && GetBlockIdAt(new GridPos(nx, y)) == id) 
                        { 
                            matches.Add(new GridPos(nx, y)); 
                            nx++; 
                        }
                        x = nx - 1;
                    }
                }
            }

            // 세로 탐색
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 2; y++)
                {
                    string id = GetBlockIdAt(new GridPos(x, y));
                    if (id != null && id == GetBlockIdAt(new GridPos(x, y + 1)) && id == GetBlockIdAt(new GridPos(x, y + 2)))
                    {
                        Log($"[ThreeMatchBoard] Vertical match found at X:{x}, Y:{y}~{y+2} (ID:{id})");
                        matches.Add(new GridPos(x, y)); 
                        matches.Add(new GridPos(x, y + 1)); 
                        matches.Add(new GridPos(x, y + 2));

                        int ny = y + 3; 
                        while (ny < Height && GetBlockIdAt(new GridPos(x, ny)) == id) 
                        { 
                            matches.Add(new GridPos(x, ny)); 
                            ny++; 
                        }
                        y = ny - 1;
                    }
                }
            }
            return matches;
        }

        /// <summary>
        /// 특정 좌표의 블럭 ID를 가져옵니다. 매칭 불가능한 셀은 null을 반환합니다.
        /// </summary>
        private string GetBlockIdAt(GridPos pos)
        {
            var cell = GetCell(pos);
            if (cell == null || cell.Block == null)
            {
                return null;
            }
            
            if (cell.CellType == CellType.Normal || cell.CellType == CellType.Generator)
            {
                return cell.Block.GetBlockId();
            }
            return null;
        }

        /// <summary>
        /// 비어있는 공간을 위쪽 블럭들이 내려와서 채우도록 처리합니다.
        /// </summary>
        /// <returns>모든 블럭이 이동을 멈췄는지 여부 (true 시 다음 상태로)</returns>
        private bool ProcessFalling()
        {
            bool anyMoved = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var currentCell = GetCell(new GridPos(x, y));
                    if (currentCell != null && (currentCell.CellType == CellType.Normal || currentCell.CellType == CellType.Generator) && currentCell.Block == null)
                    {
                        for (int ay = y + 1; ay < Height; ay++)
                        {
                            var aboveCell = GetCell(new GridPos(x, ay));
                            if (aboveCell != null && aboveCell.Block != null)
                            {
                                currentCell.Block = aboveCell.Block;
                                aboveCell.Block = null;
                                AddView(new BoardViewAction 
                                { 
                                    type = ViewType.Move, 
                                    frame = (uint)_frameCount, 
                                    position = new GridPos(x, ay), 
                                    targetPosition = new GridPos(x, y) 
                                });
                                anyMoved = true; 
                                break;
                            }
                        }
                    }
                }
            }
            return !anyMoved;
        }

        /// <summary>
        /// 생성기(Generator) 셀에서 새로운 블럭을 생성합니다.
        /// </summary>
        /// <returns>블럭이 생성되었는지 여부</returns>
        private bool ProcessFilling()
        {
            bool generated = false;
            foreach (var cell in Cells.Values.Where(c => c.CellType == CellType.Generator && c.Block == null))
            {
                cell.Block = cell.GenerateBlock(gameSpec, Random);
                if (cell.Block != null)
                {
                    AddView(new BoardViewAction 
                    { 
                        type = ViewType.Create, 
                        frame = (uint)_frameCount, 
                        position = cell.Position 
                    });
                    generated = true;
                }
            }
            return generated;
        }

        /// <summary> 보드 업데이트 일시 정지 </summary>
        public void Pause(bool pause) { }

        /// <summary> 리플레이용 조작 데이터 반환 </summary>
        public List<InputRecord> GetRecordedInputs() => new List<InputRecord>(_recordedInputs);

        /// <summary> 화면에 연출할 액션 리스트 반환 및 비움 </summary>
        public List<BoardViewAction> FetchActions() 
        { 
            var res = _views.OrderBy(v => v.frame).ToList(); 
            _views.Clear(); 
            return res; 
        }

        /// <summary> 시각적 액션 추가 </summary>
        public void AddView(BoardViewAction view) => _views.Add(view);

        /// <summary> 좌표에 해당하는 셀 반환 </summary>
        public PuzzleCell GetCell(GridPos pos) => Cells.TryGetValue(pos, out var c) ? c : null;

        /// <summary> 두 좌표가 인접한지(상하좌우) 확인 </summary>
        private bool IsAdjacent(GridPos a, GridPos b) 
        {
            return (Math.Abs(a.X - b.X) == 1 && a.Y == b.Y) || (Math.Abs(a.Y - b.Y) == 1 && a.X == b.X);
        }
        
        /// <summary> 두 좌표의 블럭을 물리적으로 스왑 </summary>
        private void SwapBlocks(GridPos a, GridPos b)
        {
            var ca = GetCell(a); 
            var cb = GetCell(b);
            if (ca == null || cb == null)
            {
                return;
            }

            var t = ca.Block; 
            ca.Block = cb.Block; 
            cb.Block = t;

            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = a, targetPosition = b });
            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = b, targetPosition = a });
        }
    }
}
