using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Core
{
    /// <summary>
    /// 3매치(Three Match) 퍼즐 게임의 보드 상태와 핵심 로직을 관리하는 Model 클래스입니다.
    /// IPuzzleBoardInterface를 구현하며, 입력 처리, 스왑, 매칭 판정 기능을 포함합니다.
    /// </summary>
    public class ThreeMatchPuzzleBoard : IPuzzleBoard
    {
        public BoardState State { get; private set; } = BoardState.Waiting;
        public PuzzleRandom Random { get; private set; }
        public ObjectiveManager Objective { get; private set; }
        public Dictionary<GridPos, PuzzleCell> Cells { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Queue<GridPos> _inputQueue = new Queue<GridPos>();
        private List<BoardViewAction> _views = new List<BoardViewAction>();
        private List<InputRecord> _recordedInputs = new List<InputRecord>();
        private ulong _frameCount;
        private GridPos? _selectedPos = null;
        internal GameSpec gameSpec;

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
                    PuzzleCell cell = new PuzzleCell(pos) { CellType = (CellType)cellData.cell_type };
                    if (cell.CellType == CellType.Generator && cellData.generator_block_ids != null)
                        cell.generatorBlockIds.AddRange(cellData.generator_block_ids);
                    if (!string.IsNullOrEmpty(cellData.block_id))
                    {
                        BlockData bData = gameSpec.GetBlock(cellData.block_id);
                        if (bData != null) cell.Block = PuzzleBlockFactory.Create(bData);
                    }
                    Cells[pos] = cell;
                }
            }
            State = BoardState.Matching;
        }

        public void Input(GridPos input)
        {
            if (State != BoardState.Waiting) return;
            if (_inputQueue.Count > 0 && _inputQueue.Last() == input) return;
            _inputQueue.Enqueue(input);
            _recordedInputs.Add(new InputRecord(_frameCount, input));
        }

        public bool InputEnd()
        {
            if (State != BoardState.Waiting || _inputQueue.Count == 0)
            {
                _inputQueue.Clear();
                return false;
            }

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

        private void ProcessSwapInput(GridPos first, GridPos second)
        {
            if (first == second) return;
            var cellA = GetCell(first);
            var cellB = GetCell(second);
            if (cellA?.Block == null || cellB?.Block == null) return;

            if (IsAdjacent(first, second))
            {
                SwapBlocks(first, second);
                if (FindMatches().Count > 0) State = BoardState.Matching;
                else SwapBlocks(first, second);
            }
        }

        public void Update()
        {
            _frameCount++;
            switch (State)
            {
                case BoardState.Waiting:
                    if (Objective != null && Objective.IsAllObjectivesCleared()) State = BoardState.Finish;
                    break;
                case BoardState.Matching:
                    if (ProcessMatching()) State = BoardState.Falling;
                    else State = HasEmptyCell() ? BoardState.Falling : BoardState.Waiting;
                    break;
                case BoardState.Falling:
                    if (ProcessFalling()) State = BoardState.Filling;
                    break;
                case BoardState.Filling:
                    State = ProcessFilling() ? BoardState.Falling : BoardState.Matching;
                    break;
            }
        }

        private bool HasEmptyCell() => Cells.Values.Any(c => c.CellType == CellType.Normal && c.Block == null);

        private bool ProcessMatching()
        {
            var matches = FindMatches();
            if (matches.Count == 0) return false;
            foreach (var pos in matches)
            {
                var cell = GetCell(pos);
                if (cell?.Block != null)
                {
                    Objective.OnBlockDestroyed(cell.Block.GetBlockId());
                    cell.Block = null;
                    AddView(new BoardViewAction { type = ViewType.Destroy, frame = (uint)_frameCount, position = pos });
                }
            }
            return true;
        }

        private HashSet<GridPos> FindMatches()
        {
            HashSet<GridPos> matches = new HashSet<GridPos>();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width - 2; x++)
                {
                    string id = GetBlockIdAt(new GridPos(x, y));
                    if (id != null && id == GetBlockIdAt(new GridPos(x + 1, y)) && id == GetBlockIdAt(new GridPos(x + 2, y)))
                    {
                        matches.Add(new GridPos(x, y)); matches.Add(new GridPos(x + 1, y)); matches.Add(new GridPos(x + 2, y));
                        int nx = x + 3; while (nx < Width && GetBlockIdAt(new GridPos(nx, y)) == id) { matches.Add(new GridPos(nx, y)); nx++; }
                        x = nx - 1;
                    }
                }
            }
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 2; y++)
                {
                    string id = GetBlockIdAt(new GridPos(x, y));
                    if (id != null && id == GetBlockIdAt(new GridPos(x, y + 1)) && id == GetBlockIdAt(new GridPos(x, y + 2)))
                    {
                        matches.Add(new GridPos(x, y)); matches.Add(new GridPos(x, y + 1)); matches.Add(new GridPos(x, y + 2));
                        int ny = y + 3; while (ny < Height && GetBlockIdAt(new GridPos(x, ny)) == id) { matches.Add(new GridPos(x, ny)); ny++; }
                        y = ny - 1;
                    }
                }
            }
            return matches;
        }

        private string GetBlockIdAt(GridPos pos)
        {
            var cell = GetCell(pos);
            return (cell?.Block != null && (cell.CellType == CellType.Normal || cell.CellType == CellType.Generator)) ? cell.Block.GetBlockId() : null;
        }

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
                                AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = new GridPos(x, ay), targetPosition = new GridPos(x, y) });
                                moved = true; break;
                            }
                        }
                    }
                }
            }
            return !moved;
        }

        private bool ProcessFilling()
        {
            bool gen = false;
            foreach (var cell in Cells.Values.Where(c => c.CellType == CellType.Generator && c.Block == null))
            {
                cell.Block = cell.GenerateBlock(gameSpec, Random);
                if (cell.Block != null)
                {
                    AddView(new BoardViewAction { type = ViewType.Create, frame = (uint)_frameCount, position = cell.Position });
                    gen = true;
                }
            }
            return gen;
        }

        public void Pause(bool pause) { }
        public List<InputRecord> GetRecordedInputs() => new List<InputRecord>(_recordedInputs);
        public List<BoardViewAction> FetchActions() { var res = _views.OrderBy(v => v.frame).ToList(); _views.Clear(); return res; }
        public void AddView(BoardViewAction view) => _views.Add(view);
        public PuzzleCell GetCell(GridPos pos) => Cells.TryGetValue(pos, out var c) ? c : null;
        private bool IsAdjacent(GridPos a, GridPos b) => (System.Math.Abs(a.X - b.X) == 1 && a.Y == b.Y) || (System.Math.Abs(a.Y - b.Y) == 1 && a.X == b.X);
        private void SwapBlocks(GridPos a, GridPos b)
        {
            var ca = GetCell(a); var cb = GetCell(b);
            var t = ca.Block; ca.Block = cb.Block; cb.Block = t;
            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = a, targetPosition = b });
            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = b, targetPosition = a });
        }
    }
}
