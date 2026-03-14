using Puzzle.Core;
using UnityEditor.Build.Pipeline;
using UnityEngine;

/// <summary>
/// 개별 퍼즐 셀(배경 타일)의 시각적 표현을 담당하는 클래스입니다.
/// </summary>
public class PuzzleCellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private PuzzleCell _cellData;
    private GridPos _gridPos;
    private PuzzleBoardView _boardView;

    /// <summary>
    /// 셀 뷰를 특정 모델 데이터 및 좌표로 초기화합니다.
    /// </summary>
    public void Initialize(PuzzleCell cellData, GridPos pos, PuzzleBoardView boardView)
    {
        _cellData = cellData;
        _gridPos = pos;
        _boardView = boardView;

        UpdateVisual();
    }

    /// <summary>
    /// 모델 데이터의 상태에 맞춰 셀의 렌더링 상태를 업데이트합니다.
    /// </summary>
    public void UpdateVisual()
    {
        if (_cellData == null) return;

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            switch (_cellData.CellType)
            {
                case CellType.Normal:
                    _spriteRenderer.gameObject.SetActive(true);
                    break;
                case CellType.Close:
                    _spriteRenderer.gameObject.SetActive(false);
                    break;
            }
        }
    }

    /// <summary>
    /// 보드 뷰(Raycast) 등에 의해 이 셀이 클릭되었을 때 호출됩니다.
    /// </summary>
    public void OnClicked()
    {
        if (_boardView == null || _cellData == null)
            return;

        if (_cellData.CellType == CellType.Normal)
        {
            _boardView.OnBlockInput(_gridPos);
        }
    }
}
