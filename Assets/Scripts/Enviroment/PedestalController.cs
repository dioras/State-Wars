using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PedestalController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private UnitsStorage unitsStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private GroupingStorage groupingStorageSO = default;
    [SerializeField] private PlayerStorage playerStorageSO = default;

    [Header("Settings")]
    [SerializeField] private Vector3 unitsScale = Vector3.one;
    [SerializeField] private Vector3 unitsOffset = default;
    [SerializeField] private LayerMask layerIgnore = default;

    [Header("Components")]
    [SerializeField] private List<Transform> pointsUnit = new List<Transform>();
    private Dictionary<Transform, UnitPedestal> pointsAvailable = new Dictionary<Transform, UnitPedestal>();
    private List<UnitPedestal> unitsPool = new List<UnitPedestal>();

    private UnitType currencyHero = default;
    private int margeCountTutorial = 3;

    [Header("Actions")]
    public static Action LoadUnitsAction = default;
    public static Action AutoSpawnAction = default;

    private HexCell selectedCell = default;
    private UnitType selectedUnitType = default;
    private UnitPedestal unitForMarge = default;
    private UnitPedestal UnitForMarge {
        get { return unitForMarge; }
        set {
            if (unitForMarge != value){
                unitForMarge?.DeselectedToMare();
                unitForMarge = value;
                if (unitForMarge != null && selectedUnitType == unitForMarge.UnitType){
                    unitForMarge?.SelectedToMarge();
                }
            }
        }
    }

    private RaycastHit raycastHit = default;
    private Collider selectedUnit = default;
    private Color unitColor = default;

    private UnitPedestal firstUnit = default;
    private UnitPedestal secondUnit = default;
    private UnitPedestal thirdUnit = default;

    private Coroutine coroutineAutoSpawn = default;

    public Collider SelectedUnit => selectedUnit;

    private void OnEnable()
    {
        LoadUnitsAction += LoadUnits;
        AutoSpawnAction += AutoSpawn;
       // PaintGenerator.UpdatePaintValueAction += CheckAvialableUnits;
    }

    private void OnDisable()
    {
        LoadUnitsAction -= LoadUnits;
        AutoSpawnAction -= AutoSpawn;
       // PaintGenerator.UpdatePaintValueAction += CheckAvialableUnits;
    }

    private void Awake() {
        dependencyContainerSO.PedestalContainer = this;
        CreateUnitsPool();
    }

    private void Update()
    {
        if (dependencyContainerSO.InGame)
        {
            if (Input.GetMouseButtonDown(0)){
                DragUnit();
            }
            if (Input.GetMouseButton(0) && selectedUnit != null)
            {
                DragUnit();
                if (coroutineAutoSpawn != null)
                {
                    StopCoroutine(coroutineAutoSpawn);
                    coroutineAutoSpawn = null;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                DropUnit();
                if (coroutineAutoSpawn != null)
                {
                    StopCoroutine(coroutineAutoSpawn);
                    coroutineAutoSpawn = null;
                }
            }
        }
    }

    private void DragUnit()
    {
        if (dependencyContainerSO.BaseControlledUnion)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 500f, Color.red);
            if (Physics.Raycast(ray, out raycastHit, 500f, ~layerIgnore))
            {
                if (raycastHit.collider.tag.Equals(Tags.Unit))
                {
                    if (selectedUnit == null)
                    {
                        GamePanelController.EnableTutrialAction?.Invoke(false, default, default, default);
                        selectedUnit = raycastHit.collider;
                        VibrationController.Vibrate(30);
                        raycastHit.collider.enabled = false;

                        selectedUnitType = selectedUnit.GetComponent<UnitPedestal>().UnitType;
                    }
                    else 
                    {
                        if (UnitForMarge != null && UnitForMarge.Collider != raycastHit.collider){
                            UnitForMarge = raycastHit.collider.GetComponent<UnitPedestal>();
                        }
                        else {
                            UnitForMarge = raycastHit.collider.GetComponent<UnitPedestal>();
                        }
                    }
                }
                else if (raycastHit.collider.tag.Equals(Tags.Cell) && selectedUnit != null)
                {
                    var _selCell = dependencyContainerSO.HexGrid.GetCell(raycastHit.collider.transform.position);;
                    selectedCell = _selCell;
                }
                else if (selectedUnit == null)
                {
                    selectedCell = null;
                    UnitForMarge = null;
                }
                else {
                    UnitForMarge = null;
                    selectedCell = null;
                }

                if (selectedUnit != null)
                {
                    selectedUnit.transform.position = raycastHit.point + unitsOffset;
                }
            }
        }
    }

    private void DropUnit()
    {
        if (selectedUnit != null)
        {
            if (selectedUnit.tag.Equals(Tags.Unit))
            {
                var unit = selectedUnit.GetComponent<UnitPedestal>();
                if (unit != null)
                {
                    var unitPrice = unitsStorageSO.GetUnit(unit.UnitType)
                        .UnitPrice;
                    //if (dependencyContainerSO.PaintAvialable >= unitPrice)
                    //{
                        Transform pointSpawn = default;
                        if (UnitForMarge != null){
                            if (UnitForMarge.UnitType == unit.UnitType){
                                MargeUnits(unit, UnitForMarge);
                            }
                        }
                        else if (selectedCell != null)
                        {
                            if (pointsAvailable.ContainsKey(unit.transform.parent)){
                                pointsAvailable[unit.transform.parent] = null;
                            }
                            unit.Reserve();

                            pointSpawn = dependencyContainerSO.HexGrid
                                    .FindNearUnionHex(selectedCell.transform.position, ControlType.Union).transform;
                                

                            VibrationController.Vibrate(30);
                            dependencyContainerSO.PaintAvialable -= unitPrice;
                            var spawnCount = unit.UnitType == UnitType.Tank || IsHero(unit.UnitType) ? 1 : unitsStorageSO.CountSpawnUnits;
                            UnitsController.SpawnUnitAction?.Invoke(spawnCount,
                                pointSpawn, unit.UnitType,
                                GroupingType.Union);

                            selectedUnit = null;
                        }
                    //}
                }
            }
        }

        
        RevertUnitsToPedestal();
    }

    private void MargeUnits(UnitPedestal selectedUnit, UnitPedestal unitMarge){
        var nextUnitType = selectedUnit.UnitType+1;
        if (IsHero(nextUnitType)){
            nextUnitType = currencyHero;
        }

        if (IsHero(selectedUnit.UnitType) == false)
        {
            //nextUnitType = currencyHero;
            if (pointsAvailable.ContainsKey(selectedUnit.transform.parent)){
                pointsAvailable[selectedUnit.transform.parent] = null;
            }
            if (pointsAvailable.ContainsKey(unitMarge.transform.parent)){
                pointsAvailable[unitMarge.transform.parent] = null;
            }
            selectedUnit.Reserve();
            unitMarge.Reserve();

            CreatePedestalUnit(nextUnitType, unitMarge.transform.parent);
            ParticleController.PlayParticleAction?.Invoke(unitMarge.transform.parent.position + Vector3.up * 2.5f, ParticleType.MargePuff);
            margeCountTutorial--;

            if (margeCountTutorial > 0){
                 GamePanelController.EnableTutrialAction?.Invoke(true, unitMarge.transform.parent.position, dependencyContainerSO.UnionBase.transform.position, default);
            }

            if (IsHero(nextUnitType)){
                GamePanelController.EnableTutrialAction?.Invoke(true, unitMarge.transform.parent.position, dependencyContainerSO.UnionBase.transform.position, MoveType.Drop);
            }
        }
        else {
            RevertUnitsToPedestal();
        }
        
    }

    private bool IsHero(UnitType unitType){
        return unitType == UnitType.Nulk || unitType == UnitType.NulkBuster || unitType == UnitType.Vinom || unitType == UnitType.None;
    }

    private UnitType GetRandomHero(){
        if (playerStorageSO.ConcretePlayer.PlayerCurrentMission > 3){
            List<UnitType> heroTypes = new List<UnitType>();
            heroTypes.Add(UnitType.Nulk);
            heroTypes.Add(UnitType.NulkBuster);
            heroTypes.Add(UnitType.Vinom);
            return heroTypes[UnityEngine.Random.Range(0, heroTypes.Count)];
        }
        else {
            return UnitType.Vinom;
        }
    }
    
    private void AutoSpawn()
    {
        if (coroutineAutoSpawn == null)
        {
            coroutineAutoSpawn = StartCoroutine(AutoSpawnUnit());
        }
    }

    private IEnumerator AutoSpawnUnit()
    {
        yield return new WaitForSecondsRealtime(3f);
        if (dependencyContainerSO.InGame)
        {
            var _unitPrice = unitsStorageSO.GetUnit(UnitType.Easy).UnitPrice;
            if (dependencyContainerSO.PaintAvialable >= _unitPrice)
            {
                dependencyContainerSO.PaintAvialable -= _unitPrice;
                UnitsController.SpawnUnitAction?.Invoke(1, dependencyContainerSO.UnionBase.transform, UnitType.Easy, GroupingType.Union);
            }
        }
        coroutineAutoSpawn = null;
    }

    private void RevertUnitsToPedestal()
    {
        UnitForMarge = null;
        foreach(var pointKey in pointsAvailable.Keys){
            if (pointsAvailable[pointKey] != null){
                var unit = pointsAvailable[pointKey];
                unit.transform.localPosition = Vector3.zero;
                unit.transform.localEulerAngles = Vector3.zero;
                unit.Collider.enabled = true;
            }
        }
        selectedUnit = null;
        selectedCell = null;
    }

    private void RevertUnit(Collider unit)
    {
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localEulerAngles = Vector3.zero;
        unit.enabled = true;
    } 

    private void ConfigUnitPedesta(UnitType unitType, UnitPedestal unit)
    {
        unitColor = groupingStorageSO.GetGrouping(GroupingType.Union).ColorStickmans;
        unit.SetStickmanColor = unitColor;

        unit.UnitType = unitType;
    }

    private void LoadUnits(){
        margeCountTutorial = 3;

        currencyHero = GetRandomHero();
        dependencyContainerSO.UnionCurrencyHero = currencyHero;

        pointsUnit.ForEach((_point) => {
            if(pointsAvailable.ContainsKey(_point)){
                if (pointsAvailable[_point] != null){
                    pointsAvailable[_point].Reserve();
                }
                pointsAvailable[_point] = null;
            }
            else {
                pointsAvailable.Add(_point, null);
            }
        });

        StopAllCoroutines();
        StartCoroutine(UnitGenerator());
    }

    private void CreateUnitsPool(){
        var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList<UnitType>();
        unitTypes.Remove(UnitType.None);
        unitTypes.ForEach((_unitType) => {
            for (int i = 0; i < 5; i++){
                CreateUnitForPool(_unitType);
            }
        });
    }

    private UnitPedestal GetFreeUnitPedestal(UnitType unitType){
        var freeUnit = unitsPool.Find((_unit) => _unit.IsBusy == false && _unit.UnitType == unitType);
        if (freeUnit == null){
            freeUnit = CreateUnitForPool(unitType);
        }

        return freeUnit;
    }

    private UnitPedestal CreateUnitForPool(UnitType unitType){
        var prefab = unitsStorageSO.GetUnit(unitType).UnitPrefabPedestal;
        var instance = Instantiate(prefab, transform);
        instance.UnitType = unitType;
        instance.transform.localScale = Vector3.one;
        instance.Reserve();
        unitsPool.Add(instance);
        ConfigUnitPedesta(unitType, instance);
        return instance;
    }

    private IEnumerator UnitGenerator(){
        while(gameObject.activeSelf){
            var freePoint = FreePoint();
            if (freePoint != null){
                PaintGenerator.StartReloadUnitAction?.Invoke(2.5f);
                yield return new WaitForSecondsRealtime(2.5f);
                CreatePedestalUnit(UnitType.Easy, freePoint);
            }
            else {
                yield return new WaitForSecondsRealtime(.5f);
            }
        }
    }

    private UnitPedestal CreatePedestalUnit(UnitType unitType, Transform point){
        var createdUnitPedestal = GetFreeUnitPedestal(unitType);
        createdUnitPedestal.transform.SetParent(point);
        createdUnitPedestal.transform.localPosition = Vector3.zero;
        createdUnitPedestal.transform.localEulerAngles = Vector3.zero;
        pointsAvailable[point] = createdUnitPedestal;
        createdUnitPedestal.Release();

        if (margeCountTutorial > 0 && selectedUnit == null){
            UnitPedestal nearUnit = null;
            float nearDist = -1;
            foreach(var unitKey in pointsAvailable.Keys){
                if (pointsAvailable[unitKey] == null) continue;
                if (createdUnitPedestal != pointsAvailable[unitKey] && pointsAvailable[unitKey].UnitType == unitType){
                    if (nearUnit == null){
                        nearUnit = pointsAvailable[unitKey];
                    }
                    
                    if (nearDist == -1) {
                        nearDist = Vector3.Distance(createdUnitPedestal.transform.position, pointsAvailable[unitKey].transform.position);
                    }
                    else if (nearDist > Vector3.Distance(createdUnitPedestal.transform.position, pointsAvailable[unitKey].transform.position)) {
                        nearDist = Vector3.Distance(createdUnitPedestal.transform.position, pointsAvailable[unitKey].transform.position);
                        nearUnit = pointsAvailable[unitKey];
                    }
                    //GamePanelController.EnableTutrialAction?.Invoke(true, createdUnitPedestal.transform.position, pointsAvailable[unitKey].transform.position);
                    //break;
                }
            }
            if (nearUnit != null){
                GamePanelController.EnableTutrialAction?.Invoke(true, createdUnitPedestal.transform.position + Vector3.up * 2f, nearUnit.transform.position + Vector3.up * 2f, MoveType.Marge);
            }
        }

        if (IsHero(unitType)){
            GamePanelController.EnableTutrialAction?.Invoke(true, createdUnitPedestal.transform.position, dependencyContainerSO.UnionBase.transform.position, default);
        }

        return createdUnitPedestal;
    }

    private Transform FreePoint(){
        
        foreach(var pointKey in pointsAvailable.Keys){
            if (pointsAvailable[pointKey] == null){
                return pointKey;
            }
        }

        return null;
    }
}
