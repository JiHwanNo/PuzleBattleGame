using System.Collections.Generic;
using System.Threading.Tasks;

namespace Puzzle.Core
{
    public class PuzzleBoard
    {
        public Dictionary<GridPos, PuzzleCell> Cells { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Queue<GameInput> inputQueue;

        private ulong frameCount;

        private bool isProcessing;
        // 사양서를 바탕으로 보드를 셋업합니다.
        public void Initialize(GameSpec spec)
        {
            inputQueue = new Queue<GameInput>();
            Cells = new Dictionary<GridPos, PuzzleCell>();
            frameCount = 0;
        }

        // 컨트롤러에서 호출하는 입력 처리 및 게임 로직 업데이트 메서드입니다. 
        public void Input(GameInput input)
        {
            inputQueue.Enqueue(input);
        }

        // 매 프레임마다 게임 상태를 업데이트하는 메서드입니다. (예: 블럭 낙하, 매치 검사 등)
        public void Update()
        {
            frameCount++;

            // 큐에 들어있는 입력을 먼저 들어온 순서대로 처리
            if(isProcessing == false)
            {
                isProcessing = true;
                if (inputQueue.Count > 0)
                {
                    var inputsToProcess = new Queue<GameInput>(inputQueue);
                    inputQueue.Clear();
                    Task.Run(() => { ProcessInput(inputsToProcess); });
                }
            }

            // 유저가 선택해. 유저가 끝내면 보드를 판정한다.
        }

        void ProcessInput(Queue<GameInput> inputs)
        {
            try
            {
                while (inputs.TryDequeue(out GameInput currentInput))
                {

                    var fromCell = GetCell(currentInput.From);
                    var toCell = GetCell(currentInput.To);

                    if(fromCell != null)
                        fromCell.Update(currentInput);

                    if(toCell != null)
                        toCell.Update(currentInput);
                }
            }
            finally
            {
                isProcessing = false;
            }
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