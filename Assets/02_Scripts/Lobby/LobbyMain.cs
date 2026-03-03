using UnityEngine;

public class LobbyMain : MonoBehaviour
{
    void Start()
    {
        var popupManager = PopupManager.Instance;
    }



    void OnClickStartStage()
    {
        Debug.Log("On Click Event StartStage");
    }

}
