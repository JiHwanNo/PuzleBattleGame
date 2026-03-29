using UnityEngine;

/// <summary>
/// 각 씬에서 팝업을 운영하는 컨트롤러의 베이스 클래스.
/// PopupManager에 자동으로 등록/해제됩니다.
/// </summary>
public abstract class PopupController : MonoBehaviour
{
    /// <summary> 컨트롤러 식별 이름 </summary>
    public abstract string ControllerName { get; }

    /// <summary>
    /// 활성화 시 PopupManager에 등록
    /// </summary>
    protected virtual void OnEnable()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.Register(this);
        }
    }

    /// <summary>
    /// 비활성화 시 PopupManager에서 해제
    /// </summary>
    protected virtual void OnDisable()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.Unregister(this);
        }
    }
}
