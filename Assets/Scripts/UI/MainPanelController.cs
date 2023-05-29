using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class MainPanelController : MonoBehaviour, IInterfacePanel
{
	[Header("Base")]
	[SerializeField] private PlayerStorage playerStorageSO = default;
	[SerializeField] private GroupingStorage groupingStorageSO = default;
	[SerializeField] private StateStorage stateStorageSO = default;
	[SerializeField] private DependencyContainer dependencyContainerSO = default;

	[Header("Settings")]
	[SerializeField] private UIPanelType uiPanelType = UIPanelType.Main;
	[SerializeField] private Transform panelContainer = default;

	[Header("Component")]
	[SerializeField] private EventTrigger eventTriggerPlay = default;
	[SerializeField] private Button buttonStore = default;

	[Header("Progress Bar")]
	[SerializeField] private RectTransform containerProgress = default;
	[SerializeField] private GroupingSliderObject groupingSliderPrefab = default;
	[SerializeField] private Transform sliderParent = default;
	[SerializeField] private TMP_Text controlText = default;

	[Header("Strings")]
	[SerializeField] private string formatControlProgressString = "You control <color=yellow>{0}%</color> of the territory!";

	private List<GroupingSliderObject> groupingSliderObjects = new List<GroupingSliderObject>();
	
	private void Awake()
	{
		Init();
	}

    private void Start()
    {
		PrepareButtons();
		PrepareProgressSliders();
	}

    private void PrepareProgressSliders()
    {
		var _groupList = groupingStorageSO.GetGroupingList;
		for (int i = 0; i < _groupList.Count; i++)
        {
			var _newSlider = Instantiate(groupingSliderPrefab, sliderParent).GetComponent<GroupingSliderObject>();
			_newSlider.Slider.value = 0;
			_newSlider.GroupIcon.color = new Color(0f, 0f, 0f, 0f);
			_newSlider.TextProgress.text = "";
			groupingSliderObjects.Add(_newSlider); 
		}
		controlText.text = string.Format(formatControlProgressString, 0);
	}

	private void UpdateProgressData(List<Grouping> groupings)
    {
		float currencyValue = 0;
		for (int i = groupingSliderObjects.Count-1; i >= 0; i--)
        {
			float controlledTerretory = (100f / stateStorageSO.Country.States.Count) * stateStorageSO.Country.GetCountStateControlled(groupings[i].Type);
			currencyValue += controlledTerretory;

			groupingSliderObjects[i].FillImage.color = controlledTerretory > 0f ? groupings[i].Color : default;

			if (controlledTerretory > 1f)
			{
				groupingSliderObjects[i].GroupIcon.color = groupings[i].Color;
				groupingSliderObjects[i].GroupIcon.sprite = groupings[i].Sprite;
			}
			else
            {
				groupingSliderObjects[i].GroupIcon.color = default;
			}

			groupingSliderObjects[i].Slider.value = currencyValue;
			groupingSliderObjects[i].TextProgress.text = controlledTerretory > 0f ? (int)controlledTerretory + "%" : "";

			if (groupings[i].Type == GroupingType.Union)
            {
				controlText.text = string.Format(formatControlProgressString, (int)controlledTerretory);
			}
		}
	}

	private void PrepareButtons()
    {
		eventTriggerPlay.triggers.Clear();
		EventTrigger.Entry _tap = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		_tap.callback.AddListener((data) => {
			UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.None);
			BackgroundController.DarkenAction?.Invoke(true, null);
			CameraController.SetTweenCameraPositionAction?.Invoke(dependencyContainerSO.CurrencyStatePlayerMap, dependencyContainerSO.CameraViewStatePoint.eulerAngles, null);
			CameraController.SetOrthographicSizeAction?.Invoke(35f, () => {
				BackgroundController.DarkenAction?.Invoke(false, () => GameManager.LevelStartAction?.Invoke());
				PlayerMapController.ShowPlayerMapAction?.Invoke(false);
				StateGenerator.CreateStateAction?.Invoke(playerStorageSO.ConcretePlayer.CurrecyStateType);
			});
		});
		eventTriggerPlay.triggers.Add(_tap);

		buttonStore.onClick.AddListener(() => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.Store);
		});
	}

	#region IInterfacePanel
	public UIPanelType UIPanelType { get => uiPanelType; }

	public void Hide()
	{
		panelContainer.gameObject.SetActive(false);
	}

	public void Show()
	{
		panelContainer.gameObject.SetActive(true);
		containerProgress.anchoredPosition = Vector2.up * 550;
		UpdateProgressData(groupingStorageSO.GetGroupingList);
		containerProgress.DOAnchorPosY(0f, .5f);
	}

	public void Init()
	{
		UIController.InterfacePanels.Add(this);
	}
	#endregion
}
