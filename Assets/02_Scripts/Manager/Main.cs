using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 전체적인 흐름과 씬 전환을 관리하는 메인 시스템 클래스입니다.
/// </summary>
public class Main : MonoBehaviour
{
    #region Singleton
    private static Main _instance;

    /// <summary> 전역 접근을 위한 싱글톤 인스턴스 </summary>
    public static Main Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion

    /// <summary> SharedScene 이름 상수 </summary>
    private const string SHARED_SCENE_NAME = "SharedScene";

    /// <summary> 현재 활성화된 씬 정보 </summary>
    private SceneEnum _curScene = SceneEnum.TitleScene;

    /// <summary> 씬 전환 중 여부 </summary>
    private bool _isMovingScene;

    /// <summary>
    /// 게임 시작 시 SharedScene을 자동 로드합니다. SharedScene 내 Main 컴포넌트가 함께 생성됩니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (SceneManager.GetSceneByName(SHARED_SCENE_NAME).isLoaded)
        {
            return;
        }

        SceneManager.LoadScene(SHARED_SCENE_NAME, LoadSceneMode.Additive);
    }

    /// <summary>
    /// 싱글톤 인스턴스 등록 및 중복 방지, Active Scene 설정
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.SetActiveScene(gameObject.scene);
        RemoveDuplicateEventSystems();
        ForceStartFromTitle();
    }

    /// <summary>
    /// 에디터에서 TitleScene이 아닌 씬으로 플레이 시 TitleScene으로 강제 이동합니다.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void ForceStartFromTitle()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != SHARED_SCENE_NAME && activeSceneName != SceneEnum.TitleScene.ToString())
        {
            SceneManager.LoadScene(SceneEnum.TitleScene.ToString());
            _curScene = SceneEnum.TitleScene;
        }
    }

    /// <summary>
    /// 씬 로드 시 중복 EventSystem을 제거합니다.
    /// </summary>
    private void RemoveDuplicateEventSystems()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 로드될 때마다 중복 EventSystem을 찾아 제거합니다.
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    /// <param name="mode">씬 로드 모드</param>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length <= 1)
        {
            return;
        }

        // SharedScene의 EventSystem을 유지하고 나머지 제거
        for (int i = 0; i < eventSystems.Length; i++)
        {
            if (eventSystems[i].gameObject.scene == gameObject.scene)
            {
                continue;
            }
            Destroy(eventSystems[i].gameObject);
        }
    }

    /// <summary>
    /// 지정된 씬으로 이동하며, 이전 씬은 언로드합니다.
    /// </summary>
    /// <param name="loadScene">로드할 대상 씬</param>
    internal void MoveScene(SceneEnum preScene, SceneEnum nextScene)
    {
        if (_isMovingScene)
        {
            return;
        }

        StartCoroutine(CoMoveScene(preScene, nextScene));
    }

    /// <summary>
    /// 이전 씬을 언로드 완료한 후 다음 씬을 로드하는 코루틴
    /// </summary>
    /// <param name="pre">언로드할 이전 씬</param>
    /// <param name="next">로드할 다음 씬</param>
    private IEnumerator CoMoveScene(SceneEnum pre, SceneEnum next)
    {
        _isMovingScene = true;

        // 1. 로딩 씬 로드
        AsyncOperation loadingOp = SceneManager.LoadSceneAsync(SceneEnum.LoadingScene.ToString(), LoadSceneMode.Additive);
        yield return loadingOp;

        // 2. 이전 씬 언로드
        if (pre != SceneEnum.None)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(pre.ToString());
            yield return unloadOp;
        }

        // 3. 다음 씬 비동기 로드 (활성화 대기)
        AsyncOperation nextOp = SceneManager.LoadSceneAsync(next.ToString(), LoadSceneMode.Additive);
        nextOp.allowSceneActivation = false;

        while (nextOp.progress < 0.9f)
        {
            yield return null;
        }

        // 4. 로딩 씬 언로드
        AsyncOperation unloadLoadingOp = SceneManager.UnloadSceneAsync(SceneEnum.LoadingScene.ToString());
        yield return unloadLoadingOp;

        // 5. 다음 씬 활성화
        nextOp.allowSceneActivation = true;
        yield return nextOp;

        _curScene = next;
        _isMovingScene = false;
    }

}

/// <summary>
/// 게임 내에서 사용하는 씬의 종류를 정의합니다.
/// </summary>
public enum SceneEnum
{
    None,
    TitleScene,
    LoadingScene,
    LobbyScene,
    GameScene,
}