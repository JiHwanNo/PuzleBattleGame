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

    /// <summary> 현재 드래그 중 마지막으로 머문 좌표 (되돌리기 등 경로 재진입 허용을 위해) </summary>
    private GridPos? _lastHoveredPos = null;

    /// <summary> 보드가 정상적으로 초기화되었는지 여부 </summary>
    private bool _isInitialized = false;

    /// <summary> 캐싱된 메인 카메라 참조 </summary>
    private Camera _mainCamera;

    /// <summary> Physics2D.OverlapPoint 결과를 재사용하기 위한 버퍼 </summary>
    private readonly List<Collider2D> _hitBuffer = new List<Collider2D>(16);

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
                _board = new LinkPuzzleBoard(); // 링크 전용 보드로 연결
                break;
            case PuzzleType.TapMatch:
                _board = new TapMatchPuzzleBoard();
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

        _mainCamera = Camera.main;
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
            // 뷰가 애니메이션 연출 중일 때는 어떠한 입력도 받지 않도록 차단합니다!
            if (boardView == null || !boardView.IsAnimating)
            {
                Vector2 screenPosition = GetPointerPosition();
                Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

                // 현재 위치 아래의 콜라이더 감지 (사전 할당 버퍼 사용으로 GC 방지)
                int hitCount = Physics2D.OverlapPoint(worldPosition, new ContactFilter2D().NoFilter(), _hitBuffer);

                // 모든 충돌체를 순회하며 PuzzleBlockCollider를 찾습니다.
                for (int i = 0; i < hitCount; i++)
                {
                    var hitCollider = _hitBuffer[i];
                    if (hitCollider.TryGetComponent<PuzzleBlockCollider>(out var blockCollider))
                    {
                        GridPos? pos = GetGridPosFromCollider(hitCollider);
                        // 이전에 머물렀던 블럭과 다른 블럭으로 마우스가 이동했을 때만 입력 처리
                        if (pos.HasValue && pos.Value != _lastHoveredPos)
                        {
                            _lastHoveredPos = pos.Value;
                            blockCollider.OnClickBlock();
                            break;
                        }
                    }
                }
            }
        }

        // 2. 마우스/터치를 뗐는가? -> 입력 종료 및 경로 초기화
        if (IsPointerReleased())
        {
            _board.InputEnd();
            _lastHoveredPos = null; // 포인터를 떼면 초기화
        }

        // 3. 보드 논리 업데이트
        _board.Update();
    }

    /// <summary>
    /// 고정 프레임 간격으로 논리 프레임을 업데이트합니다.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_isInitialized || _board == null)
        {
            return;
        }

        _board.FixedUpdate();
    }

    /// <summary>
    /// 충돌한 콜라이더로부터 그리드 좌표를 가져옵니다.
    /// </summary>
    /// <param name="col">충돌한 콜라이더</param>
    /// <returns>블럭 뷰가 가진 그리드 좌표 혹은 null</returns>
    private GridPos? GetGridPosFromCollider(Collider2D col)
    {
        var view = col.GetComponentInParent<PuzzleBlockView>();
        if (view != null)
        {
            return view.GridPos;
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
