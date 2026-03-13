using UnityEngine;
using Puzzle.Core;

/// <summary>
/// 게임 시작 시 필요한 스테이지 데이터와 규칙 정보를 관리하고 주입하는 클래스입니다.
/// 로비에서 결정된 스테이지 사양을 실제 게임 엔진(Model)에 전달하는 역할을 합니다.
/// </summary>
public class StageInjection
{
    #region Singleton
    private static StageInjection _instance;
    public static StageInjection Instance
    {
        get
        {
            if (_instance == null)
                _instance = new StageInjection();

            return _instance;
        }
    }
    #endregion

    /// <summary> 현재 구성된 게임 전체 사양서 </summary>
    private GameSpec _gameSpec;

    /// <summary>
    /// 현재 보관 중인 게임 사양서 객체를 반환합니다.
    /// </summary>
    /// <returns>구성된 GameSpec 객체</returns>
    public GameSpec GetGameSpec() => _gameSpec;

    /// <summary>
    /// 지정된 규칙과 스테이지 에셋 주소로부터 데이터를 로드하여 게임 사양서(GameSpec)를 완성합니다.
    /// </summary>
    /// <param name="ruleAddress">Addressable 내 규칙 JSON 에셋 주소</param>
    /// <param name="stageAddress">Addressable 내 스테이지 JSON 에셋 주소</param>
    public void MakeGameSpec(string ruleAddress, string stageAddress)
    {
        _gameSpec = new GameSpec();

        // 1. 규칙(Rule) 데이터 로드 및 파싱
        TextAsset ruleAsset = AssetManager.Instance.LoadAsset<TextAsset>(ruleAddress);
        if (ruleAsset != null)
        {
            GameRuleContainer ruleContainer = JsonUtility.FromJson<GameRuleContainer>(ruleAsset.text);
            _gameSpec.rules = ruleContainer.rules;
            _gameSpec.blocks = ruleContainer.blocks;
        }
        else
        {
            Debug.LogError($"규칙 에셋 로드 실패: {ruleAddress}");
        }

        // 2. 스테이지(Stage) 데이터 로드 및 파싱
        TextAsset stageAsset = AssetManager.Instance.LoadAsset<TextAsset>(stageAddress);
        if (stageAsset != null)
        {
            _gameSpec.stageData = JsonUtility.FromJson<StageData>(stageAsset.text);
        }
        else
        {
            Debug.LogError($"스테이지 에셋 로드 실패: {stageAddress}");
        }
    }
}
