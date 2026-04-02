using UnityEngine;

/// <summary>
/// 서버 통신 및 API 호출/응답을 관리하는 싱글톤 매니저
/// </summary>
public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;

    /// <summary> 전역 접근을 위한 싱글톤 인스턴스 </summary>
    public static NetworkManager Instance
    {
        get
        {
            return _instance;
        }
    }

    /// <summary>
    /// 싱글톤 인스턴스 등록 및 중복 방지
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }
}
