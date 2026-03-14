using Puzzle.Core;
using UnityEngine;

/// <summary>
/// 개별 퍼즐 블럭의 시각적 표현과 애니메이션을 담당하는 클래스입니다.
/// </summary>
public class PuzzleBlockView : MonoBehaviour
{
    /// <summary> 블럭 이미지 </summary>
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    /// <summary> 이 뷰와 연결된 블럭 모델 데이터 </summary>
    private PuzzleBlock _blockData;
    /// <summary> 이 블럭이 위치한 보드상의 그리드 좌표 </summary>
    private GridPos _gridPos;
    /// <summary> 이 블럭을 관리하는 보드 뷰 참조 </summary>
    private PuzzleBoardView _boardView;

    /// <summary>
    /// 블럭 뷰를 특정 모델 데이터 및 좌표로 초기화합니다.
    /// </summary>
    /// <param name="blockData">연결할 블럭 모델 객체</param>
    /// <param name="pos">배치될 그리드 좌표</param>
    /// <param name="boardView">관리 중인 보드 뷰 객체</param>
    public void Initialize(PuzzleBlock blockData, GridPos pos, PuzzleBoardView boardView)
    {
        _blockData = blockData;
        _gridPos = pos;
        _boardView = boardView;
        UpdateVisual();
    }

    /// <summary>
    /// 모델 데이터의 상태에 맞춰 블럭의 스프라이트나 색상 등을 업데이트합니다.
    /// </summary>
    public void UpdateVisual()
    {
        if (_blockData == null) return;

        if (_spriteRenderer != null)
        {
            string blockId = _blockData.GetBlockId();

            string address = $"Block_{blockId}";

            AssetManager.AssetArguments<Sprite> args = new AssetManager.AssetArguments<Sprite>
            {
                address = address,
                successCallback = (sprite) =>
                {
                    _spriteRenderer.sprite = sprite;
                },
                failedCallback = () =>
                {
                    Debug.LogWarning($"[PuzzleBlockView] 스프라이트 로드 실패! 주소: {address}");
                }
            };

            AssetManager.Instance.LoadAssetAsync(args);
        }
    }

    /// <summary>
    /// 보드 뷰(Raycast) 등에 의해 이 블럭이 클릭되었다고 판정되었을 때 호출됩니다.
    /// </summary>
    public void OnClicked()
    {
        if (_boardView != null)
        {
            _boardView.OnBlockInput(_gridPos);
        }
    }
}
