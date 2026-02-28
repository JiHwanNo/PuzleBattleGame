using UnityEngine;
using System.Collections.Generic;
using Puzzle.Core;

public class PuzzleBoardView : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject blockPrefab; // 생성할 블록 프리팹 (PuzzleBlockView가 붙어있어야 함)

    [Header("Settings")]
    public float cellSize = 1.0f; // 블록 간의 렌더링 간격

    private PuzzleBoard _board;
    private Dictionary<GridPos, PuzzleBlockView> _blockViews = new Dictionary<GridPos, PuzzleBlockView>();

    // 코어의 보드 데이터를 받아와서 화면에 그립니다.
    public void DrawBoard(PuzzleBoard boardData)
    {
        _board = boardData;

        // 기존에 생성된 뷰가 있다면 초기화합니다.
        foreach (var view in _blockViews.Values)
        {
            Destroy(view.gameObject);
        }
        _blockViews.Clear();

        if (_board.Cells == null) return;

        // 코어 보드의 모든 셀을 순회하며 시각적 블록을 생성합니다.
        foreach (var kvp in _board.Cells) //
        {
            GridPos gridPos = kvp.Key; //
            PuzzleCell cell = kvp.Value; //

            // 셀 위에 블록이 존재한다면 화면에 오브젝트를 생성합니다.
            if (cell.Block != null) //
            {
                CreateBlockView(gridPos, cell.Block);
            }
        }
    }

    private void CreateBlockView(GridPos gridPos, PuzzleBlock blockData)
    {
        // GridPos를 Unity의 실제 공간 좌표로 변환합니다.
        Vector3 worldPos = new Vector3(gridPos.X * cellSize, gridPos.Y * cellSize, 0);

        // 프리팹을 인스턴스화하여 화면에 배치합니다.
        GameObject blockObj = Instantiate(blockPrefab, worldPos, Quaternion.identity, this.transform);
        blockObj.name = $"Block_{gridPos.X}_{gridPos.Y}";

        // 뷰 컴포넌트를 가져와서 내부 데이터를 초기화합니다.
        PuzzleBlockView blockView = blockObj.GetComponent<PuzzleBlockView>();
        if (blockView != null)
        {
            blockView.Initialize(blockData);
            _blockViews.Add(gridPos, blockView);
        }
    }
}