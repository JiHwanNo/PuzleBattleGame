using Puzzle.Core;
using UnityEngine;

/// <summary>
/// 게임 준비(Ready) 팝업.
/// PopupHandler를 상속받아 시작/닫기/리플레이 버튼 로직을 구현합니다.
/// </summary>
public class PopupReady : PopupHandler
{
    private const string StagePath = "Stage";

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 퍼즐 모드 버튼 클릭 시 지정된 퍼즐로 게임 데이터를 준비하고 게임 씬으로 이동합니다.
    /// </summary>
    private void OnClickStartButton(string val)
    {
        if (!TryParsePuzzleType(val, out PuzzleType puzzleType))
        {
            Debug.LogError($"[PopupReady] 알 수 없는 퍼즐 타입입니다: {val}");
            return;
        }

        string ruleAddress = GetRuleAddress(puzzleType);
        if (!StageInjection.Instance.MakeGameSpec(ruleAddress, StagePath))
        {
            Debug.LogError("[PopupReady] GameSpec 준비에 실패했습니다.");
            return;
        }

        StageInjection.Instance.SetReplayData(null);
        Debug.Log($"[PopupReady] 선택된 퍼즐 모드: {ruleAddress}");
        Main.Instance.MoveScene(SceneEnum.LobbyScene, SceneEnum.GameScene);
    }

    /// <summary>
    /// 퍼즐 모드별 리플레이 버튼 클릭 시 해당 타입의 가장 최근 리플레이를 실행합니다.
    /// </summary>
    private void OnClickReplayButton(string val)
    {
        if (!TryParsePuzzleType(val, out PuzzleType puzzleType))
        {
            Debug.LogError($"[PopupReady] 알 수 없는 퍼즐 타입입니다: {val}");
            return;
        }

        ReplayData replayData = ReplayStorage.LoadLatest(puzzleType);
        if (replayData == null)
        {
            Debug.LogError("리플레이 데이터가 없습니다!");
            return;
        }

        // 유저 게임 데이터 준비 (리플레이와 동일한 규칙/스테이지 사용)
        if (!StageInjection.Instance.MakeGameSpec(replayData.ruleAddress, replayData.stageAddress))
        {
            Debug.LogError("[PopupReady] 리플레이용 GameSpec 준비에 실패했습니다.");
            return;
        }

        // 상대방 리플레이 데이터 세팅
        StageInjection.Instance.SetReplayData(replayData);
        Main.Instance.MoveScene(SceneEnum.LobbyScene, SceneEnum.GameScene);
    }

    /// <summary>
    /// 닫기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnClickClose()
    {
        ClosePopup();
    }

    private bool TryParsePuzzleType(string val, out PuzzleType puzzleType)
    {
        if (!int.TryParse(val, out int puzzleTypeValue))
        {
            puzzleType = PuzzleType.None;
            return false;
        }

        puzzleType = (PuzzleType)puzzleTypeValue;
        return puzzleType == PuzzleType.ThreeMatch ||
               puzzleType == PuzzleType.Link ||
               puzzleType == PuzzleType.TapMatch;
    }

    private string GetRuleAddress(PuzzleType puzzleType)
    {
        switch (puzzleType)
        {
            case PuzzleType.ThreeMatch:
                return "ThreeMatchRule";
            case PuzzleType.Link:
                return "LinkMatchRule";
            case PuzzleType.TapMatch:
                return "TapMatchRule";
            default:
                return string.Empty;
        }
    }

}
