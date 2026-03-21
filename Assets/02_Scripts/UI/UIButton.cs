using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [SerializeField] MonoBehaviour root;
    [SerializeField] Button unityButton;

    [SerializeField] string callbackName;
    [SerializeField] string callbackValue;
    public void OnClickEvent()
    {
        if (unityButton == null || string.IsNullOrEmpty(callbackName))
        {
            Debug.LogError($"UIButton_{gameObject.name}: Unity Button or Callback Name is not set.");
            return;
        }

        if (string.IsNullOrEmpty(callbackValue))
            root.SendMessage(callbackName, callbackValue);
        else
            root.SendMessage(callbackName);
    }
}
