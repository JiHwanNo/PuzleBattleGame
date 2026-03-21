using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 유니티 버튼 컴포넌트를 래핑하여 공통적인 사운드 및 이벤트를 처리하는 UI 클래스입니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour
{
    /// <summary> 버튼 클릭 시 실행될 추가 이벤트 </summary>
    public UnityEvent onClick;

    /// <summary>
    /// 버튼 컴포넌트의 클릭 리스너를 등록합니다.
    /// </summary>
    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// 버튼이 실제 클릭되었을 때 호출되는 내부 메서드입니다.
    /// </summary>
    private void OnClicked()
    {
        // TODO: 버튼 클릭 공통 사운드 재생 로직 등 추가 가능
        
        if (onClick != null)
        {
            onClick.Invoke();
        }
    }
}
