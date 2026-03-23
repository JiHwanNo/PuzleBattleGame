using Puzzle.Core;
using UnityEngine;
using DG.Tweening;
using System;

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

    /// <summary> 현재 그리드 좌표를 반환하는 프로퍼티 (Controller 등에서 활용) </summary>
    public GridPos GridPos
    {
        get
        {
            return _gridPos;
        }
    }

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

        // 생성되거나 풀에서 재사용될 때 이전의 애니메이션 상태(예: Scale 0)를 리셋
        transform.DOKill();
        transform.localScale = Vector3.one;

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
                // 선택 시 노란색 강조
                _spriteRenderer.color = Color.yellow;
                break;

            case BlockState.Matched:
                // 매칭 시 반투명하게 표시
                _spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                break;

            default:
                // 기본 상태
                _spriteRenderer.color = Color.white;
                break;
        }
    }

    /// <summary>
    /// 블럭이 클릭되었을 때의 시각적 연출을 수행합니다.
    /// </summary>
    public void PlayClickAnimation(Action onComplete)
    {
        transform.DOScale(1.1f, 0.066f).SetLoops(2, LoopType.Yoyo).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 지정된 좌표로 부드럽게 이동하는 애니메이션을 수행합니다.
    /// </summary>
    public void PlayMoveAnimation(Vector3 targetLocalPos, Action onComplete)
    {
        transform.DOLocalMove(targetLocalPos, 0.132f).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 지정된 좌표로 낙하하는 애니메이션을 수행합니다.
    /// </summary>
    public void PlayFallAnimation(Vector3 targetLocalPos, Action onComplete)
    {
        float duration = 0.132f; // 거리와 무관하게 동일한 시간으로 떨어지게 하거나 거리 비례로 할 수 있습니다.
        transform.DOLocalMove(targetLocalPos, duration).SetEase(Ease.OutQuad).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 블럭이 파괴될 때의 시각적 연출을 수행합니다.
    /// </summary>
    public void PlayDestroyAnimation(Action onComplete)
    {
        transform.DOScale(0f, 0.132f).SetEase(Ease.InBack).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 블럭이 생성될 때의 시각적 연출을 수행합니다.
    /// </summary>
    public void PlayCreateAnimation(Action onComplete)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1.0f, 0.132f).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());
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
        // 연출 중이면 입력 무시
        if (_boardView != null && _boardView.IsAnimating)
        {
            return;
        }

        // 로직 전달은 즉시 수행
        if (_boardView != null)
        {
            _boardView.OnBlockInput(_gridPos);
        }

        // 클릭 애니메이션 재생 (비동기)
        PlayClickAnimation(null);
    }
}
