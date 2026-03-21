using UnityEngine;
using System.Collections.Generic;
using Puzzle.Core;

/// <summary>
/// 퍼즐 보드의 데이터(Model)를 받아 화면에 시각적으로 렌더링하는 View 클래스입니다.
/// 보드의 크기에 맞춰 타일과 블럭들을 배치하고 관리합니다.
/// </summary>
public class PuzzleBoardView : MonoBehaviour
{
    [Header("Asset Addresses")]
    /// <summary> 생성할 셀(배경 타일)의 Addressables 주소 </summary>
    public string cellAddress = "CellPrefab";

    /// <summary> 생성할 블럭의 Addressables 주소 </summary>
    public string blockAddress = "BlockPrefab";

    [Header("Hierarchy Roots")]
    /// <summary> 셀들이 생성될 부모 트랜스폼 </summary>
    public Transform cellRoot;

    /// <summary> 블럭들이 생성될 부모 트랜스폼 </summary>
    public Transform blockRoot;

    [Header("Settings")]
    /// <summary> 타일 한 칸의 시각적 크기 </summary>
    public float cellSize = 1.0f;

    /// <summary> 씬 뷰에서 그리드 좌표와 블럭 정보를 표시할지 여부 </summary>
    public bool showDebugGrid = true;

    /// <summary> 현재 연결된 보드 모델 데이터 </summary>
    private IPuzzleBoard _board;

    /// <summary> 로드된 셀 프리팹 캐시 </summary>
    private GameObject _cellPrefabObj;

    /// <summary> 로드된 블럭 프리팹 캐시 </summary>
    private GameObject _blockPrefabObj;
    
    /// <summary> 화면에 생성된 셀 뷰들을 좌표별로 관리하는 딕셔너리 </summary>
    private Dictionary<GridPos, PuzzleCellView> _cellViews = new Dictionary<GridPos, PuzzleCellView>();

    /// <summary> 화면에 생성된 블럭 뷰들을 좌표별로 관리하는 딕셔너리 </summary>
    private Dictionary<GridPos, PuzzleBlockView> _blockViews = new Dictionary<GridPos, PuzzleBlockView>();

#if UNITY_EDITOR
    /// <summary>
    /// 유니티 에디터의 씬 뷰에서 그리드 좌표와 블럭 정보를 텍스트로 표시합니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGrid || _board == null || _board.Cells == null)
        {
            return;
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 12;
        style.alignment = TextAnchor.MiddleCenter;

        foreach (var kvp in _board.Cells)
        {
            GridPos pos = kvp.Key;
            PuzzleCell cell = kvp.Value;
            
            // 셀의 월드 좌표 계산 (cellRoot 기준 혹은 자체 transform 기준)
            Transform root = cellRoot != null ? cellRoot : transform;
            Vector3 worldPos = root.TransformPoint(new Vector3(pos.X * cellSize, pos.Y * cellSize, 0));
            
            string info = $"({pos.X},{pos.Y})";
            if (cell.Block != null)
            {
                info += $"\nID: {cell.Block.GetBlockId()}";
            }
            else if (cell.CellType == CellType.Generator)
            {
                info += "\n[GEN]";
            }
            else
            {
                info += "\n[EMPTY]";
            }

            UnityEditor.Handles.Label(worldPos, info, style);
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            Gizmos.DrawWireCube(worldPos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
        }
    }
#endif

    /// <summary>
    /// 전달받은 보드 데이터를 바탕으로 화면에 퍼즐 판을 그립니다.
    /// </summary>
    /// <param name="boardData">화면에 표시할 보드 모델 객체</param>
    public void DrawBoard(IPuzzleBoard boardData)
    {
        _board = boardData;
       
        if (_board.Cells == null)
        {
            return;
        }

        // 에셋 매니저를 통해 프리팹 미리 확보 (캐싱됨)
        _cellPrefabObj = AssetManager.Instance.LoadAsset<GameObject>(cellAddress);
        _blockPrefabObj = AssetManager.Instance.LoadAsset<GameObject>(blockAddress);

        if (_cellPrefabObj == null || _blockPrefabObj == null)
        {
            Debug.LogError($"[PuzzleBoardView] Failed to load prefabs from Addressables. Cell: {cellAddress}, Block: {blockAddress}");
            return;
        }

        // 기존 뷰 초기화
        ClearBoard();

        // 루트 노드가 지정 안 되어 있으면 자신을 사용
        if (cellRoot == null)
        {
            cellRoot = this.transform;
        }
        if (blockRoot == null)
        {
            blockRoot = this.transform;
        }

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
    /// 현재 보드에 표시된 모든 뷰를 제거하고 풀로 반납합니다.
    /// </summary>
    private void ClearBoard()
    {
        foreach (var view in _cellViews.Values)
        {
            if (view != null)
            {
                PoolManager.Instance.Release(view.gameObject);
            }
        }

        foreach (var view in _blockViews.Values)
        {
            if (view != null)
            {
                PoolManager.Instance.Release(view.gameObject);
            }
        }

        _cellViews.Clear();
        _blockViews.Clear();
    }

    /// <summary>
    /// 특정 좌표에 새로운 셀 뷰를 생성하고 배치합니다.
    /// </summary>
    private void CreateCellView(GridPos gridPos, PuzzleCell cellData)
    {
        if (_cellPrefabObj == null)
        {
            return;
        }

        Vector3 localPos = new Vector3(gridPos.X * cellSize, gridPos.Y * cellSize, 0);

        // 지정된 cellRoot 하위에 생성
        GameObject cellObj = PoolManager.Instance.Get(_cellPrefabObj, cellRoot);
        cellObj.transform.localPosition = localPos;
        cellObj.name = $"Cell_{gridPos.X}_{gridPos.Y}";

        PuzzleCellView cellView = cellObj.GetComponent<PuzzleCellView>();
        if (cellView == null)
        {
            cellView = cellObj.AddComponent<PuzzleCellView>();
        }

        cellView.Initialize(cellData, gridPos, this);
        _cellViews.Add(gridPos, cellView);
    }

    /// <summary>
    /// 특정 좌표에 새로운 블럭 뷰를 생성하고 배치합니다.
    /// </summary>
    private void CreateBlockView(GridPos gridPos, PuzzleBlock blockData)
    {
        if (_blockPrefabObj == null)
        {
            return;
        }

        // 블럭은 배경보다 앞에 오도록 Z를 -0.1로 설정
        Vector3 localPos = new Vector3(gridPos.X * cellSize, gridPos.Y * cellSize, -0.1f);

        // 지정된 blockRoot 하위에 생성
        GameObject blockObj = PoolManager.Instance.Get(_blockPrefabObj, blockRoot);
        blockObj.transform.localPosition = localPos;
        blockObj.name = $"Block_{gridPos.X}_{gridPos.Y}";

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
        if (_board == null)
        {
            return;
        }

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

    /// <summary>
    /// 블럭의 이동 연출을 처리합니다.
    /// </summary>
    private void HandleMoveAction(GridPos from, GridPos to)
    {
        if (_blockViews.TryGetValue(from, out PuzzleBlockView view))
        {
            _blockViews.Remove(from);
            _blockViews[to] = view;

            Vector3 targetLocalPos = new Vector3(to.X * cellSize, to.Y * cellSize, -0.1f);
            view.transform.localPosition = targetLocalPos;
            view.Initialize(view.GetBlockData(), to, this); 
        }
    }

    /// <summary>
    /// 블럭의 파괴 연출을 처리합니다.
    /// </summary>
    private void HandleDestroyAction(GridPos pos)
    {
        if (_blockViews.TryGetValue(pos, out PuzzleBlockView view))
        {
            _blockViews.Remove(pos);
            if (view != null && view.gameObject != null)
            {
                PoolManager.Instance.Release(view.gameObject);
            }
        }
    }

    /// <summary>
    /// 새로운 블럭의 생성 연출을 처리합니다.
    /// </summary>
    private void HandleCreateAction(GridPos pos)
    {
        PuzzleCell cell = _board.GetCell(pos);
        if (cell != null && cell.Block != null)
        {
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
    public void OnBlockInput(GridPos pos)
    {
        if (_board != null)
        {
            _board.Input(pos);
        }
    }

    /// <summary>
    /// 현재 보드 상태에 맞춰 모든 블럭 뷰를 재생성합니다.
    /// </summary>
    public void RefreshBlocks()
    {
        if (_board == null)
        {
            return;
        }

        foreach (var view in _blockViews.Values)
        {
            if (view != null && view.gameObject != null)
            {
                PoolManager.Instance.Release(view.gameObject);
            }
        }
        _blockViews.Clear();

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
