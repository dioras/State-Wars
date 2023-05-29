using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class Player
{
	[Header("Player's Property")]
	[SerializeField] private int playerCoins = 0;
	[SerializeField] private StateType currencyState = StateType.WA;

	[Header("Player Achivemant")]
	[SerializeField] private int playerCurrentMission = 1;

	[Header("Settings")]
	[SerializeField] private bool isMusicOn = false;
	[SerializeField] private bool isSoundOn = true;
	[SerializeField] private bool isVibrationcOn = true;

	[Header("RateUS")]
	[SerializeField] private bool isRateUs = false;
	[SerializeField] private JsonDateTime dateLastRateTry = new JsonDateTime();

	[Header("Player Last Session")]
	[SerializeField] private JsonDateTime timeLastSession = new JsonDateTime();

	[Header("No ADS")]
	[SerializeField] private bool isBuyADSOffer = false;

	[Header("Actions")]
	public static Action PlayerBuyBoxAction = default;
	public static Action PlayerBuySellCarrAction = default;
	public static Action PlayerChangeCoinAction = default;

	#region Geters/Seters
	public int PlayerCoins { get => playerCoins; }
	public JsonDateTime DateLastRateTry { get => dateLastRateTry; set => dateLastRateTry = value; }
	public bool IsRateUs { get => isRateUs; set => isRateUs = value; }
	public JsonDateTime TimeLastSession { get => timeLastSession; set => timeLastSession = value; }
	public bool IsBuyADSOffer { get => isBuyADSOffer; }
	public bool IsMusicOn { get => isMusicOn; }
	public bool IsSoundOn { get => isSoundOn; }
	public bool IsVibrationcOn { get => isVibrationcOn; }
	public int PlayerCurrentMission { get => playerCurrentMission; set => playerCurrentMission = value; }
	public StateType CurrecyStateType { get => currencyState; set => currencyState = value; }
	#endregion

	public Player()
    {
		playerCoins = 0;
		playerCurrentMission = 1;
	}

	public void ChangeCoins(int _coin) {
		playerCoins += _coin;
		PlayerChangeCoinAction?.Invoke();
	}

	public void SetSoundStatus(bool _status)
	{
		isSoundOn = _status;
	}

	public void SetMusicStatus(bool _status)
	{
		isMusicOn = _status;
	}

	public void SetVibrationtatus(bool _status)
	{
		isVibrationcOn = _status;
	}
}
