using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TurrelController : MonoBehaviour
{
    [Header("Base")] 
    [SerializeField] private GroupingStorage groupingStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private UnitsStorage unitsStorageSO = default;

    [Header("Settings")] 
    [SerializeField] private bool isDestroyed = false;
    [SerializeField] private float range = 10f;
    [SerializeField] private int damageForce = 3;
    
    [Header("Components")]
    [SerializeField] private MeshRenderer circleMeshRenderer = default;
    private UnitController unitTarget = default;
    private Transform cellTarget = default;
    [SerializeField] private Transform axisTargetRotate = default;
    [SerializeField] private Transform shootPoint = default;
    [SerializeField] private Transform gunTransform = default;
    [SerializeField] private DestructionObject destructionObject = default;
    [SerializeField] private MeshRenderer changeMeshRenderer = default;

    [Header("Health bar")] 
    [SerializeField] private Transform healthBarContainer = default;
    [SerializeField] private Slider healthSlider = default;

    private int health = 0;
    private GroupingType groupingType = default;
    private HexCell parentCell = default;
    private List<HexCell> cellsInRange = new List<HexCell>();
    
    #region  get/sets
    public bool IsBusy { get; set; }
    public GroupingType GroupingType { get; set; }
    public int TurrelHealth {
        set
        {
            health = value <= 0 ? 0 : value;
            if (health == 0)
            {
                DestroyTurrel();
            }
            healthSlider.DOValue(health, .3f);
        }
        get
        {
            return health;
        }
    }
    #endregion

    private void LateUpdate()
    {
        healthBarContainer.LookAt(Camera.main.transform);

        if (unitTarget != null)
        {
            axisTargetRotate.LookAt(unitTarget.transform.position + Vector3.up);
        }
        else if (cellTarget != null)
        {
            axisTargetRotate.LookAt(cellTarget.position + Vector3.up);
        }
        else
        {
            gunTransform.Rotate(Vector3.up * 20f * Time.deltaTime);
        }
    }

    public void Config(GroupingType groupingType, HexCell parentCell)
    {
        parentCell.ObjectInCell = this;
        healthBarContainer.gameObject.SetActive(true);
        
        StopAllCoroutines();
        cellsInRange.Clear();

        isDestroyed = false;
        //cirecleTurrel.gameObject.SetActive(true);
        destructionObject.Restore();
        
        var turrel = unitsStorageSO.Turrel;
        healthSlider.maxValue = turrel.TurrelHealth;
        TurrelHealth = turrel.TurrelHealth;

        this.groupingType = groupingType;
        this.parentCell = parentCell;
        
        changeMeshRenderer.material.SetColor("_Color", groupingStorageSO.GetGrouping(groupingType).ColorStickmans);

        //cirecleTurrel.DOKill();
        //cirecleTurrel.DORotate(Vector3.up * 360, 4f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Flash);
        //SetCircleColor();
        StartCoroutine(FindTarget());
        StartCoroutine(DamageTarget());
    }

    private void SetCircleColor()
    {
        circleMeshRenderer.material.SetColor("_Color", groupingStorageSO.GetGrouping(groupingType).ColorStickmans);
    }

    private void DestroyTurrel()
    {
        if (isDestroyed == false)
        {
            healthBarContainer.gameObject.SetActive(false);
            parentCell.ObjectInCell = null;
            cellsInRange.ForEach((_cell) => _cell.ObjectInCell = null);
            isDestroyed = true;
            destructionObject.Broke();
            //cirecleTurrel.gameObject.SetActive(false);
            StartCoroutine(WaitForTurrelDisabled());
        }
    }

    private IEnumerator WaitForTurrelDisabled()
    {
        yield return new WaitForSecondsRealtime(5f);
        gameObject.SetActive(false);
        IsBusy = false;
    }

    private IEnumerator DamageTarget()
    {
        while (gameObject.activeSelf && dependencyContainerSO.InGame && isDestroyed == false)
        {
            if (unitTarget != null)
            {
                if (Vector3.Distance(transform.position, unitTarget.transform.position) < range)
                {
                    RotateTurrelToTarget(.2f, () =>
                    {
                        if (unitTarget != null)
                        {
                            TurrelHealth -= unitsStorageSO.Turrel.AmountPaintShot;
                            BulletsController.ShootBulletAction?.Invoke(shootPoint.position,
                                unitTarget.transform.position, .2f, groupingType,
                                () =>
                                {
                                    if (unitTarget != null)
                                    {
                                        unitTarget.GetDamage(damageForce);
                                    }
                                });
                        }
                    });
                }
                else
                {
                    unitTarget = null;
                }
            }
            
            yield return new WaitForSecondsRealtime(.25f);

            bool cellsAvialableInRange = false;
            cellsInRange.ForEach((_cell) =>
            {
                if (_cell.ControlType == (groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys))
                {
                    cellsAvialableInRange = true;
                }
            });
            if (cellsAvialableInRange == false)
            {
                break;
            }
        }
        DestroyTurrel();
    }

    private IEnumerator FindTarget()
    {
        while (gameObject.activeSelf && dependencyContainerSO.InGame && isDestroyed == false)
        {
            if (unitTarget == null || unitTarget.IsBusy == false)
            {
                unitTarget = FindNearEnemyUnit(range);
                if (unitTarget == null)
                {
                    ShootToEnemyCell();
                }
            }
            yield return new WaitForSecondsRealtime(.5f);
        }
        DestroyTurrel();
    }

    private void ShootToEnemyCell()
    {
        var unionCntrol = groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys;
        var suffleCells = DependencyContainer.Shuffle(cellsInRange);
        
        var enemyCell = suffleCells.Find((_cell) =>
            _cell.ControlType != unionCntrol);
        if (enemyCell != null && enemyCell.transform != cellTarget)
        {
            cellTarget = enemyCell.transform;
            RotateTurrelToTarget(.2f, () =>
            {
                TurrelHealth -= unitsStorageSO.Turrel.AmountPaintShot;
                BulletsController.ShootBulletAction?.Invoke(shootPoint.position, enemyCell.transform.position, .4f, groupingType,
                    () =>
                    {
                        cellTarget = null;
                        enemyCell.SetCellControl(unionCntrol, groupingStorageSO.GetGrouping(groupingType).Color);
                        enemyCell.EnableHighlight(groupingStorageSO.GetGrouping(groupingType).Color, .2f);
                    });
            });
        }
    }

    private void RotateTurrelToTarget(float duration, Action callback)
    {
        var roatateTarget = new Vector3(0f, axisTargetRotate.eulerAngles.y, 0f); 
        gunTransform.DORotate(roatateTarget, duration).OnComplete(() => callback?.Invoke())
            .SetEase(Ease.Flash);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(Tags.Cell))
        {
            var cell = other.GetComponent<HexCell>();
            if (cell.IsEmpty == false)
            {
                cell.SetCellControl(groupingType == GroupingType.Union ? ControlType.Union : ControlType.Enemys,
                    groupingStorageSO.GetGrouping(groupingType).Color);
                cell.EnableHighlight(groupingStorageSO.GetGrouping(groupingType).Color, .3f, true);
                cell.ObjectInCell = this;
                if (cellsInRange.Contains(cell) == false)
                {
                    cellsInRange.Add(cell);
                }
            }
        }
    }
}
