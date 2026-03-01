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
    public void MakeGameSpec(string ruleName, string stageName)
    {
        _gameSpec = new GameSpec();

    }
}

