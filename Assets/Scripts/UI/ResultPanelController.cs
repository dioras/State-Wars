using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultPanelController : MonoBehaviour, IInterfacePanel
{
	[Header("Base")]
	[SerializeField] private UIPanelType uiPanelType = UIPanelType.LevelResult;
	[SerializeField] private Transform panelContainer = default;
	[SerializeField] private PlayerStorage playerStorageSO = default;

	[Header("Components")]
	[SerializeField] private TMP_Text resultText = default;
	[SerializeField] private TMP_Text levelText = default;
	[SerializeField] private Image imageResult = default;
	[SerializeField] private GameObject winParticles = default;
	 
	[Header("Buttons")]
	[SerializeField] private Button backButton = default;

	[Header("Strings")]
	[SerializeField] private Sprite spriteWin = default;
	[SerializeField] private Sprite spriteLose = default;

	private void Awake() {
		Init();
	}

	private void OnEnable() {
		GameManager.LevelFinishAction += ShowResultPanel;
	}

	private void OnDisable() {
		GameManager.LevelFinishAction -= ShowResultPanel;
	}

	private void PrepareButtons() {
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			PlayerMapController.ShowPlayerMapAction?.Invoke(true);
			HexGrid.HideStateMapAction?.Invoke();
			UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.Main);
			UnitsController.KillAllUnitsAction?.Invoke();
		});
	}

	private void ShowResultPanel(LevelResult _levelResult) {

		UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.None);
		StartCoroutine(WaitToShowResult(_levelResult));

		if (_levelResult.Equals(LevelResult.Win))
        {
			resultText.text = "Territory occupied!";
			levelText.text = (playerStorageSO.ConcretePlayer.PlayerCurrentMission).ToString();
			imageResult.sprite = spriteWin;
			winParticles.SetActive(true);
		}
		else
        {
			resultText.text = "Territory lost!";
			levelText.text = (playerStorageSO.ConcretePlayer.PlayerCurrentMission + 1).ToString();
			imageResult.sprite = spriteLose;
			winParticles.SetActive(false);
		}
	}

	private IEnumerator WaitToShowResult(LevelResult levelResult)
    {
		//UnitsController.FreezeAllUnitsAction?.Invoke(levelResult);
		yield return new WaitForSecondsRealtime(1.5f);
		UIController.ShowUIPanelAloneAction(UIPanelType.LevelResult);
		//UnitsController.FreezeAllUnitsAction?.Invoke(levelResult);
	}

	#region IInterfacePanel
	public UIPanelType UIPanelType { get => uiPanelType; }

	public void Hide() 
	{
		panelContainer.gameObject.SetActive(false);
	}

	public void Show() {
		panelContainer.gameObject.SetActive(true);
		backButton.gameObject.SetActive(true);
		PrepareButtons();
	}

	public void Init() {
		UIController.InterfacePanels.Add(this);
	}
	#endregion
}
