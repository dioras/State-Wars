using System;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
	[Header("Base")]
	[SerializeField] private PlayerStorage playerStorageSO = default;
	[SerializeField] private StateStorage stateStorageSO = default;
	[SerializeField] private GroupingStorage groupingStorageSO = default;
	[SerializeField] private DependencyContainer dependencyContainerSO = default;

	[Header("Global Actions")]
	public static Action GameStartAction = default;
	public static Action PlayerLoadedAction = default;
	public static Action LevelStartAction = default;
	public static Action<LevelResult> LevelFinishAction = default;
	
	[Header("Variables")]
	private float startLevelTime = default;

	private void OnEnable()
	{
		LevelStartAction += StartLevel;
		LevelFinishAction += FinishLevel;
		PlayerLoadedAction += PlayMusic;
	}

	private void Start()
	{
		DOTween.SetTweensCapacity(2000, 50);
		GameStartAction?.Invoke();
	}

	private void OnDisable()
	{
		LevelStartAction -= StartLevel;
		LevelFinishAction -= FinishLevel;
		PlayerLoadedAction -= PlayMusic;
		playerStorageSO.SavePlayer();
		stateStorageSO.Save();
	}

	private void OnDestroy()
	{
		playerStorageSO.SavePlayer();
		stateStorageSO.Save();
	}

	private void OnApplicationQuit()
	{
		playerStorageSO.SavePlayer();
		stateStorageSO.Save();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			playerStorageSO.SavePlayer();
			stateStorageSO.Save();
		}
	}

	private void PlayMusic()
	{
		SoundManager.PlaySomeSoundContinuous?.Invoke(SoundType.MainMelody, () => true);
	}

	private void StartLevel()
	{
		//Metrica.StartLevelEvent(playerStorageSO.ConcretePlayer.PlayerCurrentMission);
		startLevelTime = Time.time;
		dependencyContainerSO.InGame = true;
	}

	private void FinishLevel(LevelResult _levelResult)
	{
		//Metrica.EndLevelEvent(playerStorageSO.ConcretePlayer.PlayerCurrentMission, _levelResult, (int)(Time.time - startLevelTime));
		VibrationController.Vibrate(30);
		dependencyContainerSO.InGame = false;
		GamePanelController.EnableTutrialAction?.Invoke(false, default, default, default);

		StateGenerator.ShowPedestalAction?.Invoke(false);
		//var _enemyGroup = groupingStorageSO.GetGrouping(stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping).Type;
		//UnitsController.FreezeAllUnitsAction?.Invoke(_levelResult.Equals(LevelResult.Win) ? GroupingType.Union : _enemyGroup);

		var _currencyState = stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType);
		if (_levelResult == LevelResult.Win)
		{
			_currencyState.Grouping = GroupingType.Union;
			playerStorageSO.ConcretePlayer.CurrecyStateType = stateStorageSO.GetNextState();
			playerStorageSO.ConcretePlayer.PlayerCurrentMission++;
			dependencyContainerSO.HexGrid.SetAllHexCellColor(groupingStorageSO.GetGrouping(GroupingType.Union).Color, ControlType.Union);
		}
        else
        {
			dependencyContainerSO.HexGrid.SetAllHexCellColor(groupingStorageSO.GetGrouping(_currencyState.Grouping).Color, ControlType.Enemys);
		}
	}
}
