using UnityEngine;
using Puzzle.Core; // 코어 네임스페이스 참조

public class PuzzleGameController : MonoBehaviour
{
    [Header("View References")]
    public PuzzleBoardView boardView; // 화면을 그려줄 View 스크립트 연결

    private PuzzleBoard _board; // 코어 게임 로직 및 데이터

    void Start()
    {
        // 1. 코어 데이터(Model) 생성 및 초기화
        _board = new PuzzleBoard();
        _board.Initialize(new GameSpec()); // 현재는 빈 사양서로 초기화

        // 2. 테스트용 가짜 데이터(Dummy Data) 생성 (5x5 사이즈)
        CreateDummyData(5, 5);

        // 3. 뷰(View)에 데이터 전달하여 화면에 그리기
        if (boardView != null)
        {
            boardView.DrawBoard(_board);
        }
        else
        {
            Debug.LogError("BoardView가 연결되지 않았습니다!");
        }
    }

    // 테스트를 위해 임의의 보드 데이터를 생성하는 메서드
    private void CreateDummyData(int width, int height)
    {
        // 테스트용으로 사용할 블록 타입 배열
        BlockType[] availableTypes = { BlockType.Normal, BlockType.Item, BlockType.Target };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 위치 지정
                GridPos pos = new GridPos(x, y);

                // 셀 생성
                PuzzleCell cell = new PuzzleCell(pos);

                //cell.Block = new PuzzleBlock(randomType);

                // 보드의 딕셔너리에 셀 추가
                _board.Cells.Add(pos, cell);
            }
        }
    }
}