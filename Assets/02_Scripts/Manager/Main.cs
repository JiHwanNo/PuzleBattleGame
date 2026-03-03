using UnityEngine;
using UnityEngine.SceneManagement;

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

    Scene curScene = Scene.None;
    

    internal void MoveScene(Scene loadScene)
    {
        Scene preScene = curScene;
        if(preScene != Scene.None)
            SceneManager.UnloadSceneAsync(preScene.ToString());

        SceneManager.LoadSceneAsync(loadScene.ToString(), LoadSceneMode.Additive);
        curScene = loadScene;
    }


    public enum Scene
    {
        None,
        TitleScene,
        LoadingScene,
        LobbyScene,
        GameScene,
    }
}
