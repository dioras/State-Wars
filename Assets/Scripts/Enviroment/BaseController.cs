using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Random = System.Random;

public class BaseController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private GroupingStorage groupingStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private GameStorage gameStorageSO = default;
    [SerializeField] private UnitsStorage unitsStorageSO = default;
    [SerializeField] private PlayerStorage playerStorageSO = default;
    [SerializeField] private StateStorage stateStorageSO = default;

    [Header("Settings")]
    [SerializeField] private bool baseOccupied = false;
    [SerializeField] private GroupingType groupingType = default;
    [SerializeField] private float hideFlagPosY = .4f;
    [SerializeField] private float showFlagPosY = 5f;
    [SerializeField] private int baseHealth = 100;
    [SerializeField] private float range = 10;
    [SerializeField] private LayerMask unitPedestalLayer = default;

    [Header("Components")]
    [SerializeField] private Transform unitSpawnPoint = default;
    [SerializeField] private int materialChangeMark = 0;
    [SerializeField] private MeshRenderer meshRendererMark = default;
    [SerializeField] private int materialChangeBuikd = 0;
    [SerializeField] private MeshRenderer meshRendererBuild = default;
    [SerializeField] private Transform transformFlag = default;
    [SerializeField] private Transform healthBaContainer = default;
    [SerializeField] private List<BaseTriggerObject> baseTriggerObjects = new List<BaseTriggerObject>();
    [SerializeField] private Transform shootPoint = default;
    [SerializeField] private DestructionObject destructionObject = default;
    private UnitController unitTarget = default;

    [Header("Health bar components")]
    [SerializeField] private Slider healthBarSlider = default;

    private List<UnitType> unitTypes = new List<UnitType>();
    private List<HexCell> baseCellFromSpawn = new List<HexCell>();
    private Coroutine coroutineUnitSpawn = default;
    private Coroutine coroutineDamagedBase = default;
    private bool enemySpawn = false;
    private bool turrelavialable = true;
    private UnitType enemyHero = default;

    #region get/set
    public BaseType GetBaseType => groupingType == GroupingType.Union ? BaseType.UnionBase : BaseType.EnemyBase;
    public Color Color => groupingStorageSO.GetGrouping(groupingType).Color;
    public Transform UnitSpawnPoint => unitSpawnPoint;
    public GroupingType GroupingType
    {
        get => groupingType; set
        {
            groupingType = value;
        }
    }
    public List<HexCell> BaseCellFromSpawn => baseCellFromSpawn;
    public int Health => baseHealth;
    public bool BaseOccupioed { get => baseOccupied; set => baseOccupied = value; }
    #endregion

    private void OnEnable()
    {
        StateGenerator.StateCreatedAction += StartBase;
        GameManager.LevelFinishAction += LevelFinish;
        
        destructionObject.Restore();
    }

    private void OnDisable()
    {
        StateGenerator.StateCreatedAction -= StartBase;
        GameManager.LevelFinishAction -= LevelFinish;
    }

    private void Awake()
    {
        Prepare();
        ShowHealthBar(false);
    }

    private void LateUpdate()
    {
        if (healthBaContainer.gameObject.activeSelf)
        {
            healthBaContainer.LookAt(Camera.main.transform);
        }
    }
    

    private void Prepare()
    {
        unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList();
    }

    private void StartBase()
    {
        turrelavialable = true;
        PrepareBase();
        ShowHealthBar(false);
        if (coroutineUnitSpawn != null)
        {
            StopCoroutine(coroutineUnitSpawn);
        }
        if (groupingType != GroupingType.Union)
        {
            //enemySpawn = true;
            //coroutineUnitSpawn = StartCoroutine(EnemyBaseSceario());

            StartEnemyBase();
        }
        
        StartCoroutine(FindTarget());
        StartCoroutine(DamageTarget());
    }

    public void StartEnemyBase()
    {
        enemyHero = GetEnemyHero();

        if (coroutineUnitSpawn != null)
        {
            StopCoroutine(coroutineUnitSpawn);
            coroutineUnitSpawn = null;
        }
        enemySpawn = true;
        coroutineUnitSpawn = StartCoroutine(EnemyBaseScenario());
    }

    private UnitType GetEnemyHero(){
        var heroes = unitsStorageSO.GetHeroUnits();
        var heroesTypes = new List<UnitType>();
        heroes.ForEach((_hero) => heroesTypes.Add(_hero.UnitType));
        heroesTypes.Remove(dependencyContainerSO.UnionCurrencyHero);
        return heroesTypes[UnityEngine.Random.Range(0, heroesTypes.Count-1)];
    }
    
    private IEnumerator DamageTarget()
    {
        while (gameObject.activeSelf && dependencyContainerSO.InGame)
        {
            if (unitTarget != null)
            {
                if (Vector3.Distance(transform.position, unitTarget.transform.position) < range)
                {
                    GetDamage(gameStorageSO.BalanceParameters.GetBaseSettings(GetBaseType).DamageFromShoot);
                    BulletsController.ShootBulletAction?.Invoke(shootPoint.position, unitTarget.transform.position, .5f, groupingType,
                        () =>
                        {
                            if (unitTarget != null)
                            {
                                unitTarget.GetDamage(gameStorageSO.BalanceParameters.GetBaseSettings(GetBaseType).UnitDamage);
                            }
                        });
                }
                else
                {
                    unitTarget = null;
                }
            }
            
            yield return new WaitForSecondsRealtime(gameStorageSO.BalanceParameters.GetBaseSettings(GetBaseType).RateOfFire);
        }
    }

    private IEnumerator FindTarget()
    {
        while (gameObject.activeSelf && dependencyContainerSO.InGame)
        {
            if (unitTarget == null || unitTarget.IsBusy == false)
            {
                unitTarget = FindNearEnemyUnit(range);
            }
            yield return new WaitForSecondsRealtime(.5f);
        }
    }
    
    private UnitController FindNearEnemyUnit(float range)
    {
        float distance = -1f;
        UnitController result = null;
        foreach (var unit in dependencyContainerSO.UnitControllers)
        {
            if (unit.GroupingType != groupingType && unit.IsBusy)
            {
                float distanceToUnit = Vector3.Distance(transform.position, unit.transform.position);
                if ((distanceToUnit < distance || distance == -1) && distanceToUnit < range)
                {
                    distance = distanceToUnit;
                    result = unit;
                }
            }
        }

        return result;
    }

    private void StopSpawnEnenmy()
    {
        enemySpawn = false;
        if (coroutineUnitSpawn != null)
        {
            StopCoroutine(coroutineUnitSpawn);
            coroutineUnitSpawn = null;
        }
    }

    private void PrepareBase()
    {
        baseHealth = gameStorageSO.BalanceParameters.GetBaseSettings(GetBaseType).BaseHealth;       
        healthBarSlider.maxValue = baseHealth;
        healthBarSlider.value = baseHealth;

        transform.localScale = Vector3.one;
        var _group = groupingStorageSO.GetGrouping(groupingType);
        transformFlag.DOLocalMoveY(hideFlagPosY, 1f).OnComplete(() => {
            meshRendererMark.materials[materialChangeMark].DOColor(_group.Color, .5f);
            meshRendererBuild.materials[materialChangeBuikd].DOColor(_group.ColorBase, .5f);
            transformFlag.DOLocalMoveY(showFlagPosY, 1f);
        });
    }

    public void AddSpawnCell(HexCell hexCell)
    {
        if (baseCellFromSpawn.Contains(hexCell).Equals(false))
        {
            baseCellFromSpawn.Add(hexCell);
        }
    }

    public Transform GetSpawnPoint()
    {
        var unionControl = groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys;
        baseCellFromSpawn = DependencyContainer.Shuffle(baseCellFromSpawn);
        foreach (var _sPoint in baseCellFromSpawn)
        {
            if (_sPoint.IsEmpty == false && _sPoint.ControlType == unionControl)
            {
                return _sPoint.transform;
            }
        }
        return null;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void GetDamage(int _damage)
    {
        baseHealth -= _damage;
        baseHealth = baseHealth < 0 ? 0 : baseHealth;
        healthBarSlider.DOKill();
        healthBarSlider.DOValue(baseHealth, .3f);
        if (baseHealth == 0)
        {
            DestroyBase();
            if (groupingType == GroupingType.Union)
            {
                GameManager.LevelFinishAction?.Invoke(LevelResult.Lose);
                UnitsController.FreezeAllUnitsAction?.Invoke(LevelResult.Lose);
            }
            else
            {
                GameManager.LevelFinishAction?.Invoke(LevelResult.Win);
                UnitsController.FreezeAllUnitsAction?.Invoke(LevelResult.Win);
            }
            
            ShowHealthBar(false);
            baseOccupied = true;
        }
        else
        {
            ShowHealthBar(true);
        }
    }

    private IEnumerator EnemyBaseScenario()
    {
        yield return new WaitForSecondsRealtime(5f);
        while (enemySpawn)
        {
            float _waitToNextUnit = UnityEngine.Random.Range(gameStorageSO.BalanceParameters.EnemyBaseParams.MinSpawnUnitsDuration, gameStorageSO.BalanceParameters.EnemyBaseParams.MaxSpawnUnitsDuration);
            _waitToNextUnit -= (playerStorageSO.ConcretePlayer.PlayerCurrentMission *
                                gameStorageSO.BalanceParameters.EnemyBaseParams.MultiplierDifficulties);
            yield return new WaitForSecondsRealtime(_waitToNextUnit);
            if (dependencyContainerSO.InGame)
            {
                var res = GetRandomUnitType();
                if (res != UnitType.None)
                {
                    //if (dependencyContainerSO.PaintAvialableEnemy >= unitsStorageSO.GetUnit(res).UnitPrice)
                    {
                        //dependencyContainerSO.PaintAvialableEnemy -= unitsStorageSO.GetUnit(res).UnitPrice;
                        UnitsController.SpawnUnitAction?.Invoke(
                            gameStorageSO.BalanceParameters.EnemyBaseParams.CountEnenmySpawn, transform, res,
                            GroupingType);
                    }
                }
               /*else if (dependencyContainerSO.PaintAvialableEnemy >= unitsStorageSO.Turrel.Price && turrelavialable)
                {
                    var cell = dependencyContainerSO.HexGrid.GetRandomBoardCell(ControlType.Enemys);
                    if (cell != null)
                    {
                        dependencyContainerSO.PaintAvialableEnemy -= unitsStorageSO.Turrel.Price;
                        UnitsController.SpawnTurrelAction?.Invoke(cell, groupingType);
                        turrelavialable = false;
                        StartCoroutine(WaitForAvialableTurrel());
                    }
                }*/
            }
            else
            {
                break;
            }
        }

        coroutineUnitSpawn = null;
    }

    private IEnumerator WaitForAvialableTurrel()
    {
        yield return new WaitForSecondsRealtime(gameStorageSO.BalanceParameters.EnemyTurrelDelay);
        turrelavialable = true;
    }

    private UnitType GetRandomUnitType()
    {
        Random random = new Random();
        int randomValueForUnit = random.Next(0, 100);
        
        var avialableUnits = unitTypes.FindAll((_uType) => randomValueForUnit > (unitsStorageSO.GetUnit(_uType).SpawnProbability - (playerStorageSO.ConcretePlayer.PlayerCurrentMission * gameStorageSO.BalanceParameters.StrengtheninWithLevelEnemyUnits)));
        
        var heroes = unitsStorageSO.GetHeroUnits();
        heroes.ForEach((_hero) => {
            if (_hero.UnitType != enemyHero){
                avialableUnits.Remove(_hero.UnitType);
            }
        });

        Random _random = new Random();
        int randomIndex = _random.Next(0, avialableUnits.Count);
        if (avialableUnits.Count > 0)
        {
            var result = DependencyContainer.Shuffle(avialableUnits)[randomIndex];
            dependencyContainerSO.PaintAvialableEnemy -= unitsStorageSO.GetUnit(result).UnitPrice;
            return result;
        }
        else
        {
            return UnitType.None;
        }
    }
    private void DestroyBase()
    {
        VibrationController.Vibrate(40);

        destructionObject.Broke();
        StopSpawnEnenmy();
    }

    private void LevelFinish(LevelResult levelResult)
    {
        groupingType = levelResult == LevelResult.Win ?
                        GroupingType.Union :
                        groupingStorageSO.GetGrouping(stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping).Type;
        StartBase();
        //DestroyBase();
    }

    public HexCell GetCellNearToBase()
    {
        var _cells = new List<HexCell>();
        foreach(var trigger in baseTriggerObjects)
        {
            trigger.GetHexCells.ForEach((_cell) => {
                if (_cells.Contains(_cell) == false)
                {
                    _cells.Add(_cell);
                }
            });
        }

        return _cells.Count > 0 ? _cells[UnityEngine.Random.Range(0, _cells.Count-1)] : null;
    }

    public void ShowHealthBar(bool _show)
    {
        healthBaContainer.gameObject.SetActive(_show);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(groupingType == GroupingType.Union ? Tags.UnitEnemy : Tags.Unit) && other.gameObject.layer != unitPedestalLayer)
        {
            var unitController = other.transform.parent.GetComponent<UnitController>();
            if (unitController != null){
                unitController.AttackEnemyBase(this);
            }
        }
    }
}
