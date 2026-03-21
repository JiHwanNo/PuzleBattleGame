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
    /// 매 프레임마다 입력을 감지합니다.
    /// 누르고 있는 동안은 인풋을 보드에 전달하고, 뗐을 때만 보드에게 '입력 종료'를 알립니다.
    /// </summary>
    private void Update()
    {
        if (!isInitialized || _board == null)
            return;

        // 1. 마우스/터치를 누르고 있는 중인가?
        bool isPointerHeld = IsPointerHeld();
        bool isPointerReleased = IsPointerReleased();

        if (isPointerHeld)
        {
            Vector2 screenPosition = GetPointerPosition();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // 현재 위치 아래의 콜라이더 감지
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
            if (hitCollider != null)
            {
                // 블럭 좌표를 찾아 보드에 전달 (View를 통해 좌표 획득)
                if (hitCollider.TryGetComponent<PuzzleBlockCollider>(out var blockCollider))
                {
                    blockCollider.OnClickBlock(); 
                }
            }
        }

        // 2. 마우스/터치를 뗐는가? -> 이때가 로직 처리 시점
        if (isPointerReleased)
        {
            _board.InputEnd();
        }

        // 3. 보드 논리 업데이트 (애니메이션, 낙하 등)
        _board.Update();
    }

    private bool IsPointerHeld()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
        return false;
    }

    private bool IsPointerReleased()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) return true;
        return false;
    }

    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        
        return Mouse.current.position.ReadValue();
    }
}
