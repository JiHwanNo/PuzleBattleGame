using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 팝업 프리팹에 부착하는 베이스 클래스.
/// 팝업의 열림/닫힘 생명주기, 이벤트, 연출(애니메이션)을 담당합니다.
/// </summary>
public class PopupBase : MonoBehaviour, IDomainNode
{
    /// <summary> 팝업 식별 이름 (도메인 경로 추적에 사용) </summary>
    public string PopupName { get; private set; }

    /// <inheritdoc/>
    public string DomainName
    {
        get
        {
            return PopupName;
        }
    }

    /// <inheritdoc/>
    public DomainType DomainType
    {
        get
        {
            return DomainType.Popup;
        }
    }

    /// <summary> 팝업 열림 완료 시 발생하는 이벤트 </summary>
    public event Action<PopupBase> OnOpened;

    /// <summary> 팝업 닫힘 완료 시 발생하는 이벤트 </summary>
    public event Action<PopupBase> OnClosed;

    /// <summary> 현재 연출 진행 중 여부 </summary>
    public bool IsAnimating { get; private set; }

    /// <summary>
    /// 팝업 이름을 설정합니다. DomainManager에서 생성 시 호출됩니다.
    /// </summary>
    /// <param name="popupName">팝업 식별 이름</param>
    public void SetPopupName(string popupName)
    {
        PopupName = popupName;
    }

    /// <summary>
    /// 팝업 열림 처리를 시작합니다. DomainManager에서 호출됩니다.
    /// 열림 연출이 끝난 후 OnOpened 이벤트가 발생합니다.
    /// </summary>
    public void Open()
    {
        gameObject.SetActive(true);
        OnOpenStart();
        StartCoroutine(OpenRoutine());
    }

    /// <summary>
    /// 팝업 닫힘 처리를 시작합니다. DomainManager에서 호출됩니다.
    /// 닫힘 연출이 끝난 후 OnClosed 이벤트가 발생하고 오브젝트가 파괴됩니다.
    /// </summary>
    /// <param name="onComplete">닫힘 연출 완료 후 호출되는 콜백 (오브젝트 파괴 등)</param>
    public void Close(Action onComplete = null)
    {
        OnCloseStart();
        StartCoroutine(CloseRoutine(onComplete));
    }

    /// <summary>
    /// 열림 연출 코루틴. 하위 클래스에서 OpenAnimation을 오버라이드하여 연출을 구현합니다.
    /// </summary>
    /// <returns>코루틴 열거자</returns>
    private IEnumerator OpenRoutine()
    {
        IsAnimating = true;
        yield return StartCoroutine(OpenAnimation());
        IsAnimating = false;

        OnOpenComplete();
        OnOpened?.Invoke(this);
    }

    /// <summary>
    /// 닫힘 연출 코루틴. 하위 클래스에서 CloseAnimation을 오버라이드하여 연출을 구현합니다.
    /// </summary>
    /// <param name="onComplete">연출 완료 후 호출되는 콜백</param>
    /// <returns>코루틴 열거자</returns>
    private IEnumerator CloseRoutine(Action onComplete)
    {
        IsAnimating = true;
        yield return StartCoroutine(CloseAnimation());
        IsAnimating = false;

        OnCloseComplete();
        OnClosed?.Invoke(this);
        onComplete?.Invoke();
    }

    #region 하위 클래스 오버라이드 포인트

    /// <summary>
    /// 열림 연출 시작 직전에 호출됩니다. 초기 상태 설정 등에 활용합니다.
    /// </summary>
    protected virtual void OnOpenStart()
    {
    }

    /// <summary>
    /// 열림 연출이 완료된 후 호출됩니다. 버튼 활성화 등에 활용합니다.
    /// </summary>
    protected virtual void OnOpenComplete()
    {
    }

    /// <summary>
    /// 닫힘 연출 시작 직전에 호출됩니다.
    /// </summary>
    protected virtual void OnCloseStart()
    {
    }

    /// <summary>
    /// 닫힘 연출이 완료된 후 호출됩니다. 리소스 해제 등에 활용합니다.
    /// </summary>
    protected virtual void OnCloseComplete()
    {
    }

    /// <summary>
    /// 열림 연출 코루틴. 하위 클래스에서 오버라이드하여 애니메이션을 구현합니다.
    /// 기본 구현은 연출 없이 즉시 완료됩니다.
    /// </summary>
    /// <returns>코루틴 열거자</returns>
    protected virtual IEnumerator OpenAnimation()
    {
        yield break;
    }

    /// <summary>
    /// 닫힘 연출 코루틴. 하위 클래스에서 오버라이드하여 애니메이션을 구현합니다.
    /// 기본 구현은 연출 없이 즉시 완료됩니다.
    /// </summary>
    /// <returns>코루틴 열거자</returns>
    protected virtual IEnumerator CloseAnimation()
    {
        yield break;
    }

    #endregion
}
