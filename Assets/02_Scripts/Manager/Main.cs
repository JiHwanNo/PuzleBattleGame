using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 전체적인 흐름과 씬 전환을 관리하는 메인 시스템 클래스입니다.
/// </summary>
public class Main
{
    #region Singleton
    private static Main _instance;
    public static Main Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Main();
            return _instance;
        }
    }
    #endregion

    /// <summary> 현재 활성화된 씬 정보 </summary>
    private Scene _curScene = Scene.None;
    
    /// <summary>
    /// 메인 시스템을 초기화하고 시작 씬을 설정합니다.
    /// </summary>
    /// <param name="startScene">초기화 시 설정할 시작 씬</param>
    internal void Init(Scene startScene)
    {
        _curScene = startScene;
    }

    /// <summary>
    /// 지정된 씬으로 이동하며, 이전 씬은 언로드합니다.
    /// </summary>
    /// <param name="loadScene">로드할 대상 씬</param>
    internal void MoveScene(Scene loadScene)
    {
        Scene preScene = _curScene;
        if(preScene != Scene.None)
            SceneManager.UnloadSceneAsync(preScene.ToString());

        SceneManager.LoadSceneAsync(loadScene.ToString(), LoadSceneMode.Additive);
        _curScene = loadScene;
    }

    /// <summary>
    /// 게임 내에서 사용하는 씬의 종류를 정의합니다.
    /// </summary>
    public enum Scene
    {
        None,
        TitleScene,
        LoadingScene,
        LobbyScene,
        GameScene,
    }
}
