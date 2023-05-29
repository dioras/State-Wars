using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static List<IInterfacePanel> InterfacePanels { get; set; } = new List<IInterfacePanel>();
    public static Action<UIPanelType> ShowUIPanelAloneAction = default;
    public static Action<UIPanelType> ShowUIPanelAlongAction = default;

    private void OnEnable()
    {
        ShowUIPanelAloneAction += ShowPanelAlone;
        ShowUIPanelAlongAction += ShowPanelAlong;
    }

    private void OnDisable()
    {
        ShowUIPanelAloneAction -= ShowPanelAlone;
        ShowUIPanelAlongAction -= ShowPanelAlong;
    }

    private void ShowPanelAlone(UIPanelType _uIPanelType)
    {
        InterfacePanels.Find(somePanel => somePanel.UIPanelType == _uIPanelType)?.Show();
        var temp = InterfacePanels.FindAll(somePanel => somePanel.UIPanelType != _uIPanelType);
        foreach (var item in temp)
        {
            item.Hide();
        }
    }

    private void ShowPanelAlong(UIPanelType _uIPanelType)
    {
        InterfacePanels.Find(somePanel => somePanel.UIPanelType == _uIPanelType)?.Show();
    }
}
