using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 준비(Ready) 팝업.
/// PopupHandler를 상속받아 시작/닫기 버튼 로직을 구현합니다.
/// </summary>
public class PopupReady : PopupHandler
{

    /// <summary>
    /// 시작 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnClickStart()
    {
    }

    /// <summary>
    /// 닫기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnClickClose()
    {
        ClosePopup();   
    }
}
