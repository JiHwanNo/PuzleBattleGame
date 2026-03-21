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

    /// <summary> 현재 드래그 중인 블럭들의 좌표 경로 </summary>
    private HashSet<GridPos> _inputPath = new HashSet<GridPos>();

    /// <summary> 입력 종료(포인터 뗌)가 발생했음을 알리는 플래그 </summary>
    private bool _isReleasedPending = false;

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
            gameSpec = gameSpec ?? new GameSpec();
        }

        switch (gameSpec.rule.puzzleType)
        {
            case PuzzleType.ThreeMatch:
                _board = new ThreeMatchPuzzleBoard();
                break;
            default:
                _board = new ThreeMatchPuzzleBoard();
                break;
        }

        // 보드 로그를 유니티 콘솔에 연결 (Model-View 분리 준수)
        _board.OnLog = (msg) =>
        {
            Debug.Log($"<color=cyan>[Model]</color> {msg}");
        };

        _board.Initialize(gameSpec);

        if (boardView != null)
        {
            boardView.DrawBoard(_board);
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 매 프레임 입력 신호를 감지합니다. (Rendering Frame)
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

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
            if (hitCollider != null && hitCollider.TryGetComponent<PuzzleBlockCollider>(out var blockCollider))
            {
                GridPos? pos = GetGridPosFromCollider(hitCollider);
                if (pos.HasValue && !_inputPath.Contains(pos.Value))
                {
                    _inputPath.Add(pos.Value);
                    blockCollider.OnClickBlock(); // 보드 큐에 좌표 입력
                }
            }
        }

        // 2. 마우스/터치를 뗐는가? -> FixedUpdate에서 처리하도록 예약
        if (IsPointerReleased())
        {
            _isReleasedPending = true;
        }
    }

    /// <summary>
    /// 고정 프레임 주기로 보드 로직을 업데이트합니다. (Logic Frame)
    /// 리플레이의 결정론적 보장을 위해 사용됩니다.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_isInitialized || _board == null)
        {
            return;
        }

        // 예약된 입력 종료 처리
        if (_isReleasedPending)
        {
            _board.InputEnd();
            _inputPath.Clear();
            _isReleasedPending = false;
        }

        // 보드 상태 및 프레임 업데이트
        _board.Update();
    }

    /// <summary>
    /// 충돌체(Collider)로부터 해당 셀의 그리드 좌표를 계산합니다.
    /// </summary>
    private GridPos? GetGridPosFromCollider(Collider2D col)
    {
        var view = col.GetComponentInParent<PuzzleBlockView>();
        if (view != null && boardView != null)
        {
            float cs = boardView.cellSize;
            Vector3 lp = view.transform.localPosition;
            return new GridPos(Mathf.RoundToInt(lp.x / cs), Mathf.RoundToInt(lp.y / cs));
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
