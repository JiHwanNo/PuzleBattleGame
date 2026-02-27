using System.Collections.Generic;

namespace Puzzle.Core
{
    public class PuzzleBoard
    {
        public Dictionary<GridPos, PuzzleCell> Cells { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Queue<GridPos> inputQueue;
        // 유저가 조작하는 중. // 유저가 시작 // 유저가 끝

        private ulong frameCount;

        private bool isProcessing;
        
        // 사양서를 바탕으로 보드를 셋업합니다.
        public void Initialize(GameSpec spec)
        {
            inputQueue = new Queue<GridPos>();
            Cells = new Dictionary<GridPos, PuzzleCell>();
            frameCount = 0;
            isProcessing = true;
        }

        // 컨트롤러에서 호출하는 입력 처리 및 게임 로직 업데이트 메서드입니다. 
        public void Input(GridPos input)
        {
            inputQueue.Enqueue(input);
            
        }

        public void InputEnd()
        {
        }

        // 매 프레임마다 게임 상태를 업데이트하는 메서드입니다. (예: 블럭 낙하, 매치 검사 등)
        public void Update()
        {
            frameCount++;
        }

        public void Pause(bool pause)
        {
            isProcessing = pause;
        }

        void ProcessInput(GridPos input)
        {
            try
            {
                var inputCell = GetCell(input);

                if (inputCell != null)
                    inputCell.Update(input);
            }
            finally
            {
                isProcessing = false;
            }
        }

        bool EvaluateBoard()
        {
            // 보드 전체를 스캔하여 3매치 조건을 검사하는 로직을 구현합니다.
            // 매치가 발견되면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
            return false; // 임시 반환값
        }

        // 특정 좌표의 셀을 가져오는 안전한 메서드
        public PuzzleCell GetCell(GridPos pos)
        {
            if (Cells.TryGetValue(pos, out PuzzleCell cell))
                return cell;

            return null;
        }
    }
}