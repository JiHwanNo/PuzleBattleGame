using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 내 팝업 시스템을 관리하는 매니저 클래스입니다.
/// </summary>
public class PopupManager : MonoBehaviour
{
    /// <summary> 싱글톤 인스턴스 </summary>
    private static PopupManager _instance;
    
    /// <summary> 팝업 전용 씬 이름 </summary>
    public const string PopupSceneName = "PopupScene";

    /// <summary>
    /// PopupManager의 싱글톤 인스턴스를 반환합니다.
    /// 씬에 없을 경우 자동으로 생성하거나 로드합니다.
    /// </summary>
    public static PopupManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var scene = SceneManager.GetSceneByName(PopupSceneName);
                if (!scene.isLoaded)
                    SceneManager.LoadScene(PopupSceneName, LoadSceneMode.Additive);

                _instance = FindFirstObjectByType<PopupManager>();

                if (_instance == null)
                {
                    var go = new GameObject("PopupManager");
                    _instance = go.AddComponent<PopupManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
}
