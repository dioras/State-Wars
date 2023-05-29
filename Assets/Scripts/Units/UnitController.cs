using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Random = System.Random;

public class UnitController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private GroupingStorage groupingStorageSO = default;
    [SerializeField] private UnitsStorage unitsStorageSO = default;

    [Header("Settings")]
    [SerializeField] private bool isBusy = false;
    [SerializeField] private UnitType unitType = default;
    [SerializeField] private UnitState unitState = UnitState.Wait;
    [SerializeField] private GroupingType groupingType = default;
    private int unitHealth = 0;
    private int unitMaxHealth = 0;
    private float unitSpeed = 0f;
    private float unitDurationPainting = 0f;
    private float waitForShoot = 0;
    private int unitDamage = 0;
    private int unitPaintCellCount = 0;
    private FightType fightType = default;

    [Header("Components")]
    [SerializeField] private SkinnedMeshRenderer meshRenderer = default;
    [SerializeField] private List<MeshRenderer> meshRenderers = default;
    [SerializeField] private UnitAnimatorController animatorController = default;
    [SerializeField] private ParticleSystem paintingParticleSystem = default;
    [SerializeField] private ParticleSystemRenderer particleSystemRenderer = default;
    [SerializeField] private Transform unitBodyTransform = default;
    [SerializeField] private Collider unitBodyCollider = default;
    [SerializeField] private UnitSkinController unitSkinController = default;

    [Header("Health Bar")]
    [SerializeField] private Transform healthBarContainer = default;
    [SerializeField] private Slider healthSlider = default;

    private Vector3 defaultScale = default; 
    private Color groupingColor = default;
    private HexCell currencyCell = default;
    private HexCell targetCell = default;
    private List<HexCoordinates> hexPath = new List<HexCoordinates>();
    private Sequence moveSequence = default;
    private List<HexCell> reservedCell = new List<HexCell>();
    private int currencyPoint = 0;

    private Vector3 basePosition = default;
    private ControlType enemyCell = default;
    private ControlType unionControl = default;
    private CellState unionReserveState = default;

    private Coroutine _coroutineFindNextPath = default;

    private UnitController targetUnit = default;
    private static readonly int ColorStickmanString = Shader.PropertyToID("_Color");
    private static readonly int EmisColor = Shader.PropertyToID("_EmisColor");

    #region get/set
    public int UnitHealth { get => unitHealth; set => unitHealth = value; }
    public bool IsBusy { get => isBusy; set => isBusy = value; }
    public UnitType UnitType { get => unitType; set
        {
            unitType = value;
            UpdateUnitParams();
        }
    }
    public UnitState SetUnitState { set
        {
            unitState = value;
            UpdateUnitState();
        }
    }
    public UnitState GetUnitState => unitState;
    public GroupingType GroupingType { get => groupingType; set => groupingType = value; }
    public ParticleSystem PaintingParticleSystem => paintingParticleSystem;
    #endregion


    private void OnDisable()
    {
        moveSequence.Kill();
        StopAllCoroutines();
    }

    private void Awake() {
        defaultScale = unitBodyTransform.localScale;
    }

    private IEnumerator CheckUnitState()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSecondsRealtime(.5f);
            if (moveSequence == null && _coroutineFindNextPath == null)
            {
                _coroutineFindNextPath = StartCoroutine(WaitForNextStep());
            }
        }
    }

    private void LateUpdate()
    {
        if (targetUnit != null)
        {
            transform.LookAt(targetUnit.transform.position);
        }

        healthBarContainer.transform.LookAt(Camera.main.transform);
    }

    private void UpdateUnitParams()
    {
        var _unit = unitsStorageSO.GetUnit(unitType);
        
        unitHealth = _unit.UnitHealth;
        unitMaxHealth = unitHealth;
        healthSlider.maxValue = unitMaxHealth;
        healthSlider.value = unitMaxHealth;
        unitSpeed = _unit.UnitSpeed;
        unitDurationPainting = _unit.UnitDurationPainting;
        unitPaintCellCount = _unit.UnitPaintCellCount;
        fightType = _unit.FightType;
        waitForShoot = _unit.WaitForShoot;
        unitDamage = _unit.UnitDamage;
        
        paintingParticleSystem.Stop();
        animatorController.SetAnimation(UnitState.Wait);
    }

    private void UpdateUnitState()
    {
        switch (unitState)
        {
            case UnitState.Move:
                currencyCell = dependencyContainerSO.HexGrid.FindCurrencyCell(transform.position);
                MoveNext();
                break;
            case UnitState.Happy:
                moveSequence?.Kill();
                animatorController.SetAnimation(UnitState.Happy);
                break;
            case UnitState.Battle:
                animatorController.SetAnimation(UnitState.Painting);
                break;
        }
    }

    private void MoveNext()
    {
        HexCell targetCellFromPoint = FindNextHex();

        hexPath = dependencyContainerSO.HexGrid.Search(currencyCell, targetCellFromPoint, enemyCell);
        targetCell = targetCellFromPoint;

        if (hexPath.Count > 1)
        {
            var _preStepCell = dependencyContainerSO.HexGrid.GetCell(hexPath[hexPath.Count - 2]);
            List<HexCell> avialableCells = new List<HexCell>();
            avialableCells.Add(targetCell);
            for (int i = 0; i < _preStepCell.Neighbors.Length; i++)
            {
                if (_preStepCell.Neighbors[i] == null) continue;
                if (_preStepCell.Neighbors[i].IsEmpty) continue;
                if (_preStepCell.Neighbors[i] == currencyCell) continue;
                if (unionControl == ControlType.Enemys && _preStepCell.Neighbors[i].CellState == unionReserveState) continue;

                if (_preStepCell.Neighbors[i].ControlType != unionControl && _preStepCell.Neighbors[i].CellState != unionReserveState)
                {
                    avialableCells.Add(_preStepCell.Neighbors[i]);
                }
            }
            targetCell = avialableCells[UnityEngine.Random.Range(0, avialableCells.Count - 1)];
            hexPath = dependencyContainerSO.HexGrid.Search(currencyCell, targetCell, enemyCell);
        }

        //if (hexPath.Count == 0 || targetCell.IsEmpty)
        //{
        //    //animatorController.SetAnimation(UnitState.Looking);
        //    //var targetCell = dependencyContainerSO.HexGrid.FindNearUnionHex(transform.position, unionControl);
        //    //hexPath = dependencyContainerSO.HexGrid.Search(currencyCell, targetCell);
        //    //if (hexPath.Count == 0)
        //    //{
        //        StartCoroutine(WaitForNextStep());
        //    //}
        //    //else
        //    //{
        //    //    MoveToPath(false);
        //    //}
        //    return;
        //}

        if (hexPath.Count > 0)
        {
            ClearReservedCells();
            ReservedUnitCells();
            MoveToPath();
        }
        else
        {
            ClearReservedCells();
            _coroutineFindNextPath = StartCoroutine(WaitForNextStep());
        }
    }

    private void ReservedUnitCells()
    {
        if (reservedCell == null)
        {
            reservedCell = new List<HexCell>();
        }
        reservedCell.Clear();
        reservedCell.Add(targetCell);
        targetCell.CellState = unionReserveState;
        var _curIndex = 0;
        for (int i = 0; i < targetCell.Neighbors.Length; i++)
        {
            if (_curIndex == unitPaintCellCount - 1) break;
            if (targetCell.Neighbors[i] != null && targetCell.Neighbors[i].CellState == CellState.None && targetCell.Neighbors[i].ControlType != unionControl)
            {
                if (targetCell.Neighbors[i].IsEmpty) continue;
                targetCell.Neighbors[i].CellState = unionReserveState;
                reservedCell.Add(targetCell.Neighbors[i]);
                _curIndex++;
            }
        }
    }

    private HexCell FindNextHex()
    {
        Random _rand = new Random();
        int _randomDirection = _rand.Next(0, 10);
        
        if (_randomDirection == 0)
        {
            return dependencyContainerSO.HexGrid.FindNearNeutralHex(basePosition, unionControl, unionReserveState, currencyCell);
        }
        else
        {
            return dependencyContainerSO.HexGrid.FindNearNeutralHex(transform.position, unionControl, unionReserveState, currencyCell);
        }
    }

    private void MoveToPath(bool attack = true, bool findAfter = true, Action callBack = null)
    {
        if (moveSequence != null)
        {
            moveSequence.Kill();
            moveSequence = null;
        }
        animatorController.SetAnimation(UnitState.Wait);
        targetUnit = null;

        moveSequence = DOTween.Sequence();
        hexPath.Reverse();
        var _lastCell = dependencyContainerSO.HexGrid.GetCell(hexPath[hexPath.Count - 1]);
        currencyPoint = 0;
        paintingParticleSystem.Stop();
        foreach (var point in hexPath)
        {
            currencyPoint++;
            var cell = dependencyContainerSO.HexGrid.GetCell(point);
            moveSequence.Append(transform.DOLookAt(dependencyContainerSO.HexGrid.GetCell(hexPath[currencyPoint - 1]).transform.position + Vector3.up, .0f).SetEase(Ease.Flash));
            if (cell != _lastCell)
            {
                moveSequence.Append(transform.DOMove(new Vector3(cell.transform.position.x, transform.position.y, cell.transform.position.z), unitSpeed).SetEase(Ease.Flash).OnComplete(() =>
                {
                    currencyCell = cell;
                    if (attack)
                    {
                        if (currencyCell.ControlType != unionControl)
                        {
                            currencyCell.EnableHighlight(groupingColor, unitDurationPainting);
                            currencyCell.SetCellControl(unionControl, groupingColor);

                            GetDamage();
                        }
                    }
                }));
            }
            else
            {
                if (attack)
                {
                    moveSequence.AppendCallback(() =>
                    {
                        paintingParticleSystem.Play();
                        animatorController.SetAnimation(UnitState.Painting);
                        
                        reservedCell.ForEach((_reserveCell) =>
                        {
                            _reserveCell.SetCellControl(unionControl, groupingColor);
                        });
                        reservedCell.ForEach((_reserveCell) => _reserveCell.EnableHighlight(groupingColor, unitDurationPainting));
                    });
                    moveSequence.AppendInterval(unitDurationPainting);
                }

                moveSequence.Append(transform.DOMove(new Vector3(cell.transform.position.x, transform.position.y, cell.transform.position.z), unitSpeed)
                    .OnComplete(() =>
                    {
                        currencyCell = cell;
                    })
                    .OnStart(() =>
                    {
                        if (attack)
                        {
                            GetDamage();
                        }

                        animatorController.SetAnimation(UnitState.Move);
                        paintingParticleSystem.Stop();
                    }));
            }
        }
        moveSequence.OnComplete(() =>
        {
            if (findAfter)
            {
                _coroutineFindNextPath = StartCoroutine(WaitForNextStep());
            }
            callBack?.Invoke();
        });
        moveSequence.OnPlay(() => animatorController.SetAnimation(UnitState.Move));
        moveSequence.Play();
    }

    private bool CheckCorrectPath()
    {
        for (int i = 0; i < hexPath.Count; i++)
        {
            var _cell = dependencyContainerSO.HexGrid.GetCell(hexPath[i]);
            if (_cell.ControlType == enemyCell)
            {
                ClearReservedCells();
                moveSequence.Kill();
                moveSequence = null;
                _coroutineFindNextPath = StartCoroutine(WaitForNextStep());
                return false;
            }
        }

        return true;
    }

    public void GetDamage(int damage = 1)
    {
        unitHealth -= damage;
        if (unitHealth <= 0)
        {
            KillUnit();
        }
        else
        {
            if (gameObject.activeSelf)
            {
                //StartCoroutine(GetVisualDamage());
                healthSlider.DOValue(unitHealth, .3f);
            }
            else
            {
                ClearReservedCells();
            }
        }
    }

    private void CheckUnitHealth()
    {
        if (unitHealth > 0)
        {
            MoveNext();
        }
    }

    public void KillUnit()
    {
        ClearReservedCells();

        unitState = UnitState.Wait;
        if (targetUnit != null)
        {
            targetUnit.targetUnit = null;
        }
        targetUnit = null;
        if (IsBusy)
        {
            GamePanelController.AddUnitCountAction?.Invoke(groupingType, -1);
        }
        ParticleController.PlayParticleAction?.Invoke(transform.position, ParticleType.UnitDestroy);
        VibrationController.Vibrate(15);
        transform.DOScale(0f, .3f).OnComplete(() => {
            gameObject.SetActive(false);
            isBusy = false;
        });
    }

    public void ClearReservedCells()
    {
        moveSequence?.Kill();
        moveSequence = null;
        //targetUnit = null;
        StopAllCoroutines();
        if (gameObject.activeSelf)
        {
            StartCoroutine(CheckUnitState());
        }

        SetUnitState = UnitState.Looking;

        if (reservedCell != null)
        {
            reservedCell.ForEach((_reservCell) => {
                if (_reservCell.CellState != CellState.None /*&& _reservCell.ControlType == ControlType.Neutral*/)
                {
                    //_reservCell.ControlType = ControlType.Neutral;
                    _reservCell.CellState = CellState.None;
                }
            });
            //reservedCell.Clear();
        }
    }

    public void Config()
    {
        moveSequence?.Kill();
        unitBodyTransform.DOKill();
        var _grouping = groupingStorageSO.GetGrouping(groupingType);
        groupingColor = _grouping.Color;
        transform.GetChild(0).tag = groupingType == GroupingType.Union ? Tags.Unit : Tags.UnitEnemy;
        if (meshRenderer != null)
        {
            meshRenderer.material.DOKill();
            meshRenderer.material.SetColor(ColorStickmanString, _grouping.ColorStickmans);
        }
        else
        {
            meshRenderers.ForEach((_mesh) =>
            {
                _mesh.material.DOKill();
                _mesh.material.SetColor(ColorStickmanString, _grouping.ColorStickmans);
            });
        }

        particleSystemRenderer.material.SetColor(EmisColor, groupingColor);
        gameObject.SetActive(true);
        unitBodyTransform.localScale = defaultScale;
        UpdateUnitParams();

        targetUnit = null;

        basePosition = groupingType == GroupingType.Union ? dependencyContainerSO.UnionBase.transform.position : dependencyContainerSO.EnemyBase.transform.position;
        enemyCell = groupingType == GroupingType.Union ? ControlType.Enemys : ControlType.Union;
        unionControl = groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys;
        unionReserveState = groupingType == GroupingType.Union ? CellState.ReservedUnion : CellState.ReservedEnemy;

        StartCoroutine(CheckUnitState());
        StartCoroutine(EnemyCheckerInRange());

        unitSkinController?.SetUnitSet(groupingType == GroupingType.Union);
    }

    private void StartBattle(UnitController toUnit)
    {
        if (unitState == UnitState.BaseAttack) return;
        
        targetUnit = toUnit;
        paintingParticleSystem.Play();

        //transform.LookAt(toUnit.transform.position + Vector3.one);
        StartCoroutine(fightType.Equals(FightType.ShortRange) ? WaitBattleShort() : WaitBattleLong());
    }

    private IEnumerator WaitBattleShort()
    {
        while (targetUnit != null && unitHealth > 0 && targetUnit.unitHealth > 0)
        {
            if (targetUnit.gameObject.activeSelf == false) break;
            if (Vector3.Distance(transform.position, targetUnit.transform.position) > 6f) break;
            yield return new WaitForSecondsRealtime(.3f);
            if (targetUnit != null)
            {
                targetUnit.GetDamage(unitDamage);
            }
        }

        targetUnit = null;
        ClearReservedCells();
        _coroutineFindNextPath = StartCoroutine(WaitForNextStep());
    }
    
    private IEnumerator WaitBattleLong()
    {
        while (targetUnit != null && unitHealth > 0 && targetUnit.unitHealth > 0)
        {
            if (targetUnit.gameObject.activeSelf == false) break;
            yield return new WaitForSecondsRealtime(waitForShoot);
            if (targetUnit != null)
            {
                if (Vector3.Distance(transform.position, targetUnit.transform.position) > 12f) break;
                BulletsController.ShootBulletAction?.Invoke(transform.position, targetUnit.transform.position, .3f, groupingType,
                    () =>
                    {
                        if (targetUnit != null)
                        {
                            targetUnit.GetDamage(unitDamage);
                        }
                    });
            }
        }

        targetUnit = null;
        ClearReservedCells();
        _coroutineFindNextPath = StartCoroutine(WaitForNextStep());
    }

    private IEnumerator WaitForNextStep()
    {
        animatorController.SetAnimation(UnitState.Looking);
        paintingParticleSystem.Stop();
        yield return new WaitForSecondsRealtime(unitsStorageSO.TimeToFindNewPath);
        //currencyCell = dependencyContainerSO.HexGrid.FindCurrencyCell(transform.position);
        MoveNext();
    }

    private IEnumerator GetVisualDamage()
    {
        meshRenderer.material.DOColor(Color.white, 2f);
        yield return new WaitForSecondsRealtime(2f / unitMaxHealth);
        meshRenderer.material.DOKill();
    }

    public void AttackEnemyBase(BaseController baseController)
    {
        var pointToMove = baseController.GetCellNearToBase();
        currencyCell = dependencyContainerSO.HexGrid.FindCurrencyCell(transform.position);
        hexPath = dependencyContainerSO.HexGrid.HardSearch(currencyCell, pointToMove);
        if (hexPath != null && hexPath.Count > 0)
        {
            MoveToPath(false, false, () =>
            {
                RotateToBase(baseController);
            });
        }
        else
        {
            RotateToBase(baseController);
        }
    }

    private void RotateToBase(BaseController _base)
    {
        transform.LookAt(_base.transform);
        paintingParticleSystem.Play();
        animatorController.SetAnimation(UnitState.Painting);
        unitState = UnitState.BaseAttack;
        StartCoroutine(DamageBase(_base));
        _base.ShowHealthBar(true);
    }

    private IEnumerator DamageBase(BaseController _base)
    {
        while (_base.Health > 0 && unitHealth > 0)
        {
            yield return new WaitForSecondsRealtime(.7f);
            _base.GetDamage(10);
            GetDamage(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(groupingType == GroupingType.Union ? Tags.UnitEnemy : Tags.Unit))
        {
            if (unitState == UnitState.Battle || targetUnit != null || unitState == UnitState.BaseAttack) return;
            targetUnit = other.transform.parent.GetComponent<UnitController>();
            if (targetUnit != null)
            {
                ClearReservedCells();
                SetUnitState = UnitState.Battle;
                StartBattle(targetUnit);
            }
        }
    }

    private IEnumerator EnemyCheckerInRange()
    {
        while (unitHealth > 0 && dependencyContainerSO.InGame)
        {
            yield return new WaitForSeconds(.5f);
            if (targetUnit == null)
            {
                var res = FindNearEnemyUnit(5f);
                if (res != null)
                {
                    OnTriggerEnter(res.unitBodyCollider);
                }
            }
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
}
