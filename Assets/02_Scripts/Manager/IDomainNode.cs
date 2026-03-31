/// <summary>
/// 도메인 경로의 각 노드를 나타내는 인터페이스.
/// 팝업(PopupBase)과 탭(TabBase) 모두 이 인터페이스를 구현합니다.
/// </summary>
public interface IDomainNode
{
    /// <summary> 도메인 경로에서 사용되는 이름 </summary>
    string DomainName { get; }

    /// <summary> 도메인 항목의 종류 (팝업 또는 탭) </summary>
    DomainType DomainType { get; }
}
