using System;
using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Core
{
    /// <summary>
    /// 3매치(Three Match) 퍼즐 게임의 보드 상태와 핵심 로직을 관리하는 Model 클래스입니다.
    /// 조작 -> 매칭 -> 낙하 -> 채우기 -> 재매칭의 순차적 루프를 통해 안정적인 연쇄 반응을 보장합니다.
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
            
            // 시작 시 매칭 체크부터 수행
            State = BoardState.Matching;
        }

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

            if (first != last)
            {
                ProcessSwapInput(first, last);
            }
            return true;
        }

        private void ProcessSwapInput(GridPos first, GridPos second)
        {
            var cellA = GetCell(first);
            var cellB = GetCell(second);
            if (cellA?.Block == null || cellB?.Block == null)
            {
                return;
            }

            // 1. 물리적 스왑
            SwapBlocks(first, second);
            
            // 2. 매칭 여부 확인
            if (FindMatches().Count > 0)
            {
                // 매칭 성공 -> 매칭 페이즈 진입 (조작 차단)
                State = BoardState.Matching;
            }
            else
            {
                // 매칭 실패 -> 원상복구
                Log("[ThreeMatchBoard] 매칭 실패. 원상복구.");
                SwapBlocks(first, second);
            }
        }

        /// <summary>
        /// 보드의 논리 상태를 순차적으로 업데이트합니다.
        /// </summary>
        public void Update()
        {
            _frameCount++;

            // 목표 달성 체크
            if (Objective != null && Objective.IsAllObjectivesCleared())
            {
                State = BoardState.Finish;
                return;
            }

            switch (State)
            {
                case BoardState.Waiting:
                    // 유저 입력을 기다리는 중
                    break;

                case BoardState.Matching:
                    // 3. 매칭 및 결과 처리
                    if (ProcessMatching())
                    {
                        // 파괴 발생 시 낙하 페이즈로 이동
                        State = BoardState.Falling;
                    }
                    else
                    {
                        // 더 이상 매칭이 없으면 대기 상태로 복귀 (안정화 완료)
                        State = BoardState.Waiting;
                    }
                    break;

                case BoardState.Falling:
                    // 4. 이동 (중력 처리)
                    if (ProcessFalling())
                    {
                        // 블럭이 하나라도 이동했다면 다시 채우기 확인
                        State = BoardState.Filling;
                    }
                    else
                    {
                        // 이동할 블럭이 없으면 바로 채우기 단계로
                        State = BoardState.Filling;
                    }
                    break;

                case BoardState.Filling:
                    // 생성기에서 블럭 보충
                    if (ProcessFilling())
                    {
                        // 블럭이 새로 생성되었다면 다시 낙하시켜야 함
                        State = BoardState.Falling;
                    }
                    else
                    {
                        // 더 이상 채울 게 없으면 다시 매칭이 생겼는지 확인 (연쇄 매칭 체크)
                        State = BoardState.Matching;
                    }
                    break;
            }
        }

        /// <summary>
        /// 보드 상에 빈 공간(비어있는 Normal/Generator 셀)이 있는지 확인합니다.
        /// </summary>
        private bool HasEmptyCell()
        {
            return Cells.Values.Any(c => (c.CellType == CellType.Normal || c.CellType == CellType.Generator) && c.Block == null);
        }

        /// <summary>
        /// 매칭된 블럭들을 제거합니다.
        /// </summary>
        /// <returns>매칭이 발생하여 제거되었는지 여부</returns>
        private bool ProcessMatching()
        {
            var matches = FindMatches();
            if (matches == null || matches.Count == 0)
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
        /// 가로/세로 3개 이상 연속된 블럭들을 찾습니다.
        /// </summary>
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

        private string GetBlockIdAt(GridPos pos)
        {
            var cell = GetCell(pos);
            bool isMatchable = cell?.Block != null && (cell.CellType == CellType.Normal || cell.CellType == CellType.Generator);
            return isMatchable ? cell.Block.GetBlockId() : null;
        }

        /// <summary>
        /// 블럭들을 아래로 낙하시킵니다.
        /// </summary>
        /// <returns>최소 하나 이상의 블럭이 이동했는지 여부</returns>
        private bool ProcessFalling()
        {
            bool anyMoved = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var currentCell = GetCell(new GridPos(x, y));
                    // 블럭이 놓일 수 있는 셀인데 비어있는 경우 위에서 찾음
                    if (currentCell != null && (currentCell.CellType == CellType.Normal || currentCell.CellType == CellType.Generator) && currentCell.Block == null)
                    {
                        for (int ay = y + 1; ay < Height; ay++)
                        {
                            var aboveCell = GetCell(new GridPos(x, ay));
                            if (aboveCell != null && aboveCell.Block != null)
                            {
                                // 블럭 이동 실행
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
            return anyMoved;
        }

        /// <summary>
        /// 생성기에서 블럭을 생성합니다.
        /// </summary>
        /// <returns>하나 이상의 블럭이 새로 생성되었는지 여부</returns>
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

        public void Pause(bool pause) { }
        public List<InputRecord> GetRecordedInputs() => new List<InputRecord>(_recordedInputs);
        public List<BoardViewAction> FetchActions() { var res = _views.OrderBy(v => v.frame).ToList(); _views.Clear(); return res; }
        public void AddView(BoardViewAction view) => _views.Add(view);
        public PuzzleCell GetCell(GridPos pos) => Cells.TryGetValue(pos, out var c) ? c : null;
        private bool IsAdjacent(GridPos a, GridPos b) => (Math.Abs(a.X - b.X) == 1 && a.Y == b.Y) || (Math.Abs(a.Y - b.Y) == 1 && a.X == b.X);

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
