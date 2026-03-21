using Puzzle.Core;
using UnityEngine;

/// <summary>
/// JSON 데이터를 로드하여 게임 실행에 필요한 데이터(GameSpec)를 주입하는 클래스입니다.
/// 게임 씬 진입 전 규칙과 스테이지 정보를 병합하여 저장합니다.
/// </summary>
public class StageInjection : MonoBehaviour
{
    #region Singleton
    private static StageInjection _instance;

    /// <summary> 전역 접근을 위한 싱글톤 인스턴스 </summary>
    public static StageInjection Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("StageInjection");
                _instance = obj.AddComponent<StageInjection>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }
    #endregion

    /// <summary> JSON 파일로부터 로드된 규칙 정보 (주로 에디터 설정용) </summary>
    public TextAsset ruleJson;
    /// <summary> JSON 파일로부터 로드된 스테이지 정보 (주로 에디터 설정용) </summary>
    public TextAsset stageJson;

    /// <summary> 최종 병합된 게임 사양서 </summary>
    private GameSpec _gameSpec;

    /// <summary>
    /// 최종 병합된 게임 사양서를 반환합니다.
    /// </summary>
    /// <returns>모델 레이어에 전달할 게임 사양서</returns>
    public GameSpec GetGameSpec()
    {
        return _gameSpec;
    }

    /// <summary>
    /// 설정된 JSON 파일들로부터 데이터를 읽어 GameSpec 객체를 생성합니다.
    /// </summary>
    /// <returns>성공 여부</returns>
    public bool InjectStageData()
    {
        // 1. 규칙 파일 로드 (Rule)
        if (ruleJson == null)
        {
            ruleJson = AssetManager.Instance.LoadAsset<TextAsset>("GameRule");
        }

        // 2. 스테이지 파일 로드 (Stage)
        if (stageJson == null)
        {
            stageJson = AssetManager.Instance.LoadAsset<TextAsset>("Stage");
        }

        if (ruleJson == null || stageJson == null)
        {
            Debug.LogError("[StageInjection] JSON 데이터를 찾을 수 없습니다.");
            return false;
        }

        try
        {
            // 3. GameSpec 생성 및 병합
            _gameSpec = new GameSpec();
            _gameSpec.rule = JsonUtility.FromJson<GameRule>(ruleJson.text);
            _gameSpec.stageData = JsonUtility.FromJson<StageData>(stageJson.text);

            if (_gameSpec.rule == null || _gameSpec.stageData == null)
            {
                Debug.LogError("[StageInjection] JSON 파싱 중 오류가 발생했습니다.");
                return false;
            }

            Debug.Log("[StageInjection] 스테이지 데이터 주입 성공.");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StageInjection] 주입 도중 예외 발생: {ex.Message}");
            return false;
        }
    }
}
