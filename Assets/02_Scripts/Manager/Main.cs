using UnityEngine;

/// <summary>
/// 게임의 진입점(Entry Point) 역할을 수행하는 클래스입니다.
/// 초기 씬에서 매니저들을 생성하고 초기화하는 역할을 담당합니다.
/// </summary>
public class Main : MonoBehaviour
{
    /// <summary>
    /// 게임 시작 시 매니저 인스턴스들을 생성합니다.
    /// </summary>
    private void Awake()
    {
        // 싱글톤 매니저들 강제 생성 및 유지
        var am = AssetManager.Instance;
        var pm = PoolManager.Instance;
        var pop = PopupManager.Instance;
        var si = StageInjection.Instance;

        Debug.Log("[Main] 시스템 매니저 초기화 완료.");
    }
}
