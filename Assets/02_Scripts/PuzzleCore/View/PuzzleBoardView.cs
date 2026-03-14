using UnityEngine;
using System.Collections.Generic;
using Puzzle.Core;

/// <summary>
/// 퍼즐 보드의 데이터(Model)를 받아 화면에 시각적으로 렌더링하는 View 클래스입니다.
/// 보드의 크기에 맞춰 타일과 블럭들을 배치하고 관리합니다.
/// </summary>
public class PuzzleBoardView : MonoBehaviour
{
    [Header("Prefabs")]
    /// <summary> 생성할 셀(배경 타일)의 프리팹 에셋 </summary>
    public GameObject cellPrefab;

    /// <summary> 생성할 블럭의 프리팹 에셋 </summary>
    public GameObject blockPrefab;

    [Header("Settings")]
    /// <summary> 타일 한 칸의 시각적 크기 </summary>
    public float cellSize = 1.0f;

    /// <summary> 현재 연결된 보드 모델 데이터 </summary>
    private PuzzleBoard _board;
    
    /// <summary> 화면에 생성된 셀 뷰들을 좌표별로 관리하는 딕셔너리 </summary>
    private Dictionary<GridPos, PuzzleCellView> _cellViews = new Dictionary<GridPos, PuzzleCellView>();

    /// <summary> 화면에 생성된 블럭 뷰들을 좌표별로 관리하는 딕셔너리 </summary>
    private Dictionary<GridPos, PuzzleBlockView> _blockViews = new Dictionary<GridPos, PuzzleBlockView>();

    /// <summary>
    /// 전달받은 보드 데이터를 바탕으로 화면에 퍼즐 판을 그립니다.
    /// </summary>
    /// <param name="boardData">화면에 표시할 보드 모델 객체</param>
    public void DrawBoard(PuzzleBoard boardData)
    {
        _board = boardData;

        // 기존에 생성된 블럭 뷰들을 모두 제거하여 초기화
        foreach (var view in _blockViews.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }
        _blockViews.Clear();

        // 기존에 생성된 셀 뷰들을 모두 제거하여 초기화
        foreach (var cellView in _cellViews.Values)
        {
            if (cellView != null)
                Destroy(cellView.gameObject);
        }
        _cellViews.Clear();

        if (_board.Cells == null) return;

        // 보드의 모든 셀을 순회하며 셀 및 블럭이 있는 경우 뷰를 생성
        foreach (var kvp in _board.Cells)
        {
            GridPos gridPos = kvp.Key;
            PuzzleCell cell = kvp.Value;

            CreateCellView(gridPos, cell);

            if (cell.Block != null)
            {
                CreateBlockView(gridPos, cell.Block);
            }
        }
    }

    /// <summary>
    /// 특정 좌표에 새로운 셀 뷰를 생성하고 배치합니다.
    /// </summary>
    /// <param name="gridPos">셀이 배치될 그리드 좌표</param>
    /// <param name="cellData">셀의 모델 데이터</param>
    private void CreateCellView(GridPos gridPos, PuzzleCell cellData)
    {
        if (cellPrefab == null) return;

        // 그리드 좌표를 월드 좌표(Vector3)로 변환
        Vector3 worldPos = new Vector3(gridPos.X * cellSize, gridPos.Y * cellSize, 0);

        // 셀 프리팹 인스턴스화
        GameObject cellObj = Instantiate(cellPrefab, worldPos, Quaternion.identity, this.transform);
        cellObj.name = $"Cell_{gridPos.X}_{gridPos.Y}";

        // 뷰 컴포넌트 추가 및 초기화
        PuzzleCellView cellView = cellObj.GetComponent<PuzzleCellView>();
        if (cellView == null)
        {
            cellView = cellObj.AddComponent<PuzzleCellView>();
        }

        cellView.Initialize(cellData, gridPos, this);

        // 딕셔너리에 추가
        _cellViews.Add(gridPos, cellView);
    }

    /// <summary>
    /// 특정 좌표에 새로운 블럭 뷰를 생성하고 배치합니다.
    /// </summary>
    /// <param name="gridPos">블럭이 배치될 그리드 좌표</param>
    /// <param name="blockData">블럭의 모델 데이터</param>
    private void CreateBlockView(GridPos gridPos, PuzzleBlock blockData)
    {
        if (blockPrefab == null) return;

        // 그리드 좌표를 월드 좌표(Vector3)로 변환
        Vector3 worldPos = new Vector3(gridPos.X * cellSize, gridPos.Y * cellSize, 0);

        // 블럭 프리팹 인스턴스화
        GameObject blockObj = Instantiate(blockPrefab, worldPos, Quaternion.identity, this.transform);
        blockObj.name = $"Block_{gridPos.X}_{gridPos.Y}";

        // 뷰 컴포넌트 초기화 및 리스트 추가
        PuzzleBlockView blockView = blockObj.GetComponent<PuzzleBlockView>();
        if (blockView != null)
        {
            blockView.Initialize(blockData, gridPos, this);
            _blockViews.Add(gridPos, blockView);
        }
    }

    /// <summary>
    /// 매 프레임마다 입력을 감지하여 보드 내의 어떤 블럭이 클릭되었는지 
    /// 중앙(Board)에서 한 번만 연산(Raycast)하여 판별합니다.
    /// 클릭 지점에 여러 콜라이더가 있을 수 있으므로 OverlapPointAll을 사용하여 모든 충돌체를 확인합니다.
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 마우스 클릭 위치에 있는 모든 2D 콜라이더를 감지 (Physics2D)
            Collider2D[] hitColliders = Physics2D.OverlapPointAll(mousePosition);

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
    }

    /// <summary>
    /// 특정 블럭 뷰에서 입력(클릭/터치)이 발생했을 때 호출됩니다.
    /// </summary>
    /// <param name="pos">입력이 발생한 그리드 좌표</param>
    public void OnBlockInput(GridPos pos)
    {
        if (_board != null)
        {
            Debug.Log($"[PuzzleBoardView] Block Input Detected: ({pos.X}, {pos.Y})");
            _board.Input(pos);
        }
    }
}