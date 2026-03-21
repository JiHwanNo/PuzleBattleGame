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

        /// <summary> 보드 내부 로직에서 발생하는 로그를 외부로 전달합니다. </summary>
        public Action<string> OnLog { get; set; }

        /// <summary> 유저 입력 좌표 대기열 </summary>
        private Queue<GridPos> _inputQueue = new Queue<GridPos>();

        /// <summary> 화면에 요청할 시각적 연출 액션 리스트 </summary>
        private List<BoardViewAction> _views = new List<BoardViewAction>();

        /// <summary> 지금까지의 모든 유저 조작 기록 </summary>
        private List<InputRecord> _recordedInputs = new List<InputRecord>();

        /// <summary> 현재 보드 프레임 번호 </summary>
        private ulong _frameCount;

        /// <summary> 단일 클릭 조작 시 선택된 좌표 </summary>
        private GridPos? _selectedPos = null;

        /// <summary> 현재 보드에 적용된 게임 사양서 </summary>
        internal GameSpec gameSpec;

        /// <summary>
        /// 보드 내부 로직을 수행하며 로그를 전달합니다.
        /// </summary>
        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        /// <summary>
        /// 전달된 사양서를 바탕으로 보드를 초기화합니다.
        /// </summary>
        public void Initialize(GameSpec spec)
        {
            _inputQueue = new Queue<GridPos>();
            Cells = new Dictionary<GridPos, PuzzleCell>();
            _views = new List<BoardViewAction>();
            _recordedInputs = new List<InputRecord>();
            _frameCount = 0;
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
        /// 외부에서 입력 좌표를 전달받아 대기열에 추가합니다.
        /// </summary>
        public void Input(GridPos input)
        {
            if (State != BoardState.Waiting)
            {
                return;
            }

            if (_inputQueue.Count > 0 && _inputQueue.Last() == input)
            {
                return;
            }

            _inputQueue.Enqueue(input);
            _recordedInputs.Add(new InputRecord(_frameCount, input));
        }

        /// <summary>
        /// 입력 종료 시 대기열의 좌표들을 바탕으로 스왑 로직을 실행합니다.
        /// </summary>
        public bool InputEnd()
        {
            if (State != BoardState.Waiting || _inputQueue.Count == 0)
            {
                _inputQueue.Clear();
                return false;
            }

            // 원본 로직: 대기열에서 인접한 두 좌표를 찾아 스왑 시도
            GridPos first = _inputQueue.Dequeue();
            GridPos last = first;
            while (_inputQueue.Count > 0)
            {
                GridPos current = _inputQueue.Dequeue();
                if (IsAdjacent(first, current))
                {
                    last = current;
                    break;
                }
            }
            _inputQueue.Clear();

            ProcessSwapInput(first, last);
            return true;
        }

        /// <summary>
        /// 두 블럭의 스왑을 처리하고 매칭 여부를 확인합니다.
        /// </summary>
        private void ProcessSwapInput(GridPos first, GridPos second)
        {
            if (first == second)
            {
                return;
            }

            var cellA = GetCell(first);
            var cellB = GetCell(second);
            if (cellA?.Block == null || cellB?.Block == null)
            {
                return;
            }

            if (IsAdjacent(first, second))
            {
                SwapBlocks(first, second);
                
                if (FindMatches().Count > 0)
                {
                    State = BoardState.Matching;
                }
                else
                {
                    // 매칭 실패 시 원상복구
                    SwapBlocks(first, second);
                }
            }
        }

        /// <summary>
        /// 매 프레임마다 보드의 논리 상태를 업데이트합니다.
        /// </summary>
        public void Update()
        {
            _frameCount++;
            switch (State)
            {
                case BoardState.Waiting:
                    if (Objective != null && Objective.IsAllObjectivesCleared())
                    {
                        State = BoardState.Finish;
                    }
                    break;

                case BoardState.Matching:
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
                    State = ProcessFilling() ? BoardState.Falling : BoardState.Matching;
                    break;
            }
        }

        /// <summary>
        /// 보드 상에 비어있는 셀이 있는지 확인합니다.
        /// </summary>
        private bool HasEmptyCell()
        {
            return Cells.Values.Any(c => c.CellType == CellType.Normal && c.Block == null);
        }

        /// <summary>
        /// 매칭된 블럭들을 제거하고 연출 액션을 생성합니다.
        /// </summary>
        private bool ProcessMatching()
        {
            var matches = FindMatches();
            if (matches.Count == 0)
            {
                return false;
            }

            foreach (var pos in matches)
            {
                var cell = GetCell(pos);
                if (cell?.Block != null)
                {
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
        /// 가로/세로 방향으로 3개 이상 연속된 블럭들을 찾습니다.
        /// </summary>
        private HashSet<GridPos> FindMatches()
        {
            HashSet<GridPos> matches = new HashSet<GridPos>();
            
            // 가로 탐색 (Allman 스타일 적용)
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width - 2; x++)
                {
                    string id = GetBlockIdAt(new GridPos(x, y));
                    if (id != null && id == GetBlockIdAt(new GridPos(x + 1, y)) && id == GetBlockIdAt(new GridPos(x + 2, y)))
                    {
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
        /// 특정 좌표의 블럭 아이디를 반환합니다.
        /// </summary>
        private string GetBlockIdAt(GridPos pos)
        {
            var cell = GetCell(pos);
            bool isMatchable = cell?.Block != null && (cell.CellType == CellType.Normal || cell.CellType == CellType.Generator);
            return isMatchable ? cell.Block.GetBlockId() : null;
        }

        /// <summary>
        /// 블럭들을 아래로 떨어뜨리는 로직을 수행합니다.
        /// </summary>
        private bool ProcessFalling()
        {
            bool moved = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (GetCell(new GridPos(x, y))?.CellType == CellType.Normal && GetCell(new GridPos(x, y)).Block == null)
                    {
                        for (int ay = y + 1; ay < Height; ay++)
                        {
                            if (GetCell(new GridPos(x, ay))?.Block != null)
                            {
                                GetCell(new GridPos(x, y)).Block = GetCell(new GridPos(x, ay)).Block;
                                GetCell(new GridPos(x, ay)).Block = null;
                                AddView(new BoardViewAction 
                                { 
                                    type = ViewType.Move, 
                                    frame = (uint)_frameCount, 
                                    position = new GridPos(x, ay), 
                                    targetPosition = new GridPos(x, y) 
                                });
                                moved = true; 
                                break;
                            }
                        }
                    }
                }
            }
            return !moved;
        }

        /// <summary>
        /// 생성기에서 새로운 블럭을 채웁니다.
        /// </summary>
        private bool ProcessFilling()
        {
            bool gen = false;
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
                    gen = true;
                }
            }
            return gen;
        }

        /// <summary> 보드 업데이트 일시 정지 (미구현) </summary>
        public void Pause(bool pause) { }

        /// <summary> 기록된 모든 입력을 반환 </summary>
        public List<InputRecord> GetRecordedInputs() => new List<InputRecord>(_recordedInputs);

        /// <summary> 뷰 연출 액션을 가져오고 비움 </summary>
        public List<BoardViewAction> FetchActions() 
        { 
            var res = _views.OrderBy(v => v.frame).ToList(); 
            _views.Clear(); 
            return res; 
        }

        /// <summary> 뷰 연출 액션 추가 </summary>
        public void AddView(BoardViewAction view) => _views.Add(view);

        /// <summary> 특정 좌표의 셀 객체 반환 </summary>
        public PuzzleCell GetCell(GridPos pos) => Cells.TryGetValue(pos, out var c) ? c : null;

        /// <summary> 두 좌표가 인접한지 확인 </summary>
        private bool IsAdjacent(GridPos a, GridPos b) 
        {
            return (Math.Abs(a.X - b.X) == 1 && a.Y == b.Y) || (Math.Abs(a.Y - b.Y) == 1 && a.X == b.X);
        }

        /// <summary> 두 좌표의 블럭 위치를 물리적으로 교체 </summary>
        private void SwapBlocks(GridPos a, GridPos b)
        {
            var ca = GetCell(a); 
            var cb = GetCell(b);
            var t = ca.Block; 
            ca.Block = cb.Block; 
            cb.Block = t;

            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = a, targetPosition = b });
            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = b, targetPosition = a });
        }
    }
}
