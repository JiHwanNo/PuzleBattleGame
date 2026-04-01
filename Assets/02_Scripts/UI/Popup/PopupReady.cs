using Puzzle.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 준비(Ready) 팝업.
/// PopupHandler를 상속받아 시작/닫기 버튼 로직을 구현합니다.
/// </summary>
public class PopupReady : PopupHandler
{

    /// <summary>
    /// 시작 버튼 클릭 시 게임 데이터를 준비하고 게임 씬으로 이동합니다.
    /// </summary>
    private void OnClickStart()
    {
        string rulePath = "LinkMatchRule";
        string stagePath = "Stage";

        StageInjection.Instance.MakeGameSpec(rulePath, stagePath);

        GameSpec spec = StageInjection.Instance.GetGameSpec();
        if (spec != null &&
                string.IsNullOrEmpty(spec.rule.ruleId) == false &&
                    spec.stageData != null)
        {
            Main.Instance.MoveScene(SceneEnum.LobbyScene, SceneEnum.GameScene);
        }
        else
        {
            Debug.LogError("게임 씬으로 이동하기 전 GameSpec 준비에 실패했습니다.");
        }
    }

    /// <summary>
    /// 닫기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnClickClose()
    {
        ClosePopup();   
    }
}
