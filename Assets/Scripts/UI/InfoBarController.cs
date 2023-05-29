using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class InfoBarController : MonoBehaviour
{
	[Header("Base")]
	[SerializeField] private PlayerStorage playerStorageSO = default;
	[SerializeField] private GameStorage gameStorageSO = default;
	[SerializeField] private Transform container = default;

	[Header("Components")]
	[SerializeField] private TMP_Text currencySoft = default;
	[SerializeField] private Button settingsButton = default;

	[Header("Settings Buttons")]
	[SerializeField] private Button closeSettingsButton = default;
	[SerializeField] private Toggle musicButton = default;
	[SerializeField] private Toggle soundButton = default;
	[SerializeField] private Toggle vibrationButton = default;
	[SerializeField] private Button saveButton = default;

	[Header("Settings Parameters")]
	[SerializeField] private RectTransform settingsPanel = default;
	[SerializeField] private RectTransform settingsContainer = default;
	[SerializeField] private Image backgroundImage= default;
	private bool isSoundOn = true;
	private bool isMusicOn = true;
	private bool isVibrationOn = true;

	[Header("Money Animation")]
	[SerializeField] private int animCount = 5;
	[SerializeField] private GameObject animPrefab = default;
	[SerializeField] private Transform animContainer = default;
	[SerializeField] private RectTransform animTarget = default;
	[SerializeField] private Vector3 movingScale = new Vector3(0.75f, 0.75f, 0.75f);
	[SerializeField] private float deltaPosition = 150f;
	[SerializeField] private Vector3 coinScaler = new Vector3(1.2f, 1.2f, 1.2f);
	[SerializeField] private RectTransform coinShakeRect = default;
	private Vector2 targetShakePosition = Vector2.zero;
	private Vector3 targetShakeRotation = Vector3.zero;
	private List<RectTransform> animsList = new List<RectTransform>();
	private CancellationTokenSource cancellationToken = new CancellationTokenSource();
	public static Action<Vector3> ShowMoneyAnimationAction = default;

	[Header("Variables")]
	public static Action FillUserInfoPanelAction = default;
	private Vector3 containerScale = default;

	private void Awake()
	{
		containerScale = settingsContainer.transform.localScale;

		SettingsDisplayStatus(false);
		PrepareButtons();
		//PrepareAnims();
	}

	private void OnEnable()
	{
		Hide();
		FillUserInfoPanelAction += FillUserInfoPanel;
		UIController.ShowUIPanelAloneAction += ReactPanel;
		ShowMoneyAnimationAction += ShowAnimation;
		Player.PlayerChangeCoinAction += SetCurrencyCounter;
	}

	private void OnDisable()
	{
		FillUserInfoPanelAction -= FillUserInfoPanel;
		UIController.ShowUIPanelAloneAction -= ReactPanel;
		ShowMoneyAnimationAction -= ShowAnimation;
		cancellationToken.Cancel();
		Player.PlayerChangeCoinAction -= SetCurrencyCounter;
	}

	private void OnDestroy()
	{
		cancellationToken.Cancel();
	}

	private void ReactPanel(UIPanelType _uIPanelType)
	{
		switch (_uIPanelType)
		{
			case UIPanelType.Main:
				Show();
				break;
			case UIPanelType.Store:
				Show();
				break;
			//case UIPanelType.Game:
			//	Show();
			//	break;
			//case UIPanelType.None:
			//	Show();
			//	break;
			default:
				Hide();
				break;
		}
	}

	private void Show(bool _interactable = true)
	{
		container.gameObject.SetActive(true);
		FillUserInfoPanel();

		settingsButton.interactable = _interactable;
	}

	private void Hide()
	{
		container.gameObject.SetActive(false);
	}

	private void SetCurrencyCounter()
	{
		currencySoft.text = playerStorageSO.ConcretePlayer.PlayerCoins.ToString(gameStorageSO.GameBaseParameters.FormatingMoneyString);
	}

	public void FillUserInfoPanel()
	{
		SetCurrencyCounter();

	}

	private void PrepareButtons()
	{
		settingsButton.onClick.RemoveAllListeners();
		settingsButton.onClick.AddListener(() => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			SettingsDisplayStatus(true);
		});

		closeSettingsButton.onClick.RemoveAllListeners();
		closeSettingsButton.onClick.AddListener(() => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			SettingsDisplayStatus(false);
		});

		musicButton.onValueChanged.RemoveAllListeners();
		musicButton.onValueChanged.AddListener((value) => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			isMusicOn = value;
		});

		soundButton.onValueChanged.RemoveAllListeners();
		soundButton.onValueChanged.AddListener((value) => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			isSoundOn = value;
		});

		vibrationButton.onValueChanged.RemoveAllListeners();
		vibrationButton.onValueChanged.AddListener((value) => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			isVibrationOn = value;
		});

		saveButton.onClick.RemoveAllListeners();
		saveButton.onClick.AddListener(() => {
			SoundManager.PlaySomeSoundOnce?.Invoke(SoundType.ButtonClick);
			SetSettings();
			SettingsDisplayStatus(false);
			playerStorageSO.SavePlayer();
		});
	}

	private void GetSettings()
	{
		musicButton.isOn = playerStorageSO.ConcretePlayer.IsMusicOn;
		soundButton.isOn = playerStorageSO.ConcretePlayer.IsSoundOn;
		vibrationButton.isOn = playerStorageSO.ConcretePlayer.IsVibrationcOn;
	}

	private void SetSettings()
	{
		playerStorageSO.ConcretePlayer.SetSoundStatus(isSoundOn);
		playerStorageSO.ConcretePlayer.SetVibrationtatus(isVibrationOn);
		playerStorageSO.ConcretePlayer.SetMusicStatus(isMusicOn);
		playerStorageSO.ApplyPlayerSettings();
		if (isMusicOn)
		{
			SoundManager.PlaySomeSoundContinuous?.Invoke(SoundType.MainMelody, () => true);
		}
		GetSettings();
	}

	private void SettingsDisplayStatus(bool active)
	{
		if (active)
		{
			settingsContainer.transform.DOScale(new Vector3(1, 1, 1), gameStorageSO.GameBaseParameters.TimeToInteractUI);
			settingsPanel.gameObject.SetActive(true);
			backgroundImage.gameObject.SetActive(true);
			GetSettings();
		}
		else
		{
			settingsContainer.transform.DOScale(Vector3.zero, gameStorageSO.GameBaseParameters.TimeToInteractUI).OnComplete(() => {
				backgroundImage.gameObject.SetActive(false);
				settingsPanel.gameObject.SetActive(false);
			});
		}
	}

	private void PrepareAnims()
	{
		for (int i = 0; i < animCount; i++)
		{
			animsList.Add(Instantiate(animPrefab, animContainer).GetComponent<RectTransform>());
			animsList[i].gameObject.SetActive(false);
		}
	}

	private void ShowAnimation(Vector3 _startPosition)
	{

		for (int i = 0; i < animCount; i++)
		{
			float rand = Random.Range(0.1f, gameStorageSO.GameBaseParameters.TimeToInteractUI * 3f);
			animsList[i].localScale = Vector3.one;
			animsList[i].position = new Vector3(_startPosition.x + Random.Range(-deltaPosition, deltaPosition), _startPosition.y + Random.Range(-deltaPosition, deltaPosition));
			animsList[i].gameObject.SetActive(true);

			MoveCoin(animsList[i], rand, cancellationToken);
		}
	}

	private async void MoveCoin(RectTransform _coin, float _time, CancellationTokenSource _tokenSource)
	{
		float speed = Vector3.Distance(animTarget.position, _coin.position) / _time;
		float startTime = Time.time;
		while (!_tokenSource.IsCancellationRequested && Vector3.Distance(animTarget.position, _coin.position) > 0.1f)
		{
			await Task.Yield();
			_coin.position = Vector3.MoveTowards(_coin.position, animTarget.position, speed * Time.deltaTime);
			float percentComplete = (Time.time - startTime) / _time;
			_coin.localScale = Vector3.Slerp(_coin.localScale, movingScale, percentComplete);
		}
		_coin.gameObject.SetActive(false);
		PingPong(_tokenSource);
	}

	private async void PingPong(CancellationTokenSource _tokenSource)
	{
		if (currencySoft.gameObject.transform.localScale == Vector3.one)
		{
			Shake(_tokenSource);
			float startTime = Time.time;
			float time = gameStorageSO.GameBaseParameters.TimeToInteractUI / 3f;
			while (!_tokenSource.IsCancellationRequested && (Time.time - startTime) < time)
			{
				await Task.Yield();
				float percentComplete = (Time.time - startTime) / time;
				currencySoft.gameObject.transform.localScale = Vector3.Slerp(currencySoft.gameObject.transform.localScale, coinScaler, percentComplete);
			}

			startTime = Time.time;
			while (!_tokenSource.IsCancellationRequested && (Time.time - startTime) < time)
			{
				await Task.Yield();
				float percentComplete = (Time.time - startTime) / time;
				currencySoft.gameObject.transform.localScale = Vector3.Slerp(currencySoft.gameObject.transform.localScale, Vector3.one, percentComplete);
			}
		}
	}

	private async void Shake(CancellationTokenSource _tokenSource)
	{
		var startPositin = coinShakeRect.anchoredPosition;

		float startTime = Time.time;
		float time = gameStorageSO.GameBaseParameters.TimeToInteractUI / 6f;
		targetShakePosition.x = Random.Range(-20f, 20f);
		targetShakePosition.y = Random.Range(-20f, 20f);
		targetShakeRotation.z = Random.Range(0f, 15f);

		while (!_tokenSource.IsCancellationRequested && (Time.time - startTime) < time)
		{
			await Task.Yield();
			float percentComplete = (Time.time - startTime) / time;
			coinShakeRect.anchoredPosition = Vector2.Lerp(coinShakeRect.anchoredPosition, coinShakeRect.anchoredPosition + targetShakePosition, time);
			coinShakeRect.localRotation = Quaternion.Euler(Vector3.Slerp(coinShakeRect.localRotation.eulerAngles, targetShakeRotation, percentComplete));
		}

		startTime = Time.time;
		while (!_tokenSource.IsCancellationRequested && (Time.time - startTime) < time)
		{
			await Task.Yield();
			float percentComplete = (Time.time - startTime) / time;
			coinShakeRect.anchoredPosition = Vector2.Lerp(coinShakeRect.anchoredPosition, startPositin, time);
			coinShakeRect.localRotation = Quaternion.Euler(Vector3.Slerp(coinShakeRect.localRotation.eulerAngles, Vector3.zero, percentComplete));
		}

		coinShakeRect.anchoredPosition = startPositin;
	}
}
