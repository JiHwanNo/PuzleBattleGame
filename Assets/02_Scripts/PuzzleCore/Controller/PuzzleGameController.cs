using UnityEngine;
using Puzzle.Core;

public class PuzzleGameController : MonoBehaviour
{
    [Header("View References")]
    public PuzzleBoardView boardView; // 화면의 그래픽 View 스크립트 참조

    private PuzzleBoard _board; // 코어 로직 엔진 및 데이터

    void Start()
    {
        // 1. StageInjection에서 설정된 GameSpec 가져오기
        GameSpec gameSpec = StageInjection.Instance.GetGameSpec();

        if (gameSpec == null || gameSpec.stageData == null)
        {
            Debug.LogWarning("GameSpec이 주입되지 않았거나 비어 있습니다! 테스트용 더미 데이터로 초기화합니다.");
            gameSpec = gameSpec ?? new GameSpec();
            // 필요 시 여기서 테스트용 기본 설정을 추가할 수 있습니다.
        }

        // 2. 코어 모델(Model) 생성 및 초기화
        _board = new PuzzleBoard();
        _board.Initialize(gameSpec);

        // 3. 뷰(View)에 데이터 전달하여 화면에 그리기
        if (boardView != null)
        {
            boardView.DrawBoard(_board);
        }
        else
        {
            Debug.LogError("BoardView가 설정되지 않았습니다!");
        }
    }

    // 테스트를 위한 더미 데이터 생성 메서드 (필요 시 호출 가능)
    private void CreateDummyData(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridPos pos = new GridPos(x, y);
                PuzzleCell cell = new PuzzleCell(pos);
                _board.Cells.Add(pos, cell);
            }
        }
    }
}
