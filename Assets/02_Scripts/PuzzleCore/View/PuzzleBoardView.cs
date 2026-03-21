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
    private IPuzzleBoard _board;
    
    /// <summary> 화면에 생성된 셀 뷰들을 좌표별로 관리하는 딕셔너리 </summary>
    private Dictionary<GridPos, PuzzleCellView> _cellViews = new Dictionary<GridPos, PuzzleCellView>();

    /// <summary> 화면에 생성된 블럭 뷰들을 좌표별로 관리하는 딕셔너리 </summary>
    private Dictionary<GridPos, PuzzleBlockView> _blockViews = new Dictionary<GridPos, PuzzleBlockView>();

    /// <summary>
    /// 전달받은 보드 데이터를 바탕으로 화면에 퍼즐 판을 그립니다.
    /// </summary>
    /// <param name="boardData">화면에 표시할 보드 모델 객체</param>
    public void DrawBoard(IPuzzleBoard boardData)
    {
        _board = boardData;
       
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
    /// 매 프레임마다 보드 모델로부터 발생한 액션들을 가져와 화면에 반영합니다.
    /// </summary>
    private void Update()
    {
        if (_board == null) return;

        // 보드로부터 발생한 연출 요청(Action)들을 가져옴
        List<BoardViewAction> actions = _board.FetchActions();
        
        if (actions != null && actions.Count > 0)
        {
            foreach (var action in actions)
            {
                ProcessViewAction(action);
            }
        }
    }

    /// <summary>
    /// 개별 보드 액션 타입에 따라 시각적 연출을 수행합니다.
    /// </summary>
    /// <param name="action">수행할 연출 정보</param>
    private void ProcessViewAction(BoardViewAction action)
    {
        switch (action.type)
        {
            case ViewType.Move:
                HandleMoveAction(action.position, action.targetPosition);
                break;
            case ViewType.Destroy:
                HandleDestroyAction(action.position);
                break;
            case ViewType.Create:
                HandleCreateAction(action.position);
                break;
        }
    }

    private void HandleMoveAction(GridPos from, GridPos to)
    {
        if (_blockViews.TryGetValue(from, out PuzzleBlockView view))
        {
            // 딕셔너리 위치 갱신
            _blockViews.Remove(from);
            _blockViews[to] = view;

            // 월드 좌표 계산
            Vector3 targetWorldPos = new Vector3(to.X * cellSize, to.Y * cellSize, 0);

            // TODO: DOTween 등을 사용하여 부드러운 이동 애니메이션 적용 가능
            // 현재는 즉시 이동
            view.transform.localPosition = targetWorldPos;
            view.Initialize(view.GetBlockData(), to, this); // 좌표 정보 동기화
        }
    }

    private void HandleDestroyAction(GridPos pos)
    {
        if (_blockViews.TryGetValue(pos, out PuzzleBlockView view))
        {
            _blockViews.Remove(pos);
            
            // TODO: 파괴 애니메이션(이펙트 등) 후 Destroy 호출
            if (view != null && view.gameObject != null)
            {
                Destroy(view.gameObject);
            }
        }
    }

    private void HandleCreateAction(GridPos pos)
    {
        // 보드 모델에서 최신 블럭 데이터를 가져옴
        PuzzleCell cell = _board.GetCell(pos);
        if (cell != null && cell.Block != null)
        {
            // 이미 해당 위치에 뷰가 있다면 제거 (안전 장치)
            if (_blockViews.ContainsKey(pos))
            {
                HandleDestroyAction(pos);
            }

            CreateBlockView(pos, cell.Block);
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

    /// <summary>
    /// 보드 모델의 현재 상태에 맞춰 블럭 뷰들을 강제로 동기화(재생성)합니다.
    /// 초기화 시나 상태 복구 시 사용됩니다.
    /// </summary>
    public void RefreshBlocks()
    {
        if (_board == null) return;

        // 기존 블럭 뷰 모두 제거
        foreach (var view in _blockViews.Values)
        {
            if (view != null && view.gameObject != null)
            {
                Destroy(view.gameObject);
            }
        }
        _blockViews.Clear();

        // 현재 모델 데이터(Cells)에 맞춰 블럭 다시 생성
        foreach (var kvp in _board.Cells)
        {
            GridPos gridPos = kvp.Key;
            PuzzleCell cell = kvp.Value;

            if (cell.Block != null)
            {
                CreateBlockView(gridPos, cell.Block);
            }
        }
    }
}