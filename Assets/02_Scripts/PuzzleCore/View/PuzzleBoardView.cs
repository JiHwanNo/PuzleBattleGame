using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Puzzle.Core;
using System.Collections;

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

    /// <summary> 보드 연출 액션 그룹 대기열 (프레임 단위로 그룹화됨) </summary>
    private Queue<List<BoardViewAction>> _actionQueue = new Queue<List<BoardViewAction>>();

    /// <summary> 현재 연출이 진행 중인지 여부 </summary>
    private bool _isAnimating = false;

    /// <summary> 드래그 연결 궤적을 표시하는 라인 렌더러 </summary>
    private LineRenderer _lineRenderer;

    /// <summary> 현재 보드가 애니메이션 연출 중인지 여부를 반환합니다. </summary>
    public bool IsAnimating => _isAnimating;

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
        SetupLineRenderer();

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

        AlignBoardToCenter();
    }

    private void SetupLineRenderer()
    {
        if (_lineRenderer == null)
        {
            GameObject lineObj = new GameObject("LinkLine");
            lineObj.transform.SetParent(this.transform);
            _lineRenderer = lineObj.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = 0.15f;
            _lineRenderer.endWidth = 0.15f;
            _lineRenderer.positionCount = 0;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.sortingOrder = 10;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = new Color(1f, 1f, 1f, 0.8f);
            _lineRenderer.endColor = new Color(1f, 1f, 1f, 0.8f);
        }
        _lineRenderer.positionCount = 0;
    }

    public Vector3 GetLocalPos(GridPos pos)
    {
        if (_board == null)
        {
            return Vector3.zero;
        }

        float offsetX = (_board.Width - 1) * cellSize / 2f;
        float offsetY = (_board.Height - 1) * cellSize / 2f;

        float xPos = pos.X * cellSize - offsetX;
        float yPos = pos.Y * cellSize - offsetY;

        // 육각형(Hexagon) 오프셋 처리 (Even-Q Flat-Top)
        GameSpec spec = StageInjection.Instance.GetGameSpec();
        if (spec != null && spec.rule.boardShape == BoardShape.Hexagon)
        {
            if (pos.X % 2 == 0)
            {
                yPos -= cellSize * 0.5f; // 짝수 열은 반 칸 아래로 배치
            }
        }

        return new Vector3(xPos, yPos, 0);
    }

    private void AlignBoardToCenter()
    {
        if (_board == null)
        {
            return;
        }

        float totalRequiredWidth = (_board.Width * cellSize) + (padding * 2f);
        float totalRequiredHeight = (_board.Height * cellSize) + (padding * 2f);

        if (Camera.main != null)
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float sizeByHeight = totalRequiredHeight / 2f;
            float sizeByWidth = (totalRequiredWidth / 2f) / screenAspect;

            Camera.main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
            Camera.main.transform.position = new Vector3(0, 0, -10f);
        }

        float finalY = 0f;
        if (Camera.main != null)
        {
            float camHeightHalf = Camera.main.orthographicSize;
            float boardHeightHalf = (_board.Height * cellSize) / 2f;
            float availableSpace = camHeightHalf - boardHeightHalf - padding;
            finalY = offsetY * 2f * availableSpace;
        }

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
        _actionQueue.Clear();
        _isAnimating = false;

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 0;
        }
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

        UpdateLineRenderer();

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
            var groupedActions = actions
                .GroupBy(a => new { a.frame, a.orderIndex })
                .OrderBy(g => g.Key.frame)
                .ThenBy(g => g.Key.orderIndex);

            foreach (var group in groupedActions)
            {
                _actionQueue.Enqueue(group.ToList());
            }
        }

        if (!_isAnimating && _actionQueue.Count > 0)
        {
            StartCoroutine(ProcessActionQueue());
        }
    }

    private void UpdateLineRenderer()
    {
        if (_lineRenderer == null) return;

        if (_board is LinkPuzzleBoard linkBoard)
        {
            var path = linkBoard.GetCurrentLinkPath();
            if (path != null && path.Count > 0)
            {
                _lineRenderer.positionCount = path.Count;
                for (int i = 0; i < path.Count; i++)
                {
                    Vector3 localPos = GetLocalPos(path[i]);
                    Vector3 worldPos = this.transform.TransformPoint(localPos);
                    _lineRenderer.SetPosition(i, worldPos);
                }
            }
            else
            {
                _lineRenderer.positionCount = 0;
            }
        }
        else
        {
            _lineRenderer.positionCount = 0;
        }
    }

    private IEnumerator ProcessActionQueue()
    {
        _isAnimating = true;

        while (_actionQueue.Count > 0)
        {
            List<BoardViewAction> actionGroup = _actionQueue.Dequeue();
            
            var movementActions = actionGroup.Where(a => a.type == ViewType.Move || a.type == ViewType.Fall || a.type == ViewType.CreateAndFall).ToList();
            if (movementActions.Count > 0)
            {
                yield return StartCoroutine(ExecuteBatchMovement(movementActions));
            }

            var otherActions = actionGroup.Where(a => a.type != ViewType.Move && a.type != ViewType.Fall && a.type != ViewType.CreateAndFall).ToList();
            if (otherActions.Count > 0)
            {
                int completedCount = 0;
                int totalCount = otherActions.Count;

                foreach (var action in otherActions)
                {
                    ExecuteSingleAction(action, () => completedCount++);
                }

                while (completedCount < totalCount)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.033f);
        }

        _isAnimating = false;
    }

    private System.Collections.IEnumerator ExecuteBatchMovement(List<BoardViewAction> moveActions)
    {
        int completedCount = 0;
        int totalCount = moveActions.Count;

        Dictionary<BoardViewAction, PuzzleBlockView> actionToViewMap = new Dictionary<BoardViewAction, PuzzleBlockView>();
        
        foreach (var action in moveActions)
        {
            if (action.type == ViewType.Move || action.type == ViewType.Fall)
            {
                if (_blockViews.TryGetValue(action.position, out PuzzleBlockView view))
                {
                    actionToViewMap[action] = view;
                    _blockViews.Remove(action.position); 
                }
            }
            else if (action.type == ViewType.CreateAndFall)
            {
                if (action.blockData != null && _blockPrefabObj != null)
                {
                    if (_blockViews.ContainsKey(action.targetPosition))
                    {
                        HandleImmediateDestroy(action.targetPosition);
                    }

                    GameObject blockObj = PoolManager.Instance.Get(_blockPrefabObj, blockRoot);
                    blockObj.transform.localPosition = GetLocalPos(action.position); 
                    blockObj.name = $"Block_{action.targetPosition.X}_{action.targetPosition.Y}";

                    PuzzleBlockView bView = blockObj.GetComponent<PuzzleBlockView>();
                    if (bView != null)
                    {
                        bView.Initialize(action.blockData, action.position, this);
                        actionToViewMap[action] = bView;
                    }
                }
            }
        }

        foreach (var pair in actionToViewMap)
        {
            BoardViewAction action = pair.Key;
            PuzzleBlockView view = pair.Value;
            GridPos to = action.targetPosition;

            _blockViews[to] = view;

            Vector3 targetPos = GetLocalPos(to);
            System.Action onComplete = () => 
            {
                view.Initialize(view.GetBlockData(), to, this); 
                completedCount++;
            };

            if (action.type == ViewType.Move)
            {
                view.PlayMoveAnimation(targetPos, onComplete);
            }
            else 
            {
                view.PlayFallAnimation(targetPos, onComplete);
            }
        }

        int processedCount = actionToViewMap.Count;
        if (processedCount < totalCount)
        {
            completedCount += (totalCount - processedCount);
        }

        while (completedCount < totalCount)
        {
            yield return null;
        }
    }

    private void ExecuteSingleAction(BoardViewAction action, System.Action onComplete)
    {
        switch (action.type)
        {
            case ViewType.Destroy:
                if (_blockViews.TryGetValue(action.position, out PuzzleBlockView dView))
                {
                    _blockViews.Remove(action.position);
                    dView.PlayDestroyAnimation(() => 
                    {
                        PoolManager.Instance.Release(dView.gameObject);
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    onComplete?.Invoke();
                }
                break;

            case ViewType.Create:
                if (action.blockData != null)
                {
                    if (_blockViews.ContainsKey(action.position))
                    {
                        HandleImmediateDestroy(action.position);
                    }

                    if (_blockPrefabObj != null)
                    {
                        GameObject blockObj = PoolManager.Instance.Get(_blockPrefabObj, blockRoot);
                        blockObj.transform.localPosition = GetLocalPos(action.position);
                        blockObj.name = $"Block_{action.position.X}_{action.position.Y}";

                        PuzzleBlockView bView = blockObj.GetComponent<PuzzleBlockView>();
                        if (bView != null)
                        {
                            bView.Initialize(action.blockData, action.position, this);
                            _blockViews.Add(action.position, bView);
                            bView.PlayCreateAnimation(onComplete);
                        }
                        else
                        {
                            onComplete?.Invoke();
                        }
                    }
                    else
                    {
                        onComplete?.Invoke();
                    }
                }
                else
                {
                    onComplete?.Invoke();
                }
                break;

            default:
                onComplete?.Invoke();
                break;
        }
    }

    private void HandleImmediateDestroy(GridPos pos)
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