using Puzzle.Core;
using UnityEngine;

/// <summary>
/// 개별 퍼즐 블럭의 시각적 표현과 애니메이션을 담당하는 클래스입니다.
/// </summary>
public class PuzzleBlockView : MonoBehaviour    
{
    /// <summary> 블럭 이미지를 렌더링할 컴포넌트 </summary>
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    /// <summary> 블럭의 물리적 충돌 및 입력을 담당하는 컴포넌트 </summary>
    [SerializeField]
    private PuzzleBlockCollider _boxCollider;

    /// <summary> 외부(Collider 등)에서 스프라이트 정보를 얻기 위한 프로퍼티 </summary>
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            return _spriteRenderer;
        }
    }

    /// <summary> 이 뷰와 연결된 블럭 모델 데이터 </summary>
    private BaseBlock _blockData;

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
    public void Initialize(BaseBlock blockData, GridPos pos, PuzzleBoardView boardView)
    {
        _blockData = blockData;
        _gridPos = pos;
        _boardView = boardView;
        UpdateVisual();
        UpdateStateVisual();
    }

    /// <summary>
    /// 모델 데이터의 상태에 맞춰 블럭의 스프라이트나 색상 등을 업데이트합니다.
    /// </summary>
    public void UpdateVisual()
    {
        if (_blockData == null)
        {
            return;
        }

        if (_spriteRenderer != null)
        {
            string blockId = _blockData.GetBlockId();
            string address = $"Block_{blockId}";

            // 레이어 순서 조정 (배경보다 앞에 나오도록)
            _spriteRenderer.sortingOrder = 10;

            AssetManager.AssetArguments<Sprite> args = new AssetManager.AssetArguments<Sprite>
            {
                address = address,
                successCallback = (sprite) =>
                {
                    // 비동기 로드 도중 블럭이 파괴되었을 수 있으므로 null 체크 필수
                    if (this == null || _spriteRenderer == null)
                    {
                        return;
                    }

                    _spriteRenderer.sprite = sprite;

                    if (_boxCollider != null)
                    {
                        _boxCollider.AdjustColliderSize();
                    }
                },
                failedCallback = () =>
                {
                    if (this == null)
                    {
                        return;
                    }
                    Debug.LogWarning($"[PuzzleBlockView] 스프라이트 로드 실패! 주소: {address}");
                }
            };

            AssetManager.Instance.LoadAssetAsync(args);
        }
    }

    /// <summary>
    /// 블럭의 논리적 상태(State)에 따라 색상이나 크기 등 시각적 피드백을 업데이트합니다.
    /// </summary>
    public void UpdateStateVisual()
    {
        if (_blockData == null || _spriteRenderer == null)
        {
            return;
        }

        switch (_blockData.State)
        {
            case BlockState.Selected:
                // 선택 시 노란색 강조 및 약간 커짐
                _spriteRenderer.color = Color.yellow;
                transform.localScale = Vector3.one * 1.1f;
                break;

            case BlockState.Matched:
                // 매칭 시 반투명하게 표시
                _spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                transform.localScale = Vector3.one;
                break;

            case BlockState.Moving:
            case BlockState.Falling:
                // 이동/낙하 중에는 기본 상태 유지 (필요 시 잔상 효과 등 추가 가능)
                _spriteRenderer.color = Color.white;
                transform.localScale = Vector3.one;
                break;

            default:
                // 기본 상태 (Idle 등)
                _spriteRenderer.color = Color.white;
                transform.localScale = Vector3.one;
                break;
        }
    }

    /// <summary>
    /// 이 뷰가 참조하고 있는 블럭 모델 데이터를 반환합니다.
    /// </summary>
    /// <returns>연결된 블럭 모델 데이터</returns>
    public BaseBlock GetBlockData()
    {
        return _blockData;
    }

    /// <summary>
    /// 보드 뷰(Raycast) 등에 의해 이 블럭이 클릭되었다고 판정되었을 때 호출됩니다.
    /// </summary>
    public void OnClicked()
    {
        string blockId = _blockData != null ? _blockData.GetBlockId() : "Unknown";
        Debug.Log($"[PuzzleBlockView] 블럭 클릭됨! 아이디: {blockId}, 위치: ({_gridPos.X}, {_gridPos.Y})");

        if (_boardView != null)
        {
            _boardView.OnBlockInput(_gridPos);
        }
    }
}
