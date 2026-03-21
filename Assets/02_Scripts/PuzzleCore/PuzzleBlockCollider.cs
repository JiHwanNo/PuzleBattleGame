using UnityEngine;

/// <summary>
/// 유니티의 물리 충돌 및 마우스 이벤트를 감지하여 PuzzleBlockView에 전달하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PuzzleBlockCollider : MonoBehaviour
{
    /// <summary> 이 콜라이더가 속한 블럭 뷰 </summary>
    [SerializeField]
    private PuzzleBlockView _blockView;

    /// <summary> 충돌체 컴포넌트 </summary>
    private BoxCollider2D _collider;

    /// <summary>
    /// 컴포넌트 초기화 및 콜라이더 참조를 확보합니다.
    /// </summary>
    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        
        if (_blockView == null)
        {
            _blockView = GetComponentInParent<PuzzleBlockView>();
        }
    }

    /// <summary>
    /// 블럭이 클릭되었을 때 호출되어 뷰의 입력 로직을 실행합니다.
    /// </summary>
    public void OnClickBlock()
    {
        if (_blockView != null)
        {
            _blockView.OnClicked();
        }
    }

    /// <summary>
    /// 로드된 스프라이트의 실제 크기에 맞춰 BoxCollider2D의 크기를 자동으로 조절합니다.
    /// </summary>
    public void AdjustColliderSize()
    {
        if (_blockView != null && _blockView.SpriteRenderer != null && _collider != null)
        {
            Sprite sprite = _blockView.SpriteRenderer.sprite;
            if (sprite != null)
            {
                // 스프라이트의 bounds 크기를 그대로 콜라이더 크기로 적용
                _collider.size = sprite.bounds.size;
            }
        }
    }
}
