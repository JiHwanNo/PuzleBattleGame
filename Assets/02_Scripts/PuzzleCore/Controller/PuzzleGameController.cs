using Puzzle.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 유저의 입력을 감지하고 퍼즐 보드 모델 및 뷰와 통신하여 전체 게임 루프를 제어하는 컨트롤러 클래스입니다.
/// </summary>
public class PuzzleGameController : MonoBehaviour
{
    [Header("View References")]
    /// <summary> 화면에 보드를 그리는 뷰 객체 </summary>
    public PuzzleBoardView boardView;

    /// <summary> 퍼즐 보드의 핵심 로직을 처리하는 모델 인터페이스 </summary>
    private IPuzzleBoard _board;

    /// <summary> 현재 드래그 세션에서 이미 거쳐간 좌표 목록 (중복 입력 방지용) </summary>
    private HashSet<GridPos> _inputPath = new HashSet<GridPos>();

    /// <summary> 보드가 정상적으로 초기화되었는지 여부 </summary>
    private bool _isInitialized = false;

    /// <summary>
    /// 게임 시작 시 스테이지 데이터를 로드하고 보드를 초기화합니다.
    /// </summary>
    private void Start()
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
                _board = new ThreeMatchPuzzleBoard(); // TODO: Link 전용 보드 구현 시 교체
                break;
            default:
                _board = new ThreeMatchPuzzleBoard();
                break;
        }

        _board.Initialize(gameSpec);

        if (boardView != null)
        {
            boardView.DrawBoard(_board);
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 매 프레임 입력 신호를 감지하고 보드 상태를 업데이트합니다.
    /// </summary>
    private void Update()
    {
        if (!_isInitialized || _board == null)
        {
            return;
        }

        // 1. 마우스/터치를 누르고 있는 중인가?
        if (IsPointerHeld())
        {
            Vector2 screenPosition = GetPointerPosition();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // 현재 위치 아래의 콜라이더 감지
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
            if (hitCollider != null)
            {
                if (hitCollider.TryGetComponent<PuzzleBlockCollider>(out var blockCollider))
                {
                    // 원본 로직: Collider 단위 중복 체크 후 블럭 클릭 이벤트 전달
                    GridPos? pos = GetGridPosFromCollider(hitCollider);
                    if (pos.HasValue && !_inputPath.Contains(pos.Value))
                    {
                        _inputPath.Add(pos.Value);
                        blockCollider.OnClickBlock();
                    }
                }
            }
        }

        // 2. 마우스/터치를 뗐는가? -> 입력 종료 및 경로 초기화
        if (IsPointerReleased())
        {
            _board.InputEnd();
            _inputPath.Clear(); // 거쳐간 경로 초기화
        }

        // 3. 보드 논리 업데이트
        _board.Update();
    }

    /// <summary>
    /// 충돌한 콜라이더로부터 그리드 좌표를 역산합니다.
    /// </summary>
    /// <param name="col">충돌한 콜라이더</param>
    /// <returns>계산된 그리드 좌표 혹은 null</returns>
    private GridPos? GetGridPosFromCollider(Collider2D col)
    {
        var view = col.GetComponentInParent<PuzzleBlockView>();
        if (view != null)
        {
            if (boardView != null)
            {
                float cs = boardView.cellSize;
                Vector3 lp = view.transform.localPosition;
                return new GridPos(Mathf.RoundToInt(lp.x / cs), Mathf.RoundToInt(lp.y / cs));
            }
        }
        return null;
    }

    /// <summary>
    /// 현재 화면 클릭 또는 터치가 유지되고 있는지 확인합니다.
    /// </summary>
    private bool IsPointerHeld()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 현재 화면 클릭 또는 터치가 종료되었는지 확인합니다.
    /// </summary>
    private bool IsPointerReleased()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 현재 포인터의 스크린 좌표를 가져옵니다.
    /// </summary>
    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        return Mouse.current.position.ReadValue();
    }
}
