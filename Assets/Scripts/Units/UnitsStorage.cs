using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/UnitsStorage", fileName = "UnitsStorageSO")]
public class UnitsStorage : ScriptableObject
{
    [Header("Unit base settings")]
    [SerializeField] private float timeToFindNewPath = 1f;
    [SerializeField] private int countSpawnUnits = 3;
    [Space(15)]
    [SerializeField] private List<Unit> units = new List<Unit>();

    [SerializeField] private Turrel turrel = default;

    #region get/set
    public float TimeToFindNewPath => timeToFindNewPath;
    public int CountSpawnUnits => countSpawnUnits;
    public Turrel Turrel => turrel;
    #endregion

    public Unit GetUnit(UnitType unitType)
    {
        return units.Find((_unit) => _unit.UnitType == unitType);
    }

    public List<Unit> GetHeroUnits(){
        return units.FindAll((_unit) => _unit.IsHero);
    } 
}

[Serializable]
public class Unit
{
    [SerializeField] private UnitType unitType = default;
    [SerializeField] private bool isHero = false;
    [SerializeField] private FightType fightType = default;
    [SerializeField] private UnitPedestal unitPrefabPedestal = default;
    [SerializeField] private UnitController unitPrefab = default;
    [Range(0, 100)] [SerializeField] private int spawnProbability = default;
    [Space(20)] [SerializeField] private int unitHealth = 5;
    [SerializeField] private float unitSpeed = .2f;
    [SerializeField] private float unitDurationPainting = .1f;
    [SerializeField] private int unitPaintCellCount = 1;
    [SerializeField] private float waitForShoot = .5f;
    [SerializeField] private int unitDamage = 1;
    [Space(20)] [SerializeField] private int unitPrice = 1;

    #region get/set
    public UnitType UnitType => unitType;
    public bool IsHero => isHero;
    public float WaitForShoot => waitForShoot;
    public int UnitDamage => unitDamage;
    public UnitPedestal UnitPrefabPedestal => unitPrefabPedestal;
    public UnitController UnitPrefab => unitPrefab;
    public int UnitHealth => unitHealth;
    public float UnitSpeed => unitSpeed;
    public int SpawnProbability => spawnProbability;
    public float UnitDurationPainting => unitDurationPainting;
    public int UnitPrice => unitPrice;
    public int UnitPaintCellCount => unitPaintCellCount;
    public FightType FightType => fightType;
    #endregion
}

[Serializable]
public class Turrel
{
    [SerializeField] private TurrelPedestal turrelPedestalPrefab = default;
    [SerializeField] private TurrelController turrelPrefab = default;
    [SerializeField] private int price = 10;
    [SerializeField] private int turrelHealth = 500;
    [SerializeField] private int amountPaintShot = 5;

    public TurrelPedestal TurrelPedestal => turrelPedestalPrefab;
    public TurrelController TurrelPrefab => turrelPrefab;
    public int Price => price;
    public int TurrelHealth => turrelHealth;
    public int AmountPaintShot => amountPaintShot;
}
