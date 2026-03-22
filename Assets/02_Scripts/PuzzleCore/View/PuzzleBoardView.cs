using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary> 보드 외곽의 여백 크기 </summary>
    public float padding = 1.0f;

    /// <summary> 
    /// 보드의 수직 위치 오프셋 (0: 중앙, 0.5: 상단 정렬, -0.5: 하단 정렬) 
    /// </summary>
    [Range(-0.5f, 0.5f)]
    public float offsetY = 0f;

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
            
            Transform root = cellRoot != null ? cellRoot : transform;
            Vector3 worldPos = root.TransformPoint(GetLocalPos(pos));
            
            string info = $"({pos.X},{pos.Y})";

            if (_blockViews.TryGetValue(pos, out PuzzleBlockView blockView))
            {
                info += $"\nID: {blockView.GetBlockData()?.GetBlockId() ?? "NullData"}";
            }
            else if (cell.Block != null)
            {
                if (_board.State == BoardState.Waiting)
                {
                    info += $"\nID: {cell.Block.GetBlockId()}\n[MISSING VIEW]";
                }
                else
                {
                    info += $"\nID: {cell.Block.GetBlockId()}\n[PROCESSING]";
                }
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

    public void DrawBoard(IPuzzleBoard boardData)
    {
        _board = boardData;
       
        if (_board.Cells == null)
        {
            return;
        }

        _cellPrefabObj = AssetManager.Instance.LoadAsset<GameObject>(cellAddress);
        _blockPrefabObj = AssetManager.Instance.LoadAsset<GameObject>(blockAddress);

        if (_cellPrefabObj == null || _blockPrefabObj == null)
        {
            Debug.LogError($"[PuzzleBoardView] Failed to load prefabs. Cell: {cellAddress}, Block: {blockAddress}");
            return;
        }

        ClearBoard();

        if (cellRoot == null)
        {
            cellRoot = this.transform;
        }
        if (blockRoot == null)
        {
            blockRoot = this.transform;
        }

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

        // --- 보드 생성 완료 후 중앙 정렬 및 카메라 줌 조절 ---
        AlignBoardToCenter();
    }

    /// <summary>
    /// 그리드 좌표를 보드 중심 기준의 로컬 월드 좌표로 변환합니다.
    /// 보드 자체는 (0,0)에 있고, 내부 셀들이 오프셋 배치됩니다.
    /// </summary>
    public Vector3 GetLocalPos(GridPos pos)
    {
        if (_board == null)
        {
            return Vector3.zero;
        }

        // 보드 중심을 (0,0)으로 만들기 위한 오프셋 계산
        float offsetX = (_board.Width - 1) * cellSize / 2f;
        float offsetY = (_board.Height - 1) * cellSize / 2f;

        return new Vector3(pos.X * cellSize - offsetX, pos.Y * cellSize - offsetY, 0);
    }

    private void AlignBoardToCenter()
    {
        if (_board == null)
        {
            return;
        }

        // 1. 카메라 줌(Orthographic Size) 먼저 결정 (여백 포함)
        float totalRequiredWidth = (_board.Width * cellSize) + (padding * 2f);
        float totalRequiredHeight = (_board.Height * cellSize) + (padding * 2f);

        if (Camera.main != null)
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float sizeByHeight = totalRequiredHeight / 2f;
            float sizeByWidth = (totalRequiredWidth / 2f) / screenAspect;

            Camera.main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
            
            // 카메라 위치 고정 (0,0)
            Camera.main.transform.position = new Vector3(0, 0, -10f);
        }

        // 2. 수직 정렬 오프셋 계산
        float finalY = 0f;
        if (Camera.main != null)
        {
            float camHeightHalf = Camera.main.orthographicSize;
            float boardHeightHalf = (_board.Height * cellSize) / 2f;
            
            // 화면 끝에서 보드 끝까지의 여유 공간 (단방향)
            float availableSpace = camHeightHalf - boardHeightHalf - padding;
            
            // offsetY 비율에 따라 실제 Y 위치 결정 (0.5면 위로 바짝, -0.5면 아래로 바짝)
            finalY = offsetY * 2f * availableSpace;
        }

        // 3. 보드 컨테이너 위치 설정 (X는 0 고정, Y는 계산된 오프셋 적용)
        transform.localPosition = new Vector3(0, finalY, 0);
    }

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

    private void CreateCellView(GridPos gridPos, PuzzleCell cellData)
    {
        if (_cellPrefabObj == null)
        {
            return;
        }

        GameObject cellObj = PoolManager.Instance.Get(_cellPrefabObj, cellRoot);
        cellObj.transform.localPosition = GetLocalPos(gridPos);
        cellObj.name = $"Cell_{gridPos.X}_{gridPos.Y}";

        PuzzleCellView cellView = cellObj.GetComponent<PuzzleCellView>();
        if (cellView == null)
        {
            cellView = cellObj.AddComponent<PuzzleCellView>();
        }

        cellView.Initialize(cellData, gridPos, this);
        _cellViews.Add(gridPos, cellView);
    }

    private void CreateBlockView(GridPos gridPos, BaseBlock blockData)
    {
        if (_blockPrefabObj == null)
        {
            return;
        }

        GameObject blockObj = PoolManager.Instance.Get(_blockPrefabObj, blockRoot);
        blockObj.transform.localPosition = GetLocalPos(gridPos);
        blockObj.name = $"Block_{gridPos.X}_{gridPos.Y}";

        PuzzleBlockView blockView = blockObj.GetComponent<PuzzleBlockView>();
        if (blockView != null)
        {
            blockView.Initialize(blockData, gridPos, this);
            _blockViews.Add(gridPos, blockView);
        }
    }

    private void Update()
    {
        if (_board == null)
        {
            return;
        }

        // --- 매 프레임 블럭 상태 시각적 동기화 ---
        foreach (var kvp in _blockViews)
        {
            if (kvp.Value != null)
            {
                kvp.Value.UpdateStateVisual();
            }
        }

        List<BoardViewAction> actions = _board.FetchActions();
        if (actions != null && actions.Count > 0)
        {
            var moveActions = actions.Where(a => a.type == ViewType.Move).ToList();
            var otherActions = actions.Where(a => a.type != ViewType.Move).ToList();

            if (moveActions.Count > 0)
            {
                HandleBatchMove(moveActions);
            }

            foreach (var action in otherActions)
            {
                ProcessViewAction(action);
            }
        }
    }

    private void HandleBatchMove(List<BoardViewAction> moveActions)
    {
        Dictionary<GridPos, PuzzleBlockView> tempMovingViews = new Dictionary<GridPos, PuzzleBlockView>();
        
        foreach (var action in moveActions)
        {
            if (_blockViews.TryGetValue(action.position, out PuzzleBlockView view))
            {
                tempMovingViews[action.position] = view;
                _blockViews.Remove(action.position);
            }
        }

        foreach (var action in moveActions)
        {
            if (tempMovingViews.TryGetValue(action.position, out PuzzleBlockView view))
            {
                GridPos to = action.targetPosition;
                _blockViews[to] = view;

                view.transform.localPosition = GetLocalPos(to);
                view.Initialize(view.GetBlockData(), to, this);
            }
        }
    }

    private void ProcessViewAction(BoardViewAction action)
    {
        switch (action.type)
        {
            case ViewType.Destroy:
                HandleDestroyAction(action.position);
                break;
            case ViewType.Create:
                HandleCreateAction(action.position);
                break;
        }
    }

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

    public void OnBlockInput(GridPos pos)
    {
        if (_board != null)
        {
            _board.Input(pos);
        }
    }

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
