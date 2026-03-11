using UnityEngine;

public class LobbyMain : MonoBehaviour
{
    void Start()
    {
        var popupManager = PopupManager.Instance;
    }



    void OnClickStartStage()
    {
        Main.Instance.MoveScene(Main.Scene.GameScene);
    }

}
