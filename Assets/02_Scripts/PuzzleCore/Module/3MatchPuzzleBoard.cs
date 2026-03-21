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

        /// <summary> 현재 보드가 로직 처리 중인지 여부 </summary>
        private bool _isProcessing;

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
            _isProcessing = false;
            _selectedPos = null;
            gameSpec = spec;

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
            _inputQueue.Enqueue(input);
        }

        /// <summary>
        /// 대기열에 쌓인 입력들을 모두 처리합니다.
        /// </summary>
        /// <returns>하나 이상의 입력이 처리되었다면 true를 반환합니다.</returns>
        public bool InputEnd()
        {
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
        /// 매 프레임마다 보드의 상태를 업데이트합니다. (예: 블럭 낙하 연산 등)
        /// </summary>
        public void Update()
        {
            _frameCount++;
        }

        /// <summary>
        /// 보드의 로직 처리를 일시 정지하거나 재개합니다.
        /// </summary>
        /// <param name="pause">true일 경우 정지, false일 경우 재개</param>
        public void Pause(bool pause)
        {
            _isProcessing = pause;
        }

        /// <summary>
        /// 특정 좌표에 대한 입력을 실제로 처리하는 내부 로직입니다. (다중 입력 방식 지원)
        /// </summary>
        /// <param name="input">처리할 타일의 그리드 좌표</param>
        private void ProcessInput(GridPos input)
        {
            try
            {
                var inputCell = GetCell(input);
                if (inputCell == null || inputCell.CellType != CellType.Normal || inputCell.Block == null)
                    return;

                var block = inputCell.Block;

                // 1. 만약 첫 번째 클릭인데, 터치(클릭) 능력이 있는 블럭이면 즉시 발동
                if (_selectedPos == null && block is ITouchableBlock touchable)
                {
                    touchable.OnTouched(this, input);
                    return; // 스왑 처리 없이 여기서 종료
                }

                // 2. 터치 전용이 아니면 스왑 대기열(선택 상태)로 만듦
                if (_selectedPos == null)
                {
                    _selectedPos = input;
                    UnityEngine.Debug.Log($"[ThreeMatchBoard] 첫 번째 블럭 선택됨: {input.X}, {input.Y}");
                    return;
                }

                // 3. 두 번째 클릭 시 스왑 처리
                GridPos firstPos = _selectedPos.Value;
                GridPos secondPos = input;
                _selectedPos = null;

                if (firstPos == secondPos)
                {
                    UnityEngine.Debug.Log("[ThreeMatchBoard] 같은 블럭 클릭 취소");
                    return;
                }

                var firstCell = GetCell(firstPos);
                var firstBlock = firstCell?.Block;

                if (firstBlock != null && IsAdjacent(firstPos, secondPos) && firstBlock is ISwappableBlock swappable)
                {
                    UnityEngine.Debug.Log($"[ThreeMatchBoard] 인접 블럭 스왑 시도: {firstPos} <-> {secondPos}");
                    
                    // 물리적 자리 교환 (임시 스왑 로직)
                    SwapBlocks(firstPos, secondPos);
                    
                    // 블럭 내부의 스왑 로직(폭발, 매치 확인 등) 실행
                    bool success = swappable.OnSwapped(this, firstPos, secondPos);
                    
                    if (success)
                    {
                        // 임시: 3매치 룰에서 일반 블럭의 경우 보드단에서 매치 체크가 필요함
                        var targetBlock = GetCell(secondPos).Block;
                        if (targetBlock != null && firstBlock.GetBlockId() == targetBlock.GetBlockId())
                        {
                            UnityEngine.Debug.Log($"[ThreeMatchBoard] 매칭 성공! 동일 블럭 ID: {firstBlock.GetBlockId()}");
                            GetCell(firstPos).Block = null;
                            GetCell(secondPos).Block = null;
                            AddView(new BoardViewAction { type = ViewType.Destroy, frame = 0 });
                        }
                        else
                        {
                            // 매치 실패 시 다시 되돌림
                            UnityEngine.Debug.Log("[ThreeMatchBoard] 매칭 실패. 스왑 취소 및 원상복구.");
                            SwapBlocks(firstPos, secondPos);
                        }
                    }
                    else
                    {
                        // 블럭 자체에서 스왑을 거부한 경우 (원상복구)
                        SwapBlocks(firstPos, secondPos);
                    }
                }
                else
                {
                    // 인접하지 않은 곳을 클릭했으면 해당 블럭을 새로 선택
                    _selectedPos = input;
                    UnityEngine.Debug.Log($"[ThreeMatchBoard] 인접하지 않음. 새로운 블럭 선택됨: {input.X}, {input.Y}");
                }
            }
            finally
            {
                _isProcessing = false;
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

        /// <summary>
        /// 화면 연출을 위해 발생한 상태 변화(BoardViewAction)를 추가합니다.
        /// </summary>
        /// <param name="view">추가할 BoardViewAction 변화 데이터</param>
        public void AddView(BoardViewAction view)
        {
            _views.Add(view);
        }

        /// <summary>
        /// 발생한 View 변화 데이터들을 프레임 기준 내림차순으로 정렬하여 반환합니다.
        /// </summary>
        /// <returns>정렬된 View 데이터 리스트</returns>
        public List<BoardViewAction> GetViewsDescending()
        {
            return _views.OrderByDescending(v => v.frame).ToList();
        }
    }
}