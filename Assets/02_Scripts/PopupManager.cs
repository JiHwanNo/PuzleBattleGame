using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupManager : MonoBehaviour
{
    static PopupManager instance;
    public const string PopupSceneName = "PopupScene";

    public static PopupManager Instance
    {
        get
        {
            if (instance == null)
            {

                if (instance == null)
                {
                    var scene = SceneManager.GetSceneByName(PopupSceneName);
                    if (!scene.isLoaded)
                        SceneManager.LoadScene(PopupSceneName, LoadSceneMode.Additive);

                    instance = FindFirstObjectByType<PopupManager>();

                    if (instance == null)
                    {
                        var go = new GameObject("PopupManager");
                        instance = go.AddComponent<PopupManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return instance;
        }
    }

}
