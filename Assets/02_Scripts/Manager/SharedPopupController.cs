/// <summary>
/// SharedScene 전용 팝업 컨트롤러. 네트워크 에러, 점검 공지 등 공용 팝업을 관리합니다.
/// </summary>
public class SharedPopupController : PopupController
{
    /// <summary> 컨트롤러 식별 이름 </summary>
    public override string ControllerName => "Shared";
}
