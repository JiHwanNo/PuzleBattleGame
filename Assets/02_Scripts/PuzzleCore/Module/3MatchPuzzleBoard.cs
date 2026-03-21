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

        /// <summary> 입력된 좌표를 순차적으로 처리하기 위한 큐 </summary>
        private Queue<GridPos> _inputQueue;

        /// <summary> 뷰(View)에 전달할 보드 상태 변화 기록 리스트 </summary>
        private List<BoardViewAction> _views;

        /// <summary> 리플레이를 위해 기록된 유저의 조작 내역 </summary>
        private List<InputRecord> _recordedInputs = new List<InputRecord>();

        /// <summary> 게임 시작 후 누적된 프레임 수 </summary>
        private ulong _frameCount;

        /// <summary> 현재 보드가 로직 처리 중인지 여부 (State가 Waiting이 아님을 의미) </summary>
        private bool _isProcessing => State != BoardState.Waiting;

        /// <summary> 첫 번째로 클릭되어 선택된 셀의 좌표 </summary>
        private GridPos? _selectedPos = null;

        /// <summary> 현재 적용 중인 게임 사양서 데이터 </summary>
        internal GameSpec gameSpec;

        /// <summary>
        /// 게임 사양서(GameSpec)를 바탕으로 보드를 초기화합니다.
        /// </summary>
        /// <param name="spec">스테이지 및 규칙 정보가 담긴 사양서 객체</param>
        public void Initialize(GameSpec spec)
        {
            _inputQueue = new Queue<GridPos>();
            Cells = new Dictionary<GridPos, PuzzleCell>();
            _views = new List<BoardViewAction>();
            _recordedInputs = new List<InputRecord>();
            _frameCount = 0;
            _selectedPos = null;
            gameSpec = spec;

            // 난수 생성기 초기화
            Random = new PuzzleRandom(0);

            // 목표 관리자 초기화
            Objective = new ObjectiveManager(gameSpec?.rule.objectives);

            if (gameSpec != null && gameSpec.stageData != null)
            {
                Width = gameSpec.stageData.stage_width;
                Height = gameSpec.stageData.stage_height;

                if (gameSpec.stageData.cells != null)
                {
                    foreach (var cellData in gameSpec.stageData.cells)
                    {
                        GridPos pos = new GridPos(cellData.x, cellData.y);
                        PuzzleCell cell = new PuzzleCell(pos);
                        
                        cell.CellType = (CellType)cellData.cell_type;

                        // 생성기 설정 로드
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
            }

            // 초기 보드 매칭 제거를 위해 Matching 상태로 시작
            State = BoardState.Matching;
        }

        /// <summary>
        /// 외부(Controller)에서 입력 좌표를 전달받아 처리 대기열에 추가합니다.
        /// </summary>
        /// <param name="input">입력된 타일의 그리드 좌표</param>
        public void Input(GridPos input)
        {
            if (State == BoardState.Waiting)
            {
                _inputQueue.Enqueue(input);
                _recordedInputs.Add(new InputRecord(_frameCount, input));
            }
        }

        /// <summary>
        /// 대기열에 쌓인 입력들을 모두 처리합니다.
        /// </summary>
        public bool InputEnd()
        {
            if (State != BoardState.Waiting) return false;

            bool processed = false;
            while (_inputQueue.Count > 0)
            {
                GridPos input = _inputQueue.Dequeue();
                ProcessInput(input);
                processed = true;
            }
            return processed;
        }

        public List<InputRecord> GetRecordedInputs()
        {
            return new List<InputRecord>(_recordedInputs);
        }

        /// <summary>
        /// 매 프레임마다 보드의 상태를 업데이트합니다.
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
                        UnityEngine.Debug.Log("[ThreeMatchBoard] 모든 목표 달성!");
                    }
                    break;

                case BoardState.Matching:
                    if (ProcessMatching())
                    {
                        State = BoardState.Falling;
                    }
                    else
                    {
                        // 매칭이 없어도 구멍이 있다면 떨어뜨려야 함
                        if (HasEmptyCell()) State = BoardState.Falling;
                        else State = BoardState.Waiting;
                    }
                    break;

                case BoardState.Falling:
                    if (ProcessFalling())
                    {
                        // 낙하가 완료되면 생성기에서 보충
                        State = BoardState.Filling;
                    }
                    break;

                case BoardState.Filling:
                    if (ProcessFilling())
                    {
                        // 보충된 게 있다면 다시 떨어뜨림
                        State = BoardState.Falling;
                    }
                    else
                    {
                        // 보충할 게 없다면 다시 매칭 체크 (콤보 확인)
                        State = BoardState.Matching;
                    }
                    break;
            }
        }

        private bool HasEmptyCell()
        {
            return Cells.Values.Any(c => c.CellType == CellType.Normal && c.Block == null);
        }

        private bool ProcessMatching()
        {
            HashSet<GridPos> matchedPositions = FindMatches();
            if (matchedPositions.Count > 0)
            {
                foreach (var pos in matchedPositions)
                {
                    var cell = GetCell(pos);
                    if (cell != null && cell.Block != null)
                    {
                        Objective.OnBlockDestroyed(cell.Block.GetBlockId());
                        cell.Block = null;
                        AddView(new BoardViewAction { type = ViewType.Destroy, frame = (uint)_frameCount, position = pos });
                    }
                }
                return true;
            }
            return false;
        }

        private HashSet<GridPos> FindMatches()
        {
            HashSet<GridPos> matches = new HashSet<GridPos>();
            // 가로 탐색
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width - 2; x++)
                {
                    string id1 = GetBlockIdAt(new GridPos(x, y));
                    if (string.IsNullOrEmpty(id1)) continue;
                    if (id1 == GetBlockIdAt(new GridPos(x + 1, y)) && id1 == GetBlockIdAt(new GridPos(x + 2, y)))
                    {
                        matches.Add(new GridPos(x, y));
                        matches.Add(new GridPos(x + 1, y));
                        matches.Add(new GridPos(x + 2, y));
                        int nextX = x + 3;
                        while (nextX < Width && GetBlockIdAt(new GridPos(nextX, y)) == id1) { matches.Add(new GridPos(nextX, y)); nextX++; }
                        x = nextX - 1;
                    }
                }
            }
            // 세로 탐색
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 2; y++)
                {
                    string id1 = GetBlockIdAt(new GridPos(x, y));
                    if (string.IsNullOrEmpty(id1)) continue;
                    if (id1 == GetBlockIdAt(new GridPos(x, y + 1)) && id1 == GetBlockIdAt(new GridPos(x, y + 2)))
                    {
                        matches.Add(new GridPos(x, y));
                        matches.Add(new GridPos(x, y + 1));
                        matches.Add(new GridPos(x, y + 2));
                        int nextY = y + 3;
                        while (nextY < Height && GetBlockIdAt(new GridPos(x, nextY)) == id1) { matches.Add(new GridPos(x, nextY)); nextY++; }
                        y = nextY - 1;
                    }
                }
            }
            return matches;
        }

        private string GetBlockIdAt(GridPos pos)
        {
            var cell = GetCell(pos);
            if (cell == null || cell.Block == null) return null;
            if (cell.CellType == CellType.Normal || cell.CellType == CellType.Generator) return cell.Block.GetBlockId();
            return null;
        }

        private bool ProcessFalling()
        {
            bool anyMoved = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var currentCell = GetCell(new GridPos(x, y));
                    if (currentCell != null && currentCell.CellType == CellType.Normal && currentCell.Block == null)
                    {
                        for (int aboveY = y + 1; aboveY < Height; aboveY++)
                        {
                            var aboveCell = GetCell(new GridPos(x, aboveY));
                            if (aboveCell != null && aboveCell.Block != null)
                            {
                                currentCell.Block = aboveCell.Block;
                                aboveCell.Block = null;
                                AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = new GridPos(x, aboveY), targetPosition = new GridPos(x, y) });
                                anyMoved = true;
                                break;
                            }
                        }
                    }
                }
            }
            return !anyMoved;
        }

        private bool ProcessFilling()
        {
            bool generated = false;
            foreach (var cell in Cells.Values)
            {
                if (cell.CellType == CellType.Generator && cell.Block == null)
                {
                    PuzzleBlock newBlock = cell.GenerateBlock(gameSpec, Random);
                    if (newBlock != null)
                    {
                        cell.Block = newBlock;
                        AddView(new BoardViewAction { type = ViewType.Create, frame = (uint)_frameCount, position = cell.Position });
                        generated = true;
                    }
                }
            }
            return generated;
        }

        public void Pause(bool pause) { }

        private void ProcessInput(GridPos input)
        {
            try
            {
                var inputCell = GetCell(input);
                if (inputCell == null || (inputCell.CellType != CellType.Normal && inputCell.CellType != CellType.Generator) || inputCell.Block == null) return;
                if (_selectedPos == null) { _selectedPos = input; return; }
                GridPos firstPos = _selectedPos.Value;
                _selectedPos = null;
                if (firstPos == input) return;
                var firstBlock = GetCell(firstPos)?.Block;
                if (firstBlock != null && IsAdjacent(firstPos, input) && firstBlock is ISwappableBlock swappable)
                {
                    SwapBlocks(firstPos, input);
                    if (FindMatches().Count > 0) State = BoardState.Matching;
                    else SwapBlocks(firstPos, input);
                }
                else _selectedPos = input;
            }
            catch (System.Exception e) { UnityEngine.Debug.LogError(e.Message); }
        }

        private bool IsAdjacent(GridPos a, GridPos b)
        {
            return (System.Math.Abs(a.X - b.X) == 1 && a.Y == b.Y) || (System.Math.Abs(a.Y - b.Y) == 1 && a.X == b.X);
        }

        public List<BoardViewAction> FetchActions() { var result = _views.OrderBy(v => v.frame).ToList(); _views.Clear(); return result; }
        public void AddView(BoardViewAction view) { _views.Add(view); }
        private void SwapBlocks(GridPos a, GridPos b)
        {
            var cellA = GetCell(a); var cellB = GetCell(b);
            if (cellA == null || cellB == null) return;
            var temp = cellA.Block; cellA.Block = cellB.Block; cellB.Block = temp;
            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = a, targetPosition = b });
            AddView(new BoardViewAction { type = ViewType.Move, frame = (uint)_frameCount, position = b, targetPosition = a });
        }

        public PuzzleCell GetCell(GridPos pos) { return Cells.TryGetValue(pos, out PuzzleCell cell) ? cell : null; }
    }
}
