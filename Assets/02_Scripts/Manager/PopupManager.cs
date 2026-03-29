using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 팝업 시스템을 중앙 관리하는 싱글톤 매니저.
/// 각 씬의 PopupController를 등록받아 필요한 컨트롤러에 명령을 전달합니다.
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

    /// <summary> 등록된 팝업 컨트롤러 목록 </summary>
    private readonly Dictionary<string, PopupController> _controllers = new Dictionary<string, PopupController>();

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

    /// <summary>
    /// 팝업 컨트롤러를 등록합니다.
    /// </summary>
    /// <param name="controller">등록할 팝업 컨트롤러</param>
    public void Register(PopupController controller)
    {
        string key = controller.ControllerName;
        if (!_controllers.ContainsKey(key))
        {
            _controllers.Add(key, controller);
        }
    }

    /// <summary>
    /// 팝업 컨트롤러 등록을 해제합니다.
    /// </summary>
    /// <param name="controller">해제할 팝업 컨트롤러</param>
    public void Unregister(PopupController controller)
    {
        _controllers.Remove(controller.ControllerName);
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
}
