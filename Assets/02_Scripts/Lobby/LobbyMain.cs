using UnityEngine;

/// <summary>
/// 로비 씬의 메인 로직을 담당하는 클래스입니다.
/// </summary>
public class LobbyMain : MonoBehaviour
{
    /// <summary>
    /// 객체 생성 시 초기화를 수행합니다.
    /// </summary>
    void Awake()
    {
        Main.Instance.Init(Main.Scene.LobbyScene);
        var popupManager = PopupManager.Instance;
    }

    /// <summary>
    /// 스테이지 시작 버튼 클릭 시 호출되며, 데이터를 준비하고 게임 씬으로 이동합니다.
    /// </summary>
    void OnClickStartStage()
    {
        // Addressable 주소는 프로젝트 설정에 따라 다를 수 있습니다. 
        // 여기서는 파일 이름(확장자 포함)으로 가정합니다.
        string rulePath = "GameRule.json";
        string stagePath = "Stage.json";

        // 스테이지 및 규칙 데이터 준비
        StageInjection.Instance.MakeGameSpec(rulePath, stagePath);

        // 데이터 준비 성공 여부 확인 후 씬 이동
        if (StageInjection.Instance.GetGameSpec() != null)
        {
            Main.Instance.MoveScene(Main.Scene.GameScene);
        }
        else
        {
            Debug.LogError("게임 씬으로 이동하기 전 GameSpec 준비에 실패했습니다.");
        }
    }
}
