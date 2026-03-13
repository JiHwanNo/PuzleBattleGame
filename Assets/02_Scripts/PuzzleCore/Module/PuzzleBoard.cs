using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Core
{
    /// <summary>
    /// 퍼즐 게임의 전체 보드 상태와 핵심 로직을 관리하는 Model 클래스입니다.
    /// 보드는 여러 개의 PuzzleCell로 구성되며, 입력 처리 및 상태 업데이트를 담당합니다.
    /// </summary>
    public class PuzzleBoard
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
        private List<View> _views;

        /// <summary> 게임 시작 후 누적된 프레임 수 </summary>
        private ulong _frameCount;

        /// <summary> 현재 보드가 로직 처리 중인지 여부 </summary>
        private bool _isProcessing;

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
            _views = new List<View>();
            _frameCount = 0;
            _isProcessing = true;
            gameSpec = spec;
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
        public void InputEnd()
        {
            while (_inputQueue.Count > 0)
            {
                GridPos input = _inputQueue.Dequeue();
                ProcessInput(input);
            }
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
        /// 특정 좌표에 대한 입력을 실제로 처리하는 내부 로직입니다.
        /// </summary>
        /// <param name="input">처리할 타일의 그리드 좌표</param>
        private void ProcessInput(GridPos input)
        {
            try
            {
                var inputCell = GetCell(input);
                if (inputCell != null)
                {
                    inputCell.Update(input);
                }
            }
            finally
            {
                _isProcessing = false;
            }
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
        /// 화면 연출을 위해 발생한 상태 변화(View)를 추가합니다.
        /// </summary>
        /// <param name="view">추가할 View 변화 데이터</param>
        public void AddView(View view)
        {
            _views.Add(view);
        }

        /// <summary>
        /// 발생한 View 변화 데이터들을 프레임 기준 내림차순으로 정렬하여 반환합니다.
        /// </summary>
        /// <returns>정렬된 View 데이터 리스트</returns>
        public List<View> GetViewsDescending()
        {
            return _views.OrderByDescending(v => v.frame).ToList();
        }
    }

    /// <summary>
    /// 보드 상태 변화(매치, 파괴, 이동 등)를 화면에 연출하기 위해 기록하는 데이터 클래스입니다.
    /// </summary>
    public class View
    {
        /// <summary> 해당 연출이 발생한 보드 로직 프레임 </summary>
        public uint frame;
        /// <summary> 연출의 종류 (파괴, 생성, 이동 등) </summary>
        public ViewType type;
    }
}
