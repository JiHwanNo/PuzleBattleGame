using UnityEngine;

public class LobbyMain : MonoBehaviour
{
    void Awake()
    {
        Main.Instance.Init(Main.Scene.LobbyScene);
        var popupManager = PopupManager.Instance;
    }



    void OnClickStartStage()
    {
        // Addressable 주소는 프로젝트 설정에 따라 다를 수 있습니다. 
        // 여기서는 파일 이름(확장자 포함)으로 가정합니다.
        string rulePath = "GameRule.json";
        string stagePath = "Stage.json";

        StageInjection.Instance.MakeGameSpec(rulePath, stagePath);

        if (StageInjection.Instance.GetGameSpec() != null)
        {
            Main.Instance.MoveScene(Main.Scene.GameScene);
        }
        else
        {
            Debug.LogError("Failed to prepare GameSpec before moving to GameScene.");
        }
    }

}
