using UnityEngine;
using Puzzle.Core;

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

    private GameSpec _gameSpec;
    public GameSpec GetGameSpec() => _gameSpec;

    public void MakeGameSpec(string ruleAddress, string stageAddress)
    {
        _gameSpec = new GameSpec();

        // 1. Rule 데이터 로드
        TextAsset ruleAsset = AssetManager.Instance.LoadAsset<TextAsset>(ruleAddress);
        if (ruleAsset != null)
        {
            GameRuleContainer ruleContainer = JsonUtility.FromJson<GameRuleContainer>(ruleAsset.text);
            _gameSpec.rules = ruleContainer.rules;
            _gameSpec.blocks = ruleContainer.blocks;
        }
        else
        {
            Debug.LogError($"Failed to load Rule Asset: {ruleAddress}");
        }

        // 2. Stage 데이터 로드
        TextAsset stageAsset = AssetManager.Instance.LoadAsset<TextAsset>(stageAddress);
        if (stageAsset != null)
        {
            _gameSpec.stageData = JsonUtility.FromJson<StageData>(stageAsset.text);
        }
        else
        {
            Debug.LogError($"Failed to load Stage Asset: {stageAddress}");
        }
    }
}

