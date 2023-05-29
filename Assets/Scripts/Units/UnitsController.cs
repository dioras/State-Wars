using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private UnitsStorage unitsStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private StateStorage stateStorageSO = default;
    [SerializeField] private PlayerStorage playerStorageSO = default;
    [SerializeField] private GroupingStorage groupingStorageSO = default;

    [Header("Settings")]
    [SerializeField] private int unitsPoolSize = 10;    

    [Header("Actions")]
    public static Action<int, Transform, UnitType, GroupingType> SpawnUnitAction = default;
    public static Action<HexCell, GroupingType> SpawnTurrelAction = default;
    public static Action<LevelResult> FreezeAllUnitsAction = default;
    public static Action KillAllUnitsAction = default;
    public static Action<LevelResult> KillEnemyUnitsAction = default;
    public static Action<BaseController, GroupingType> AttackBaseAction = default;

    public List<UnitController> unitControllers = new List<UnitController>();
    public List<TurrelController> turrelControllers = new List<TurrelController>();

    public List<UnitController> UnitControllers => unitControllers;
    
    private void OnEnable()
    {
        SpawnUnitAction += SpawnUnit;
        SpawnTurrelAction += SpawnTurrel;
        FreezeAllUnitsAction += FreezeAllUnits;
        KillAllUnitsAction += KillAllUnits;
        KillEnemyUnitsAction += KillEnemyUnits;
        AttackBaseAction += AttackBase;
    }

    private void OnDisable()
    {
        SpawnUnitAction -= SpawnUnit;
        SpawnTurrelAction -= SpawnTurrel;
        FreezeAllUnitsAction -= FreezeAllUnits;
        KillAllUnitsAction -= KillAllUnits;
        KillEnemyUnitsAction -= KillEnemyUnits;
        AttackBaseAction -= AttackBase;
    }

    private void Awake()
    {
        dependencyContainerSO.UnitsController = this;
        CreateUnitsPool(UnitType.Easy);
        CreateUnitsPool(UnitType.Tank);
        CreateUnitsPool(UnitType.Hard);
        CreateUnitsPool(UnitType.Nulk);
        CreateUnitsPool(UnitType.NulkBuster);
        CreateUnitsPool(UnitType.Vinom);
        //CreateTurrelPool();

        dependencyContainerSO.UnitControllers = unitControllers;
    }

    private void SpawnUnit(int count, Transform point, UnitType unitType, GroupingType groupingType)
    {
        var parentCell = dependencyContainerSO.HexGrid.GetCell(point.position);
        var unionControl = groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys;
        List<Vector3> avialablePosToSpawn = new List<Vector3>();
        avialablePosToSpawn.Add(parentCell.transform.position);
        for (int i = 0; i < parentCell.Neighbors.Length; i++)
        {
            if (parentCell.Neighbors[i] == null) continue;
            if (parentCell.Neighbors[i].ControlType == unionControl && parentCell.Neighbors[i].IsEmpty == false)
            {
                avialablePosToSpawn.Add(parentCell.Neighbors[i].transform.position);
            }
        }

        for (int i = 0; i < count; i++)
        {
            var _unitSpawned = GetFreeUnit(unitType);
            _unitSpawned.IsBusy = true;
            _unitSpawned.gameObject.SetActive(true);
            _unitSpawned.transform.position = (i >= avialablePosToSpawn.Count ? point.position : avialablePosToSpawn[i]) + Vector3.one;
            _unitSpawned.transform.LookAt(groupingType == GroupingType.Union ? dependencyContainerSO.EnemyBase.transform : dependencyContainerSO.UnionBase.transform);
            _unitSpawned.transform.localScale = Vector3.one;
            _unitSpawned.GroupingType = groupingType;

            if (groupingType != GroupingType.Union){
                _unitSpawned.UnitHealth -= (int)(_unitSpawned.UnitHealth * .2f);
            }

            _unitSpawned.Config();
            _unitSpawned.SetUnitState = UnitState.Move;

            GamePanelController.AddUnitCountAction?.Invoke(groupingType, 1);
        }
        
        //dependencyContainerSO.EnemyBase.StartEnemyBase();
    }

    private void SpawnTurrel(HexCell point, GroupingType groupingType)
    {
        //var unionControl = groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys;
        var _turrelSpawn = GetFreeTurrel();
        _turrelSpawn.IsBusy = true;
        _turrelSpawn.gameObject.SetActive(true);
        _turrelSpawn.transform.position = point.transform.position + _turrelSpawn.transform.up / 2f;
        //_turrelSpawn.transform.LookAt(groupingType == GroupingType.Union ? dependencyContainerSO.EnemyBase.transform : dependencyContainerSO.UnionBase.transform);
        _turrelSpawn.transform.localScale = Vector3.one;
        _turrelSpawn.GroupingType = groupingType;
        
        _turrelSpawn.Config(groupingType, point);
        
        //dependencyContainerSO.EnemyBase.StartEnemyBase();
    }

    private UnitController GetFreeUnit(UnitType unitType)
    {
        var _freeUnit = unitControllers.Find((_unit) => _unit.IsBusy == false && _unit.UnitType == unitType);
        if (_freeUnit == null) {
            _freeUnit = Instantiate(unitsStorageSO.GetUnit(unitType).UnitPrefab, transform).GetComponent<UnitController>();
            _freeUnit.UnitType = unitType;
            _freeUnit.IsBusy = false;
            unitControllers.Add(_freeUnit);
        }
        return _freeUnit;
    }

    private TurrelController GetFreeTurrel()
    {
        var _freeTurrel = turrelControllers.Find((_turrel) => _turrel.IsBusy == false);
        if (_freeTurrel == null)
        {
            _freeTurrel = Instantiate(unitsStorageSO.Turrel.TurrelPrefab, transform).GetComponent<TurrelController>();
            _freeTurrel.IsBusy = false;
            turrelControllers.Add(_freeTurrel);
        }
        return _freeTurrel;
    }

    private void CreateUnitsPool(UnitType unitType)
    {
        for (int i = 0; i < unitsPoolSize; i++)
        {
            var _newUnit = Instantiate(unitsStorageSO.GetUnit(unitType).UnitPrefab, transform).GetComponent<UnitController>();
            _newUnit.UnitType = unitType;
            _newUnit.gameObject.SetActive(false);
            unitControllers.Add(_newUnit);
        }
    }

    private void CreateTurrelPool()
    {
        for (int i = 0; i <= 5; i++)
        {
            var _newTurrel = Instantiate(unitsStorageSO.Turrel.TurrelPrefab, transform).GetComponent<TurrelController>();
            _newTurrel.gameObject.SetActive(false);
            _newTurrel.IsBusy = false;
            turrelControllers.Add(_newTurrel);
        }
    }

    private void FreezeAllUnits(LevelResult _levelResult)
    {
        var _enemyGroup = groupingStorageSO.GetGrouping(stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping).Type;
        var groupingTypeWinner = _levelResult.Equals(LevelResult.Win) ? GroupingType.Union : _enemyGroup;

        unitControllers.ForEach((_unit) =>
        {
            _unit.StopAllCoroutines();
            _unit.ClearReservedCells();
            _unit.SetUnitState = UnitState.Happy;
            if (_unit.GroupingType.Equals(groupingTypeWinner) == false)
            {
                _unit.KillUnit();
            }

            _unit.PaintingParticleSystem.Stop();
        });
    }

    private void KillAllUnits()
    {
        unitControllers.ForEach((_unit) => {
            _unit.IsBusy = false;
            _unit.gameObject.SetActive(false);
        });
    }

    private void KillEnemyUnits(LevelResult levelResult)
    {
        unitControllers.ForEach((_unit) =>
        {
            if (_unit.IsBusy)
            {
                if (levelResult == LevelResult.Win && _unit.GroupingType != GroupingType.Union)
                {
                    _unit.KillUnit();
                }
                else if (levelResult == LevelResult.Lose && _unit.GroupingType == GroupingType.Union)
                {
                    _unit.KillUnit();
                }
            }
        });
    }

    private void AttackBase(BaseController baseController, GroupingType groupingType)
    {
        unitControllers.ForEach((_unit) => { 
            if (_unit.GroupingType == groupingType && _unit.IsBusy)
            {
                _unit.AttackEnemyBase(baseController);
            }
        });
    }
}
