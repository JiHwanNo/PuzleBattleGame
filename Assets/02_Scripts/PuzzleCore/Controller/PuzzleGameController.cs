using Puzzle.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGameController : MonoBehaviour
{
    [Header("View References")]
    public PuzzleBoardView boardView; // 화면의 그래픽 View 스크립트 참조

    private IPuzzleBoard _board; // 코어 로직 엔진 및 데이터

    bool isInitialized = false;
    void Start()
    {
        GameSpec gameSpec = StageInjection.Instance.GetGameSpec();

        if (gameSpec == null || gameSpec.stageData == null)
        {
            Debug.LogWarning("GameSpec이 주입되지 않았거나 비어 있습니다! 테스트용 더미 데이터로 초기화합니다.");
            gameSpec = gameSpec ?? new GameSpec();
        }

        // 게임 타입에 따라 적절한 보드 구현체 생성
        switch (gameSpec.rule.puzzleType)
        {
            case PuzzleType.ThreeMatch:
                _board = new ThreeMatchPuzzleBoard();
                break;
            case PuzzleType.Link:
                // TODO: 링크 방식 보드 구현체 생성
                _board = new ThreeMatchPuzzleBoard(); // 임시로 3매치 보드 할당
                break;
            default:
                Debug.LogWarning($"알 수 없는 퍼즐 타입({gameSpec.rule.puzzleType})입니다. 기본 ThreeMatch 보드로 초기화합니다.");
                _board = new ThreeMatchPuzzleBoard();
                break;
        }

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

        isInitialized = true;
    }




    /// <summary>
    /// 매 프레임마다 입력을 감지하여 보드 내의 어떤 블럭이 클릭되었는지 
    /// 중앙(Board)에서 한 번만 연산(Raycast)하여 판별합니다.
    /// 클릭 지점에 여러 콜라이더가 있을 수 있으므로 OverlapPointAll을 사용하여 모든 충돌체를 확인합니다.
    /// </summary>
    private void Update()
    {
        if (!isInitialized)
            return;

        if (TryGetPointerDownPosition(out Vector2 screenPosition))
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // 마우스 클릭 위치에 있는 모든 2D 콜라이더를 감지 (Physics2D)
            Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPosition);

            if (hitColliders != null && hitColliders.Length > 0)
            {
                foreach (Collider2D hitCollider in hitColliders)
                {
                    // 충돌한 객체에 PuzzleBlockCollider 컴포넌트가 있다면 클릭 이벤트 실행
                    if (hitCollider.TryGetComponent<PuzzleBlockCollider>(out var blockCollider))
                    {
                        blockCollider.OnClickBlock();
                    }

                    // 충돌한 객체에 PuzzleCellCollider 컴포넌트가 있다면 클릭 이벤트 실행
                    if (hitCollider.TryGetComponent<PuzzleCellCollider>(out var cellCollider))
                    {
                        cellCollider.OnClickCell();
                    }
                }
            }
        }

        if (_board != null)
        {
            // 쌓인 입력을 처리하고 보드 상태를 갱신
            bool wasProcessed = _board.InputEnd();
            _board.Update();

            if (wasProcessed && boardView != null)
            {
                // 입력 처리 후 뷰를 동기화하여 변경사항(스왑, 매치)을 화면에 반영
                boardView.RefreshBlocks();
            }
        }
    }

    /// <summary>
    /// 마우스 클릭 또는 터치 입력이 발생했는지 확인하고 해당 화면(Screen) 좌표를 반환합니다.
    /// </summary>
    /// <param name="position">입력이 발생한 화면 좌표</param>
    /// <returns>입력이 발생했다면 true</returns>
    private bool TryGetPointerDownPosition(out Vector2 position)
    {
        position = Vector2.zero;

        // 마우스 입력 확인
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }

        // 터치 입력 확인
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        return false;
    }

}
