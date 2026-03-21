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
            _frameCount = 0;
            _selectedPos = null;
            gameSpec = spec;
            State = BoardState.Waiting;

            // 난수 생성기 초기화 (임시 시드 0 사용, 추후 GameSpec 등에서 주입 가능)
            Random = new PuzzleRandom(0);

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
        }

        /// <summary>
        /// 외부(Controller)에서 입력 좌표를 전달받아 처리 대기열에 추가합니다.
        /// </summary>
        /// <param name="input">입력된 타일의 그리드 좌표</param>
        public void Input(GridPos input)
        {
            // 입력 대기 상태일 때만 입력을 받음
            if (State == BoardState.Waiting)
            {
                _inputQueue.Enqueue(input);
            }
        }

        /// <summary>
        /// 대기열에 쌓인 입력들을 모두 처리합니다.
        /// </summary>
        /// <returns>하나 이상의 입력이 처리되었다면 true를 반환합니다.</returns>
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

        /// <summary>
        /// 매 프레임마다 보드의 상태를 업데이트합니다.
        /// 상태 머신(State Machine)에 따라 매칭, 낙하, 보충 등의 로직을 순차적으로 수행합니다.
        /// </summary>
        public void Update()
        {
            _frameCount++;

            switch (State)
            {
                case BoardState.Waiting:
                    // 입력 대기 중 (별도 로직 없음)
                    break;

                case BoardState.Matching:
                    if (ProcessMatching())
                    {
                        // 매칭된 게 있다면 낙하 상태로 전환
                        State = BoardState.Falling;
                    }
                    else
                    {
                        // 더 이상 매칭될 게 없다면 입력 대기 상태로 복귀
                        State = BoardState.Waiting;
                    }
                    break;

                case BoardState.Falling:
                    if (ProcessFalling())
                    {
                        // 모든 블럭이 낙하 완료되었다면 보충 상태로 전환
                        State = BoardState.Filling;
                    }
                    break;

                case BoardState.Filling:
                    if (ProcessFilling())
                    {
                        // 모든 빈 공간이 보충되었다면 다시 매칭 판정 (콤보 체크)
                        State = BoardState.Matching;
                    }
                    break;
            }
        }

        /// <summary>
        /// 보드 전체의 매칭 여부를 검사하고 파괴 처리합니다.
        /// </summary>
        /// <returns>매칭이 발생했다면 true</returns>
        private bool ProcessMatching()
        {
            // TODO: 실제 3매치 십자 탐색 로직 구현 (현재는 임시로 false)
            return false;
        }

        /// <summary>
        /// 빈 공간으로 블럭들을 떨어뜨리는 로직을 수행합니다.
        /// </summary>
        /// <returns>낙하 로직이 완료(안정화)되었다면 true</returns>
        private bool ProcessFalling()
        {
            // TODO: 블럭 낙하(Gravity) 로직 구현
            return true; 
        }

        /// <summary>
        /// 생성기(Generator)를 통해 빈 공간에 새 블럭을 보충합니다.
        /// </summary>
        /// <returns>보충이 완료되었다면 true</returns>
        private bool ProcessFilling()
        {
            bool anyFilled = false;
            foreach (var cell in Cells.Values)
            {
                if (cell.CellType == CellType.Generator && cell.Block == null)
                {
                    PuzzleBlock newBlock = cell.GenerateBlock(gameSpec, Random);
                    if (newBlock != null)
                    {
                        cell.Block = newBlock;
                        AddView(new BoardViewAction { type = ViewType.Create, frame = (uint)_frameCount });
                        anyFilled = true;
                    }
                }
            }
            return true; // 현재는 즉시 보충되므로 항상 true 반환
        }

        /// <summary>
        /// 보드의 로직 처리를 일시 정지하거나 재개합니다.
        /// </summary>
        public void Pause(bool pause)
        {
            // 일시 정지 기능은 상태 머신 확장 시 추가 구현 가능
        }

        /// <summary>
        /// 특정 좌표에 대한 입력을 실제로 처리하는 내부 로직입니다.
        /// </summary>
        private void ProcessInput(GridPos input)
        {
            try
            {
                var inputCell = GetCell(input);
                if (inputCell == null || inputCell.CellType != CellType.Normal || inputCell.Block == null)
                    return;

                var block = inputCell.Block;

                // 1. 터치(클릭) 능력이 있는 블럭 즉시 발동
                if (_selectedPos == null && block is ITouchableBlock touchable)
                {
                    touchable.OnTouched(this, input);
                    State = BoardState.Matching; // 상태 변화 유발
                    return;
                }

                // 2. 선택 상태 처리
                if (_selectedPos == null)
                {
                    _selectedPos = input;
                    UnityEngine.Debug.Log($"[ThreeMatchBoard] 첫 번째 블럭 선택됨: {input}");
                    return;
                }

                // 3. 두 번째 클릭 시 스왑 처리
                GridPos firstPos = _selectedPos.Value;
                GridPos secondPos = input;
                _selectedPos = null;

                if (firstPos == secondPos) return;

                var firstBlock = GetCell(firstPos)?.Block;

                if (firstBlock != null && IsAdjacent(firstPos, secondPos) && firstBlock is ISwappableBlock swappable)
                {
                    SwapBlocks(firstPos, secondPos);
                    bool success = swappable.OnSwapped(this, firstPos, secondPos);
                    
                    if (success)
                    {
                        // 스왑 후 매칭 여부 판정으로 상태 전환
                        State = BoardState.Matching;
                    }
                    else
                    {
                        SwapBlocks(firstPos, secondPos); // 원상복구
                    }
                }
                else
                {
                    _selectedPos = input; // 인접하지 않으면 새로운 선택으로 변경
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[ThreeMatchBoard] 입력 처리 중 오류: {e.Message}");
            }
        }

        /// <summary>
        /// 두 좌표가 상하좌우로 인접해 있는지 확인합니다.
        /// </summary>
        private bool IsAdjacent(GridPos a, GridPos b)
        {
            int dx = System.Math.Abs(a.X - b.X);
            int dy = System.Math.Abs(a.Y - b.Y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// 발생한 View 변화 데이터들을 가져오고 내부 리스트를 비웁니다.
        /// </summary>
        /// <returns>누적된 View 데이터 리스트</returns>
        public List<BoardViewAction> FetchActions()
        {
            var result = _views.OrderBy(v => v.frame).ToList();
            _views.Clear();
            return result;
        }

        /// <summary>
        /// 화면 연출을 위해 발생한 상태 변화(BoardViewAction)를 추가합니다.
        /// </summary>
        /// <param name="view">추가할 BoardViewAction 변화 데이터</param>
        public void AddView(BoardViewAction view)
        {
            _views.Add(view);
        }

        /// <summary>
        /// 두 좌표에 있는 셀의 블럭 데이터를 서로 교환(Swap)합니다.
        /// </summary>
        private void SwapBlocks(GridPos a, GridPos b)
        {
            var cellA = GetCell(a);
            var cellB = GetCell(b);
            
            if (cellA == null || cellB == null) return;
            
            var tempBlock = cellA.Block;
            cellA.Block = cellB.Block;
            cellB.Block = tempBlock;

            // 스왑 연출 기록
            AddView(new BoardViewAction 
            { 
                type = ViewType.Move, 
                frame = (uint)_frameCount, 
                position = a, 
                targetPosition = b 
            });
            AddView(new BoardViewAction 
            { 
                type = ViewType.Move, 
                frame = (uint)_frameCount, 
                position = b, 
                targetPosition = a 
            });
        }

        /// <summary>
        /// 보드 전체를 검사하여 매치 여부를 판별합니다.
        /// </summary>
        /// <returns>매치가 발생했다면 true, 아니면 false</returns>
        private bool EvaluateBoard()
        {
            // TODO: 보드 전체 스캔 및 3매치 검사 로직 구현 예정
            return false;
        }

        /// <summary>
        /// 지정된 좌표에 해당하는 셀(Cell) 객체를 안전하게 가져옵니다.
        /// </summary>
        /// <param name="pos">가져올 셀의 그리드 좌표</param>
        /// <returns>해당 좌표의 셀 객체. 없을 경우 null 반환</returns>
        public PuzzleCell GetCell(GridPos pos)
        {
            if (Cells.TryGetValue(pos, out PuzzleCell cell))
                return cell;

            return null;
        }
    }
}