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
    /// <summary> 생성할 블럭의 프리팹 에셋 </summary>
    public GameObject blockPrefab;

    [Header("Settings")]
    /// <summary> 타일 한 칸의 시각적 크기 </summary>
    public float cellSize = 1.0f;

    /// <summary> 현재 연결된 보드 모델 데이터 </summary>
    private PuzzleBoard _board;
    
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

        if (_board.Cells == null) return;

        // 보드의 모든 셀을 순회하며 블럭이 있는 경우 뷰를 생성
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

    /// <summary>
    /// 특정 좌표에 새로운 블럭 뷰를 생성하고 배치합니다.
    /// </summary>
    /// <param name="gridPos">블럭이 배치될 그리드 좌표</param>
    /// <param name="blockData">블럭의 모델 데이터</param>
    private void CreateBlockView(GridPos gridPos, PuzzleBlock blockData)
    {
        // 그리드 좌표를 월드 좌표(Vector3)로 변환
        Vector3 worldPos = new Vector3(gridPos.X * cellSize, gridPos.Y * cellSize, 0);

        // 블럭 프리팹 인스턴스화
        GameObject blockObj = Instantiate(blockPrefab, worldPos, Quaternion.identity, this.transform);
        blockObj.name = $"Block_{gridPos.X}_{gridPos.Y}";

        // 뷰 컴포넌트 초기화 및 리스트 추가
        PuzzleBlockView blockView = blockObj.GetComponent<PuzzleBlockView>();
        if (blockView != null)
        {
            blockView.Initialize(blockData);
            _blockViews.Add(gridPos, blockView);
        }
    }
}
