/// <summary>
/// 도메인 항목의 종류를 정의합니다.
/// </summary>
public enum DomainType
{
    /// <summary> 팝업 도메인 (스택 방식, 열고 닫을 때 생성/파괴) </summary>
    Popup,

    /// <summary> 탭 도메인 (전환 방식, 활성/비활성으로 전환) </summary>
    Tab
}
