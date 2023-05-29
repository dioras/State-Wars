using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "ScriptableObjects/GameStorage", fileName = "GameStorage")]
public class GameStorage : ScriptableObject
{
	[Header("GameBaseParameters")]
	[SerializeField] private GameBaseParameters gameBaseParameters = default;

	[Header("Balance Parameters")] 
	[SerializeField] private BalanceParameters balanceParameters = default;

	#region Geters/Seters
	public GameBaseParameters GameBaseParameters => gameBaseParameters;
	public BalanceParameters BalanceParameters => balanceParameters;
	#endregion
}

[Serializable]
public class GameBaseParameters
{
	[Header("Paint Generator")]
	[SerializeField] private float fillDuration = 2f;
	[SerializeField] private float sliderReactionDuration = .3f;

	[Header("UI Block")]
	[SerializeField] private float timeToInteractUI = 0.3f;
	[SerializeField] private float timeToShowHiddenButton = 3f;

	[Header("Rate US Block")]
	[SerializeField] private int reRateUsDelta = 259200;
	[SerializeField] private int playerLevelToRateUs = 1;

	[Header("ADS Block")]
	[SerializeField] private int playerLevelToInterstitial = 3;

	[Header("Coins")]
	[SerializeField] private int coinReward = 55; 

	[Header("Formating")]
	[SerializeField] private string formatingMoneyString = "$#,0";


	#region Geters/Seters
	public float FillDuration => fillDuration;
	public float SliderReactionDuration => sliderReactionDuration;
	public float TimeToShowHiddenButton { get => timeToShowHiddenButton; }
	public float TimeToInteractUI { get => timeToInteractUI; }
	public int ReRateUsDelta { get => reRateUsDelta; }
	public int PlayerLevelToRateUs { get => playerLevelToRateUs; }
	public int PlayerLevelToInterstitial { get => playerLevelToInterstitial; }
	public string FormatingMoneyString { get => formatingMoneyString; }
	public int CoinReward { get => coinReward; }
	#endregion
}

[Serializable] 
public class EnenmyBaseParams
{
	[SerializeField] private float minSpawnUnitsDuration = 2f;
	[SerializeField] private float maxSpawnUnitsDuration = 6f;
	[Tooltip("INFO: <time to spawn> - (<current mission> * <multiplier>)")]
	[SerializeField] private float multiplierDifficulties = 2f;
	[SerializeField] private int countEnenmySpawn = 2;

	#region get/set
	public float MinSpawnUnitsDuration => minSpawnUnitsDuration;
	public float MaxSpawnUnitsDuration => maxSpawnUnitsDuration;
	public float MultiplierDifficulties => multiplierDifficulties;
	public int CountEnenmySpawn => countEnenmySpawn;
	#endregion
}

[Serializable]
public class BalanceParameters
{
	[SerializeField] private int strengtheninWithLevelEnemyUnits = 5;
	[SerializeField] private float enemyTurrelDelay = 20f;
	[SerializeField] private EnenmyBaseParams enenmyBaseParams = default;
	[SerializeField] private List<BaseSettings> baseSettings = new List<BaseSettings>();

	#region get/set
	public int StrengtheninWithLevelEnemyUnits => strengtheninWithLevelEnemyUnits;
	public EnenmyBaseParams EnemyBaseParams => enenmyBaseParams;
	public float EnemyTurrelDelay => enemyTurrelDelay;
	#endregion

	public BaseSettings GetBaseSettings(BaseType baseType)
	{
		return baseSettings.Find((_settings) => _settings.BaseType == baseType);
	}
}

[Serializable]
public class BaseSettings
{
	[SerializeField] private BaseType baseType = default;
	[SerializeField] private int baseHealh = 100;
	[SerializeField] private float rateOfFire = .5f;
	[SerializeField] private int damageFromShoot = 10;
	[SerializeField] private int unitDamage = 1;

	public BaseType BaseType => baseType;
	public int BaseHealth => baseHealh;
	public float RateOfFire => rateOfFire;
	public int DamageFromShoot => damageFromShoot;
	public int UnitDamage => unitDamage;
}
