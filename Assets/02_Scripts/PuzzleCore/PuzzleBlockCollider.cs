using UnityEngine;

/// <summary>
/// 유니티 내장 물리/이벤트(마우스 클릭 등)를 감지하여
/// 블럭 뷰(PuzzleBlockView)로 전달하는 역할을 합니다.
/// </summary>
public class PuzzleBlockCollider : MonoBehaviour
{
    [SerializeField]
    PuzzleBlockView _block;
    [SerializeField]
    BoxCollider2D _boxCollider2D;

    /// <summary>
    /// 마우스 또는 터치로 BoxCollider2D 영역이 클릭되었을 때 유니티에서 자동 호출됩니다.
    /// </summary>
    private void OnMouseDown()
    {
        OnClickBlock();
    }

    /// <summary>
    /// 뷰 쪽에 클릭 이벤트를 전달합니다.
    /// </summary>
    internal void OnClickBlock()
    {
        if (_block != null)
        {
            _block.OnClicked();
        }
    }

    /// <summary>
    /// 연결된 뷰(PuzzleBlockView)의 스프라이트 크기를 확인하여 BoxCollider2D 크기를 조절합니다.
    /// </summary>
    public void AdjustColliderSize()
    {
        if (_block != null && _block.SpriteRenderer != null && _block.SpriteRenderer.sprite != null)
        {
            _boxCollider2D.size = _block.SpriteRenderer.sprite.bounds.size;
        }
    }
}
