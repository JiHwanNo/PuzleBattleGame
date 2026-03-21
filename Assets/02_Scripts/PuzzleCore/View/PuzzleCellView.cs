using Puzzle.Core;
using UnityEditor.Build.Pipeline;
using UnityEngine;

/// <summary>
/// 개별 퍼즐 셀(배경 타일)의 시각적 표현을 담당하는 클래스입니다.
/// </summary>
public class PuzzleCellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private PuzzleCellCollider _boxCollider;
    
    /// <summary> 외부(Collider 등)에서 스프라이트 정보를 얻기 위한 프로퍼티 </summary>
    public SpriteRenderer SpriteRenderer => _spriteRenderer;

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
                    
                    if (_boxCollider != null)
                    {
                        _boxCollider.AdjustColliderSize();
                    }
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
        string cellType = _cellData != null ? _cellData.CellType.ToString() : "Unknown";

        if (_boardView == null || _cellData == null)
            return;

        if (_cellData.CellType == CellType.Normal)
        {
            _boardView.OnBlockInput(_gridPos);
        }
    }
}
