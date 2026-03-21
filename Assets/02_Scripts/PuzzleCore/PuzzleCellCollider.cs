using UnityEngine;

/// <summary>
/// 유니티의 물리 충돌 및 마우스 이벤트를 감지하여 PuzzleCellView에 전달하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PuzzleCellCollider : MonoBehaviour
{
    /// <summary> 이 콜라이더가 속한 셀 뷰 </summary>
    [SerializeField]
    private PuzzleCellView _cellView;

    /// <summary>
    /// 컴포넌트 초기화 및 셀 뷰 참조를 확보합니다.
    /// </summary>
    private void Awake()
    {
        if (_cellView == null)
        {
            _cellView = GetComponentInParent<PuzzleCellView>();
        }
    }

    /// <summary>
    /// 셀(타일)이 클릭되었을 때 호출됩니다.
    /// </summary>
    public void OnClickCell()
    {
        // TODO: 블럭이 없는 바닥 클릭 시 필요한 로직 구현
    }
}
