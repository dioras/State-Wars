using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class PromoPanelController : MonoBehaviour, IInterfacePanel
{
	[Header("Base")]
	[SerializeField] private UIPanelType uiPanelType = UIPanelType.Promo;
	[SerializeField] private Transform panelContainer = default;
	[SerializeField] private PlayerStorage playerStorageSO = default;
	[SerializeField] private StateStorage stateStorageSO = default;

	[Header("Loading Image")]
	[SerializeField] private Slider loadingSlider = default;
	[SerializeField] private float fillingTime = default;
	[SerializeField] private Image redFlag = default;
	[SerializeField] private Color unionColor = default;

	[Header("Build Version")]
	[SerializeField] private TMP_Text buildVersionText = default;
	[SerializeField] private string builVersionPrefixString = "ver.";

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		GameManager.GameStartAction += ShowPromo;
	}

	private void OnDisable()
	{
		GameManager.GameStartAction -= ShowPromo;
	}

	private void ShowPromo()
	{
		UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.Promo);
	}

	#region IInterfacePanel
	public UIPanelType UIPanelType { get => uiPanelType; }

	public void Hide()
	{
		panelContainer.gameObject.SetActive(false);
	}

	public void Show()
	{
		buildVersionText.text = $"{builVersionPrefixString}{Application.version}";
		if (panelContainer != null) 
        {
			panelContainer.gameObject.SetActive(true);
		}
		
		loadingSlider.value = .1f;
		loadingSlider.DOValue(.9f, fillingTime).SetEase(Ease.Flash).OnComplete(() =>
		{
			loadingSlider.DOValue(1f, .5f);
			redFlag.DOColor(unionColor, .5f).OnComplete(() => {
				playerStorageSO.LoadPlayer();
				stateStorageSO.Load();

				if (stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping == GroupingType.Union)
				{
					playerStorageSO.ConcretePlayer.CurrecyStateType = stateStorageSO.GetNextState();
				}
			});
		});
	}

	public void Init()
	{
		UIController.InterfacePanels.Add(this);
	}
	#endregion
}
