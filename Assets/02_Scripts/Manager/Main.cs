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
    /// <summary>
    /// 게임 시작 시 SharedScene을 자동 로드합니다.
    /// AfterSceneLoad 시점이므로 SharedScene에서 직접 플레이 시 Main이 이미 생성되어 중복 로드를 방지합니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        // SharedScene에서 직접 플레이한 경우 Main.Awake가 이미 실행됨
        if (_instance != null)
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
        SceneManager.sceneLoaded += OnSharedSceneLoaded;
        RemoveDuplicateEventSystems();
    }


    /// <summary>
    /// SharedScene 로드 완료 시 Active Scene으로 설정합니다.
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    /// <param name="mode">씬 로드 모드</param>
    private void OnSharedSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SHARED_SCENE_NAME)
        {
            SceneManager.SetActiveScene(scene);
            SceneManager.sceneLoaded -= OnSharedSceneLoaded;
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
        // 중복 EventSystem 제거 (SharedScene 또는 DontDestroyOnLoad의 것은 유지)
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length > 1)
        {
            for (int i = 0; i < eventSystems.Length; i++)
            {
                string sceneName = eventSystems[i].gameObject.scene.name;
                if (sceneName == SHARED_SCENE_NAME || sceneName == "DontDestroyOnLoad")
                {
                    continue;
                }
                Destroy(eventSystems[i].gameObject);
            }
        }

        // 중복 AudioListener 제거 (SharedScene 또는 DontDestroyOnLoad의 것은 유지)
        AudioListener[] audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (audioListeners.Length > 1)
        {
            for (int i = 0; i < audioListeners.Length; i++)
            {
                string sceneName = audioListeners[i].gameObject.scene.name;
                if (sceneName == SHARED_SCENE_NAME || sceneName == "DontDestroyOnLoad")
                {
                    continue;
                }
                Destroy(audioListeners[i].gameObject);
            }
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

        // 4. 다음 씬 활성화
        nextOp.allowSceneActivation = true;
        yield return nextOp;

        // 5. 로딩 씬 언로드
        AsyncOperation unloadLoadingOp = SceneManager.UnloadSceneAsync(SceneEnum.LoadingScene.ToString());
        yield return unloadLoadingOp;

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