using UnityEngine;
using UnityEngine.UI;

public class RatePopupController : MonoBehaviour, IInterfacePanel
{
	[Header("Base")]
	[SerializeField] private UIPanelType uiPanelType = UIPanelType.PU_RateUs;
	[SerializeField] private Transform panelContainer = default;
	[SerializeField] private PlayerStorage playerStorageSO = default;

	[Header("Rate Buttons")]
	[SerializeField] private Button goodButton = default;
	[SerializeField] private Button notGoodButton = default;
	[SerializeField] private Button closeButton = default;

	private void Awake()
	{
		Init();
		PrepareButtons();
	}

	private void PrepareButtons()
	{
		goodButton.onClick.RemoveAllListeners();
		goodButton.onClick.AddListener(() => {
			playerStorageSO.TryRateUS(RateUsResultType.Rate);
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			Application.OpenURL("market://details?id=car.repair.mini.games");
			Hide();
		});

		notGoodButton.onClick.RemoveAllListeners();
		notGoodButton.onClick.AddListener(() => {
			playerStorageSO.TryRateUS(RateUsResultType.NoRate);
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			Hide();
		});

		closeButton.onClick.RemoveAllListeners();
		closeButton.onClick.AddListener(() => {
			playerStorageSO.TryRateUS(RateUsResultType.Close);
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			Hide();
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
	}

	public void Init()
	{
		UIController.InterfacePanels.Add(this);
	}
	#endregion
}
