using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StateGenerator : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private PlayerStorage playerStorageSO = default;
    [SerializeField] private StateStorage stateStorageSO = default;
    [SerializeField] private GroupingStorage groupingStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private UnitsStorage unitsStorageSO = default;

    [Header("Settings")]
    [SerializeField] private float speedRotationFinishCamera = default;
    [SerializeField] private Vector3 centerMapCameraPosition = new Vector3(20, 0, 25);

    [Header("Components")]
    [SerializeField] private GameObject basePrefab = default;
    [SerializeField] private Transform cameraPosition = default;
    [SerializeField] private Transform cameraPositionBase = default;
    [SerializeField] private Transform pointSpawnState = default;

    [SerializeField] private Transform centerOfMap = default;
    [SerializeField] private Transform finishCameraPosition = default;
    [SerializeField] private Transform cameraPositionTarget = default;
    [SerializeField] private Transform sightPoint = default;

    [Header("Actions")]
    public static Action<StateType> CreateStateAction = default;
    public static Action StateCreatedAction = default;
    public static Action<bool> ShowPedestalAction = default;
    public static Action<LevelResult> FinishMapAction = default;

    private BaseController base01 = default;
    private BaseController base02 = default;
    private GameObject currencyStateSight = default;
    private StateLevelObject currencyStateTemplate = default;

    private Coroutine coroutineFinish = default;
    private Vector3 movePointStartPosition = default;
    private Vector3 movePointStartEuler = default;

    private void OnEnable()
    {
        CreateStateAction += CreateState;
        ShowPedestalAction += ShowPedestal;

        GameManager.LevelFinishAction += FinishMap;
        FinishMapAction += ShowBase;
        PlayerMapController.ShowPlayerMapAction += ShowBase;
        PlayerMapController.ShowPlayerMapAction += CanceledFinishScenario;

        GamePanelController.OnMovePanelAction += MoveCameraTarget;
    }

    private void OnDisable()
    {
        CreateStateAction -= CreateState;
        ShowPedestalAction -= ShowPedestal;

        GameManager.LevelFinishAction -= FinishMap;
        FinishMapAction -= ShowBase;
        PlayerMapController.ShowPlayerMapAction -= ShowBase;
        PlayerMapController.ShowPlayerMapAction -= CanceledFinishScenario;

        GamePanelController.OnMovePanelAction -= MoveCameraTarget;
    }

    private void Awake()
    {
        movePointStartPosition = cameraPositionTarget.localPosition;
        movePointStartEuler = cameraPositionTarget.localEulerAngles;

        CreateBase();
    }

    private void Start(){
        ShowPedestal(false);
    }

    private void LateUpdate()
    {
        centerOfMap.Rotate(centerOfMap.up * speedRotationFinishCamera * Time.deltaTime);
    }

    private void CreateBase()
    {
        base01 = Instantiate(basePrefab, transform).GetComponent<BaseController>();
        base02 = Instantiate(basePrefab, transform).GetComponent<BaseController>();

        base01.gameObject.SetActive(false);
        base02.gameObject.SetActive(false);
    }

    private void CreateState(StateType stateType)
    {
        cameraPositionTarget.localPosition = movePointStartPosition;
        cameraPositionTarget.localEulerAngles = movePointStartEuler;


        base01.gameObject.SetActive(false);
        base02.gameObject.SetActive(false);

        var _stateSetting = stateStorageSO.GetStateSettings(stateType);
        if (currencyStateTemplate != null)
        {
            Destroy(currencyStateTemplate.gameObject);
        }
        currencyStateTemplate = Instantiate(_stateSetting.StatePrefab.gameObject, pointSpawnState).GetComponent<StateLevelObject>();
        currencyStateTemplate.transform.localPosition = _stateSetting.StatePosition;
        currencyStateTemplate.transform.localScale = _stateSetting.ScaleScale;
        currencyStateTemplate.transform.eulerAngles = _stateSetting.EulerRotation;
        SetCurrencyStateSight(_stateSetting.StateSight, currencyStateTemplate.PointSight);

        centerOfMap.position = new Vector3(currencyStateTemplate.transform.position.x, centerOfMap.position.y, currencyStateTemplate.transform.position.z);
        centerMapCameraPosition = centerOfMap.position;

        dependencyContainerSO.CountUnionHex = 0;
        dependencyContainerSO.CountEnemyHex = 0;

        currencyStateTemplate.transform.DOMoveY(-10f, 2f).OnComplete(()=> {

            base01.gameObject.SetActive(true);
            base02.gameObject.SetActive(true);

            base01.transform.position = new Vector3(currencyStateTemplate.PointBase01.transform.position.x, 10f, currencyStateTemplate.PointBase01.transform.position.z);
            base01.transform.eulerAngles = currencyStateTemplate.PointBase01.transform.eulerAngles;
            base02.transform.position = new Vector3(currencyStateTemplate.PointBase02.transform.position.x, 10f, currencyStateTemplate.PointBase02.transform.position.z);
            base02.transform.eulerAngles = currencyStateTemplate.PointBase02.transform.eulerAngles;

            base01.transform.DOScale(new Vector3(1f, .3f, 1f), .1f);
            base02.transform.DOScale(new Vector3(1f, .3f, 1f), .1f);

            base01.transform.DOMoveY(0f, .3f)
                        .SetEase(Ease.Flash)
                        .OnComplete(() => {
                            base01.transform.DOScale(1.3f, .1f).OnComplete(() => base01.transform.DOScale(1f, .5f));
                            ParticleController.PlayParticleAction?.Invoke(base01.transform.position + new Vector3(0f, .5f, 0f), ParticleType.PlacedBase);
                            VibrationController.Vibrate(30);
                            StateCreatedAction?.Invoke();
                        });
            base02.transform.DOMoveY(0f, .3f)
                        .SetEase(Ease.Flash)
                        .OnComplete(() => {
                            base02.transform.DOScale(1.3f, .1f).OnComplete(() => base02.transform.DOScale(1f, .5f));
                            ParticleController.PlayParticleAction?.Invoke(base02.transform.position + new Vector3(0f, .5f, 0f), ParticleType.PlacedBase);
                            VibrationController.Vibrate(30);
                            StateCreatedAction?.Invoke();
                        });

            base01.GroupingType = GroupingType.Union;
            dependencyContainerSO.BaseControlledUnion = true;
            base01.BaseOccupioed = false;
            base01.transform.tag = Tags.UnionBase;
            dependencyContainerSO.UnionBase = base01;
            base02.GroupingType = groupingStorageSO.GetGrouping(stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping).Type;
            base02.BaseOccupioed = false;
            dependencyContainerSO.EnemyBase = base02;

            ShowPedestal(true);
            //Destroy(currencyStateTemplate.gameObject);

            //GamePanelController.EnableTutrialAction?.Invoke(true);
        });
        
        CameraController.SetOrthographicAction?.Invoke(false);
        //CameraController.SetCameraPositionAction?.Invoke(cameraPosition.position, cameraPosition.eulerAngles);
        CameraController.SetTargetAction?.Invoke(cameraPositionTarget);
        HexGrid.CreateEmptyMapAction?.Invoke();

        dependencyContainerSO.PaintAvialable = unitsStorageSO.GetUnit(UnitType.Easy).UnitPrice;
    }

    private void SetCurrencyStateSight(GameObject prefab, Transform pointSpawn)
    {
        if (currencyStateSight != null)
        {
            Destroy(currencyStateSight);
        }
        currencyStateSight = Instantiate(prefab, sightPoint);
        currencyStateSight.transform.localPosition = Vector3.zero;
        currencyStateSight.transform.localEulerAngles = Vector3.zero;
        //currencyStateSight.transform.position = new Vector3(pointSpawn.position.x, 0f, pointSpawn.position.z);
        //currencyStateSight.transform.eulerAngles = pointSpawn.eulerAngles;
    }

    private void ShowPedestal(bool _show)
    {
        dependencyContainerSO.PedestalContainer.gameObject.SetActive(_show);
        if (_show)
        {
            PedestalController.LoadUnitsAction?.Invoke();
        }
    }

    private void ShowBase(bool _show)
    {
        base01.gameObject.SetActive(!_show);
        base02.gameObject.SetActive(!_show);

        currencyStateSight?.SetActive(!_show);
    }

    private void FinishMap(LevelResult levelResult)
    {
        coroutineFinish = StartCoroutine(FinishScenario(levelResult.Equals(LevelResult.Win) ? dependencyContainerSO.EnemyBase.transform : dependencyContainerSO.UnionBase.transform));
        UnitsController.FreezeAllUnitsAction?.Invoke(levelResult);
    }

    private IEnumerator FinishScenario(Transform camTarget)
    {
        centerOfMap.position = camTarget.position;
        CameraController.SetTargetAction?.Invoke(cameraPositionBase);
        yield return new WaitForSecondsRealtime(3f);
        centerOfMap.position = centerMapCameraPosition;
        CameraController.SetTargetAction?.Invoke(finishCameraPosition);
    }

    private void ShowBase(LevelResult levelResult)
    {
        StateGenerator.ShowPedestalAction?.Invoke(false);
        UIController.ShowUIPanelAloneAction?.Invoke(UIPanelType.None);
        var _point = levelResult.Equals(LevelResult.Win) ? dependencyContainerSO.EnemyBase.transform : dependencyContainerSO.UnionBase.transform;
        centerOfMap.position = _point.position;
        CameraController.SetTargetAction?.Invoke(cameraPositionBase);
    }

    private void CanceledFinishScenario(bool val)
    {
        if (coroutineFinish != null)
        {
            StopCoroutine(coroutineFinish);
            coroutineFinish = null;
        }
    }

    private void MoveCameraTarget(Vector2 movePosition){
        if (dependencyContainerSO.PedestalContainer.SelectedUnit == null){
            var newPosition = cameraPositionTarget.localPosition + new Vector3(movePosition.x, 0f, movePosition.y) * 20f;
            cameraPositionTarget.localPosition = new Vector3(Mathf.Clamp(newPosition.x, 14.5f, 50f), cameraPositionTarget.localPosition.y, Mathf.Clamp(newPosition.z, 45.5f, 100f));
        }
    }
}
