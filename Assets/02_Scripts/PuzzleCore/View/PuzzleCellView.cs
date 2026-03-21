using Puzzle.Core;
using UnityEngine;

/// <summary>
/// 개별 퍼즐 셀(타일)의 시각적 표현을 담당하는 클래스입니다.
/// </summary>
public class PuzzleCellView : MonoBehaviour
{
    /// <summary> 이 뷰와 연결된 셀 모델 데이터 </summary>
    private PuzzleCell _cellData;

    /// <summary> 이 셀이 위치한 보드상의 그리드 좌표 </summary>
    private GridPos _gridPos;

    /// <summary> 이 셀을 관리하는 보드 뷰 참조 </summary>
    private PuzzleBoardView _boardView;

    /// <summary>
    /// 셀 뷰를 특정 모델 데이터 및 좌표로 초기화합니다.
    /// </summary>
    /// <param name="cellData">연결할 셀 모델 객체</param>
    /// <param name="pos">배치될 그리드 좌표</param>
    /// <param name="boardView">관리 중인 보드 뷰 객체</param>
    public void Initialize(PuzzleCell cellData, GridPos pos, PuzzleBoardView boardView)
    {
        _cellData = cellData;
        _gridPos = pos;
        _boardView = boardView;
        
        UpdateVisual();
    }

    /// <summary>
    /// 모델 데이터의 속성(CellType 등)에 맞춰 타일의 외형을 업데이트합니다.
    /// </summary>
    public void UpdateVisual()
    {
        if (_cellData == null)
        {
            return;
        }

        // TODO: CellType에 따른 스프라이트 변경이나 특수 효과 추가 가능
    }
}
