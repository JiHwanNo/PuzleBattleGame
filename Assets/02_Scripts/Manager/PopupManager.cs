using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 게임 내 팝업 시스템을 중앙 관리하는 싱글톤 매니저.
/// 팝업 데이터(스택, 도메인 경로)를 관리하고, 실제 생성/제거는 각 씬의 PopupController에 위임합니다.
/// </summary>
public class PopupManager : MonoBehaviour
{
    private static PopupManager _instance;

    /// <summary> 전역 접근을 위한 싱글톤 인스턴스 </summary>
    public static PopupManager Instance
    {
        get
        {
            return _instance;
        }
    }

    /// <summary> 열린 팝업을 순서대로 추적하는 스택 (도메인 경로) </summary>
    private readonly List<PopupBase> _popupStack = new List<PopupBase>();

    /// <summary> 등록된 팝업 컨트롤러 목록 (씬 이름 → 컨트롤러) </summary>
    private readonly Dictionary<string, PopupController> _controllers = new Dictionary<string, PopupController>();

    /// <summary> 현재 활성화된 팝업 컨트롤러 </summary>
    private PopupController _activeController;

    /// <summary> 현재 팝업이 열려있는지 여부 </summary>
    public bool HasPopup
    {
        get
        {
            return _popupStack.Count > 0;
        }
    }

    /// <summary> 현재 열린 팝업 개수 </summary>
    public int PopupCount
    {
        get
        {
            return _popupStack.Count;
        }
    }

    /// <summary>
    /// 현재 팝업 경로를 도메인 형태로 반환합니다.
    /// 예: "/Lobby/StageSelect/Confirm"
    /// </summary>
    public string CurrentPath
    {
        get
        {
            if (_popupStack.Count == 0)
            {
                return "/";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _popupStack.Count; i++)
            {
                sb.Append("/");
                sb.Append(_popupStack[i].PopupName);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 싱글톤 인스턴스 등록 및 중복 방지
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
    }

    #region 팝업 Open / Close / Back

    /// <summary>
    /// 팝업을 이름으로 엽니다. 활성 컨트롤러를 통해 프리팹을 생성합니다.
    /// </summary>
    /// <param name="popupName">팝업 이름 (Addressable 주소: "Popup/{popupName}")</param>
    /// <param name="onOpened">팝업 열림 완료 시 콜백</param>
    public void Open(string popupName, Action<PopupBase> onOpened = null)
    {
        if (_activeController == null)
        {
            Debug.LogError("[PopupManager] 활성화된 PopupController가 없습니다.");
            return;
        }

        _activeController.CreatePopup(popupName, (popup) =>
        {
            _popupStack.Add(popup);
            popup.Open();

            Debug.Log($"[PopupManager] 팝업 열림: {popupName} | 경로: {CurrentPath}");
            onOpened?.Invoke(popup);
        });
    }

    /// <summary>
    /// 특정 이름의 팝업을 닫습니다. 해당 팝업 위에 쌓인 모든 팝업도 함께 닫힙니다.
    /// 도메인 경로에서 해당 경로 이하를 모두 제거하는 것과 같습니다.
    /// </summary>
    /// <param name="popupName">닫을 팝업 이름</param>
    public void Close(string popupName)
    {
        int index = FindPopupIndex(popupName);
        if (index < 0)
        {
            Debug.LogWarning($"[PopupManager] 닫을 팝업을 찾을 수 없습니다: {popupName}");
            return;
        }

        for (int i = _popupStack.Count - 1; i >= index; i--)
        {
            RemovePopupAt(i);
        }

        Debug.Log($"[PopupManager] 팝업 닫힘: {popupName} | 경로: {CurrentPath}");
    }

    /// <summary>
    /// 가장 위에 있는 팝업을 닫고 이전 팝업으로 돌아갑니다.
    /// </summary>
    public void Back()
    {
        if (_popupStack.Count == 0)
        {
            Debug.LogWarning("[PopupManager] 닫을 팝업이 없습니다.");
            return;
        }

        string closedName = _popupStack[_popupStack.Count - 1].PopupName;
        RemovePopupAt(_popupStack.Count - 1);

        Debug.Log($"[PopupManager] Back: {closedName} 닫힘 | 경로: {CurrentPath}");
    }

    /// <summary>
    /// 열려있는 모든 팝업을 닫습니다.
    /// </summary>
    public void CloseAll()
    {
        for (int i = _popupStack.Count - 1; i >= 0; i--)
        {
            RemovePopupAt(i);
        }

        Debug.Log("[PopupManager] 모든 팝업 닫힘");
    }

    /// <summary>
    /// 현재 최상위 팝업을 반환합니다.
    /// </summary>
    /// <returns>최상위 팝업, 없으면 null</returns>
    public PopupBase Peek()
    {
        if (_popupStack.Count == 0)
        {
            return null;
        }
        return _popupStack[_popupStack.Count - 1];
    }

    /// <summary>
    /// 특정 이름의 팝업이 현재 열려있는지 확인합니다.
    /// </summary>
    /// <param name="popupName">확인할 팝업 이름</param>
    /// <returns>열려있으면 true</returns>
    public bool IsOpen(string popupName)
    {
        return FindPopupIndex(popupName) >= 0;
    }

    /// <summary>
    /// 특정 이름의 열린 팝업 인스턴스를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">PopupBase를 상속한 타입</typeparam>
    /// <param name="popupName">팝업 이름</param>
    /// <returns>해당 팝업 인스턴스, 없으면 null</returns>
    public T GetPopup<T>(string popupName) where T : PopupBase
    {
        int index = FindPopupIndex(popupName);
        if (index >= 0)
        {
            return _popupStack[index] as T;
        }
        return null;
    }

    /// <summary>
    /// 스택에서 특정 이름의 팝업 인덱스를 찾습니다.
    /// </summary>
    /// <param name="popupName">찾을 팝업 이름</param>
    /// <returns>인덱스, 없으면 -1</returns>
    private int FindPopupIndex(string popupName)
    {
        for (int i = 0; i < _popupStack.Count; i++)
        {
            if (_popupStack[i].PopupName == popupName)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 스택에서 팝업을 제거하고 활성 컨트롤러를 통해 닫힘 처리를 위임합니다.
    /// </summary>
    /// <param name="index">스택 인덱스</param>
    private void RemovePopupAt(int index)
    {
        PopupBase popup = _popupStack[index];
        _popupStack.RemoveAt(index);

        if (popup != null && _activeController != null)
        {
            _activeController.DestroyPopup(popup);
        }
    }

    #endregion

    #region PopupController 등록/조회

    /// <summary>
    /// 팝업 컨트롤러를 등록합니다. 등록 시 활성 컨트롤러로 설정됩니다.
    /// </summary>
    /// <param name="controller">등록할 팝업 컨트롤러</param>
    public void Register(PopupController controller)
    {
        string key = controller.ControllerName;
        if (!_controllers.ContainsKey(key))
        {
            _controllers.Add(key, controller);
        }
        _activeController = controller;

        Debug.Log($"[PopupManager] 컨트롤러 등록 및 활성화: {key}");
    }

    /// <summary>
    /// 팝업 컨트롤러 등록을 해제합니다. 활성 컨트롤러였다면 해제됩니다.
    /// </summary>
    /// <param name="controller">해제할 팝업 컨트롤러</param>
    public void Unregister(PopupController controller)
    {
        _controllers.Remove(controller.ControllerName);

        if (_activeController == controller)
        {
            _activeController = null;
        }

        Debug.Log($"[PopupManager] 컨트롤러 해제: {controller.ControllerName}");
    }

    /// <summary>
    /// 이름으로 팝업 컨트롤러를 가져옵니다.
    /// </summary>
    /// <param name="controllerName">컨트롤러 이름</param>
    /// <returns>해당 팝업 컨트롤러, 없으면 null</returns>
    public PopupController GetController(string controllerName)
    {
        _controllers.TryGetValue(controllerName, out PopupController controller);
        return controller;
    }

    /// <summary>
    /// 제네릭으로 특정 타입의 팝업 컨트롤러를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">PopupController를 상속한 타입</typeparam>
    /// <returns>해당 타입의 컨트롤러, 없으면 null</returns>
    public T GetController<T>() where T : PopupController
    {
        foreach (PopupController controller in _controllers.Values)
        {
            if (controller is T typed)
            {
                return typed;
            }
        }
        return default;
    }

    #endregion
}
