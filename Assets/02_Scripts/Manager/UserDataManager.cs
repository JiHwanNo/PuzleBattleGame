using UnityEngine;

/// <summary>
/// 유저 데이터 저장 및 관리를 담당하는 싱글톤 매니저
/// </summary>
public class UserDataManager : MonoBehaviour
{
    private static UserDataManager _instance;

    /// <summary> 전역 접근을 위한 싱글톤 인스턴스 </summary>
    public static UserDataManager Instance
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
        DontDestroyOnLoad(gameObject);
    }
}
