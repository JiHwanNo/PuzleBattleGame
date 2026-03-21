using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 팝업 윈도우의 생성과 관리를 담당하는 매니저 클래스입니다.
/// </summary>
public class PopupManager : MonoBehaviour
{
    #region Singleton
    private static PopupManager _instance;

    /// <summary> 전역 접근을 위한 싱글톤 인스턴스 </summary>
    public static PopupManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("PopupManager");
                _instance = obj.AddComponent<PopupManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }
    #endregion

    /// <summary> 현재 화면에 띄워진 팝업들의 스택 </summary>
    private Stack<GameObject> _popupStack = new Stack<GameObject>();

    /// <summary>
    /// 지정된 프리팹 주소를 사용하여 팝업을 화면에 띄웁니다.
    /// </summary>
    /// <param name="address">팝업 프리팹의 Addressables 주소</param>
    public void Show(string address)
    {
        AssetManager.AssetArguments<GameObject> args = new AssetManager.AssetArguments<GameObject>
        {
            address = address,
            successCallback = (popup) =>
            {
                if (popup != null)
                {
                    _popupStack.Push(popup);
                }
            }
        };

        AssetManager.Instance.LoadGameObjectAsync(args, this.transform);
    }

    /// <summary>
    /// 가장 최근에 띄워진 팝업을 닫습니다.
    /// </summary>
    public void Close()
    {
        if (_popupStack.Count > 0)
        {
            GameObject popup = _popupStack.Pop();
            if (popup != null)
            {
                Destroy(popup);
            }
        }
    }

    /// <summary>
    /// 화면에 열려 있는 모든 팝업을 닫습니다.
    /// </summary>
    public void CloseAll()
    {
        while (_popupStack.Count > 0)
        {
            Close();
        }
    }
}
