using System.IO;
using Puzzle.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 준비(Ready) 팝업.
/// PopupHandler를 상속받아 시작/닫기/리플레이 버튼 로직을 구현합니다.
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
    /// 리플레이 테스트 버튼 클릭 시 가장 최근 리플레이를 로드하고 게임을 시작합니다.
    /// 유저는 정상 플레이하며, 상대방 보드에 리플레이가 재생됩니다.
    /// </summary>
    private void OnClickReplay()
    {
        // 가장 최근 리플레이 파일 탐색
        string replayDir = ReplayStorage.GetReplayDirectoryPath();
        if (!Directory.Exists(replayDir))
        {
            Debug.LogError("리플레이 디렉터리가 존재하지 않습니다.");
            return;
        }

        string[] files = Directory.GetFiles(replayDir, "replay_*.json");
        if (files.Length == 0)
        {
            Debug.LogError("저장된 리플레이 파일이 없습니다.");
            return;
        }

        // 파일명 기준 정렬하여 가장 최근 파일 선택
        System.Array.Sort(files);
        string latestFile = files[files.Length - 1];

        ReplayData replayData = ReplayStorage.Load(latestFile);
        if (replayData == null)
        {
            Debug.LogError("리플레이 데이터 로드에 실패했습니다.");
            return;
        }

        // 유저 게임 데이터 준비 (리플레이와 동일한 규칙/스테이지 사용)
        StageInjection.Instance.MakeGameSpec(replayData.ruleAddress, replayData.stageAddress);

        GameSpec spec = StageInjection.Instance.GetGameSpec();
        if (spec == null || string.IsNullOrEmpty(spec.rule.ruleId) || spec.stageData == null)
        {
            Debug.LogError("게임 씬으로 이동하기 전 GameSpec 준비에 실패했습니다.");
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
}
