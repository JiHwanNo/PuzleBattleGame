using UnityEngine;

/// <summary>
/// 로비 씬의 메인 로직을 담당하는 클래스입니다.
/// </summary>
public class LobbyMain : MonoBehaviour
{
    /// <summary>
    /// 객체 생성 시 초기화를 수행합니다.
    /// </summary>
    private void Awake()
    {
        Main.Instance.Init(Main.Scene.LobbyScene);
        var popupManager = PopupManager.Instance;
    }

    /// <summary>
    /// 스테이지 시작 버튼 클릭 시 호출되며, 데이터를 준비하고 게임 씬으로 이동합니다.
    /// </summary>
    public void OnClickStartStage()
    {
        string rulePath = "TapMatchRule";
        string stagePath = "Stage";

        StageInjection.Instance.MakeGameSpec(rulePath, stagePath);
        
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
