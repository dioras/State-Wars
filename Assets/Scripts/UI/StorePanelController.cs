using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class StorePanelController : MonoBehaviour, IInterfacePanel
{
    [Header("Settings")]
    [SerializeField] private UIPanelType uiPanelType = UIPanelType.Store;
    [SerializeField] private float durationShowHide = .5f;

    [Header("Components")]
    [SerializeField] private RectTransform panelContainer = default;
    [SerializeField] private RectTransform boardContainer = default;
    [SerializeField] private Button buttonBack = default;

    private RectTransform buttonBackRect = default;
    private Vector2 defaultBoardsPosition = default;
    private Vector2 defaultButtonBackPosition = default;

    private void Awake()
    {
        Init();
        PrepareButtons();
        SetDefaultsParams();
    }

    private void SetDefaultsParams()
    {
        buttonBackRect = buttonBack.GetComponent<RectTransform>();
        defaultBoardsPosition = new Vector2(0f, 400f);
        defaultButtonBackPosition = buttonBackRect.anchoredPosition;
    }

    private void PrepareButtons()
    {
        buttonBack.onClick.AddListener(() => {
            SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
            UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.Main);
        });
    }

    #region IInterfacePanel
    public UIPanelType UIPanelType { get => uiPanelType; }

    public void Hide()
    {
        buttonBackRect.DOAnchorPos(-defaultButtonBackPosition, durationShowHide);
        boardContainer.DOAnchorPos(-defaultBoardsPosition, durationShowHide).OnComplete(() => {
            panelContainer.gameObject.SetActive(false);
        });
    }

    public void Show()
    {
        buttonBackRect.DOKill();
        boardContainer.DOKill();

        panelContainer.gameObject.SetActive(true);
        boardContainer.anchoredPosition = -defaultBoardsPosition;
        buttonBackRect.anchoredPosition = -defaultButtonBackPosition;
        buttonBack.interactable = false;

        boardContainer.DOAnchorPos(Vector2.zero, durationShowHide);
        buttonBackRect.DOAnchorPos(defaultButtonBackPosition, durationShowHide).OnComplete(() => buttonBack.interactable = true);

        //CameraController.SetTweenCameraPositionAction?.Invoke(dependencyContainerSO.StoreMenuCameraPosition.position, dependencyContainerSO.StoreMenuCameraPosition.eulerAngles, null);
    }

    public void Init()
    {
        UIController.InterfacePanels.Add(this);
    }
    #endregion
}
