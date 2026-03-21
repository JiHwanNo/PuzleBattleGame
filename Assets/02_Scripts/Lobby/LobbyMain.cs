using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 로비 씬의 UI 이벤트와 흐름을 제어하는 클래스입니다.
/// </summary>
public class LobbyMain : MonoBehaviour
{
    /// <summary>
    /// 로비 씬이 시작될 때 초기화를 수행합니다.
    /// </summary>
    private void Start()
    {
        Debug.Log("[LobbyMain] 로비 씬 진입.");
    }

    /// <summary>
    /// 스테이지 시작 버튼 클릭 시 호출됩니다.
    /// StageInjection을 통해 데이터를 준비하고 게임 씬으로 이동합니다.
    /// </summary>
    public void OnClickStartStage()
    {
        Debug.Log("[LobbyMain] 스테이지 시작 요청.");
        
        if (StageInjection.Instance != null)
        {
            // 스테이지 및 규칙 데이터 주입 시도
            bool success = StageInjection.Instance.InjectStageData();
            if (success)
            {
                // 성공 시 게임 씬으로 이동
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError("[LobbyMain] 스테이지 데이터 로드 실패! 사양서가 올바른지 확인하세요.");
            }
        }
    }
}
