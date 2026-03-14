using UnityEngine;
using Puzzle.Core;

public class PuzzleGameController : MonoBehaviour
{
    [Header("View References")]
    public PuzzleBoardView boardView; // 화면의 그래픽 View 스크립트 참조

    private PuzzleBoard _board; // 코어 로직 엔진 및 데이터

    void Start()
    {
        GameSpec gameSpec = StageInjection.Instance.GetGameSpec();

        if (gameSpec == null || gameSpec.stageData == null)
        {
            Debug.LogWarning("GameSpec이 주입되지 않았거나 비어 있습니다! 테스트용 더미 데이터로 초기화합니다.");
            gameSpec = gameSpec ?? new GameSpec();
        }

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

}
