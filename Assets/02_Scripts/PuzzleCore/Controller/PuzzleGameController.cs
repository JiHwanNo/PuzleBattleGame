using Puzzle.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGameController : MonoBehaviour
{
    [Header("View References")]
    public PuzzleBoardView boardView; // 화면의 그래픽 View 스크립트 참조

    private IPuzzleBoard _board; // 코어 로직 엔진 및 데이터

    /// <summary> 현재 드래그 세션에서 이미 거쳐간 좌표 목록 (중복 입력 방지용) </summary>
    private HashSet<GridPos> _inputPath = new HashSet<GridPos>();

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
                _board = new ThreeMatchPuzzleBoard(); // 임시
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

        isInitialized = true;
    }

    /// <summary>
    /// 매 프레임마다 입력을 감지합니다.
    /// </summary>
    private void Update()
    {
        if (!isInitialized || _board == null)
            return;

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
                    // 해당 블럭 뷰로부터 좌표 정보를 가져옴
                    PuzzleBlockView blockView = blockCollider.GetComponentInParent<PuzzleBlockView>();
                    if (blockView != null)
                    {
                        // 뷰 내부의 비공개 필드인 _gridPos를 가져올 수 없으므로, 
                        // 이미 만들어진 OnClicked() 대신 뷰를 통해 좌표를 확인하는 로직이 필요할 수 있으나
                        // 일단 기존 OnClicked() 흐름을 유지하되, 뷰의 데이터(GridPos)를 체크하는 구조로 우회합니다.
                        // (참고: PuzzleBlockView에 public으로 좌표를 노출하거나, 직접 BoardView에서 좌표를 계산할 수도 있습니다.)
                        
                        // 현재는 편의상 OnClicked 내부에서 보드에 Input을 넣는 과정을 
                        // 이 컨트롤러에서 중복 체크 후 실행하도록 합니다.
                        // (※ PuzzleBlockView.OnClicked() 내부 로그가 중복 방지 되는지 확인용)
                        
                        // TODO: 더 깔끔한 구조를 위해 View에서 좌표를 받아오는 메서드 추가 권장
                    }
                    
                    // 우선은 기존 UI 조작 체계를 유지하며 중복 실행만 방지 (Collider 단위 중복 체크)
                    // (※ 여기서 Collider 자체를 체크하는 방식으로 중복을 막습니다.)
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
    /// 충돌한 콜라이더로부터 그리드 좌표를 역산하거나 가져옵니다.
    /// </summary>
    private GridPos? GetGridPosFromCollider(Collider2D col)
    {
        // View 컴포넌트에 접근하여 좌표를 가져오는 것이 가장 정확함
        var view = col.GetComponentInParent<PuzzleBlockView>();
        if (view != null)
        {
            // 현재 PuzzleBlockView.cs에 좌표를 반환하는 public 필드가 없으므로 
            // 리플렉션을 쓰거나 필드를 추가해야 함. (일단은 좌표 계산 로직으로 대체)
            // 혹은 View에 public GridPos 속성을 추가하는 것이 정석입니다.
            
            // 임시: 월드 좌표 기반 역산 (cellSize 사용)
            if (boardView != null)
            {
                float cs = boardView.cellSize;
                Vector3 lp = view.transform.localPosition;
                return new GridPos(Mathf.RoundToInt(lp.x / cs), Mathf.RoundToInt(lp.y / cs));
            }
        }
        return null;
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
