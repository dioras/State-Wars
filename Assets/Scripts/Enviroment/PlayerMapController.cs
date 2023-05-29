using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private GroupingStorage groupingStorageSO = default;
    [SerializeField] private StateStorage stateStorageSO = default;
    [SerializeField] private PlayerStorage playerStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;

    [Header("Settings")]
    [SerializeField] private Color closedColorState = default;
    [SerializeField] private Color captureColorState = default;
    [SerializeField] private Color enemysColorState = default;
    [SerializeField] private Sprite captureSprite = default;
    [SerializeField] private Vector3 cameraOffset = default;

    [Header("Components")]
    [SerializeField] private Transform cameraViewPoint = default;
    [SerializeField] private Transform mapContainer = default;

    [Header("States")]
    [SerializeField] private List<StateObject> availableStates = new List<StateObject>();
    [SerializeField] private List<MeshRenderer> closedStates = new List<MeshRenderer>();

    [Header("Actions")]
    public static Action<bool> ShowPlayerMapAction = default;

    private void OnEnable()
    {
        ShowPlayerMapAction += ShowPlayerMap;
    }

    private void OnDisable()
    {
        ShowPlayerMapAction -= ShowPlayerMap;
    }

    private void ShowPlayerMap(bool _show)
    {
        if (_show)
        {
            Transform currenctState = default;
            closedStates.ForEach((_state) => _state.material.SetColor("_Color", closedColorState));
            availableStates.ForEach((_state) => {
                if (playerStorageSO.ConcretePlayer.CurrecyStateType != _state.StateType)
                {
                    var _grouping = groupingStorageSO.GetGrouping(stateStorageSO.Country.GetState(_state.StateType).Grouping);
                    if (_grouping.Type == GroupingType.Union)
                    {
                        _state.Config(_grouping.Color, _grouping.Sprite, _grouping.UseColorToIcon);
                    }
                    else
                    {
                        _state.Config(enemysColorState, _grouping.Sprite, _grouping.UseColorToIcon);
                    }
                    _state.Pulse(false);
                }
                else
                {
                    _state.Config(captureColorState, captureSprite, Color.white);
                    _state.Pulse(true);
                    currenctState = _state.transform;
                }
            });

            CameraController.SetOrthographicAction?.Invoke(true);
            dependencyContainerSO.CameraViewStatePoint = cameraViewPoint;
            dependencyContainerSO.CurrencyStatePlayerMap = new Vector3(currenctState.position.x + cameraOffset.x, cameraViewPoint.position.y + cameraOffset.y, currenctState.position.z + cameraOffset.z);
            CameraController.SetCameraPositionAction?.Invoke(/*cameraViewPoint.position*/new Vector3(currenctState.position.x + cameraOffset.x, cameraViewPoint.position.y + cameraOffset.y, currenctState.position.z + cameraOffset.z), cameraViewPoint.eulerAngles);
        }
        mapContainer.gameObject.SetActive(_show);
    }
}
