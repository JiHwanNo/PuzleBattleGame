using Puzzle.Core;
using UnityEngine;

/// <summary>
/// 개별 퍼즐 셀(배경 타일)의 시각적 표현을 담당하는 클래스입니다.
/// </summary>
public class PuzzleCellView : MonoBehaviour
{
    /// <summary> 셀 배경 이미지 </summary>
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
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
    /// 모델 데이터의 상태에 맞춰 셀의 스프라이트나 색상 등을 업데이트합니다.
    /// </summary>
    public void UpdateVisual()
    {
        if (_cellData == null) return;

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            // 셀 타입에 따른 시각적 차별화 (예: 빈 칸, 장애물, 특수 타일 등)
            CellType cellType = _cellData.CellType;
            
            // Addressables를 통해 스프라이트 비동기 로드
            // 셀의 경우 Cell_TypeEnumName 형식으로 주소를 지정했다고 가정
            string address = $"Cell_{cellType.ToString()}";
            
            AssetManager.AssetArguments<Sprite> args = new AssetManager.AssetArguments<Sprite>
            {
                address = address,
                successCallback = (sprite) =>
                {
                    if (_spriteRenderer != null)
                    {
                        _spriteRenderer.sprite = sprite;
                    }
                },
                failedCallback = () =>
                {
                    // 일반 타일(Normal) 외의 특수한 타일 이미지가 없을 경우를 대비한 경고 최소화
                    if (cellType != CellType.Normal)
                    {
                        Debug.LogWarning($"[PuzzleCellView] 스프라이트 로드 실패! 주소: {address}");
                    }
                }
            };
            
            AssetManager.Instance.LoadAssetAsync(args);
        }
    }

    /// <summary>
    /// 보드 뷰(Raycast) 등에 의해 이 셀이 클릭되었다고 판정되었을 때 호출됩니다.
    /// </summary>
    public void OnClicked()
    {
        if (_boardView != null)
        {
            // 셀 터치 시에도 동일하게 보드 뷰의 OnBlockInput을 호출하거나 별도의 처리를 할 수 있습니다.
            // 일단 블럭과 동일한 입력 처리를 위해 OnBlockInput을 사용합니다.
            _boardView.OnBlockInput(_gridPos);
        }
    }
}
