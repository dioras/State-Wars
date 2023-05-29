using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;

public class GamePanelController : MonoBehaviour, IInterfacePanel
{
    [Header("Base")]
    [SerializeField] private UIPanelType uiPanelType = UIPanelType.Game;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private PlayerStorage playerStorageSO = default;
    [SerializeField] private StateStorage stateStorageSO = default;
    [SerializeField] private GroupingStorage groupingStorageSO = default;

    [Header("Settings")]
    [SerializeField] private Sprite[] reactions = default;

    [Header("ProgressBar components")]
    [SerializeField] private RectTransform controlBarContainer = default;
    [SerializeField] private Slider sliderProgressControlUnion = default;
    [SerializeField] private Slider sliderProgressControlEnemy = default;
    [SerializeField] private TMP_Text controlText = default;
    [SerializeField] private Image progressBarBackground = default;
    [SerializeField] private Image progressBarFillImage = default;
    [SerializeField] private Image teamImage01 = default;
    [SerializeField] private Image teamImage02 = default;

    [Header("Components")]
    [SerializeField] private Transform panelContainer = default;
    [SerializeField] private RectTransform rectReaction = default;
    [SerializeField] private Image reactionImage = default;
    [SerializeField] private TMP_Text unionCountText = default;
    [SerializeField] private TMP_Text enemyCountText = default;
    [SerializeField] private RectTransform paintContainer = default;
    [SerializeField] private EventTrigger eventTriggerPanelMover = default;

    [Header("Tutorial components")]
    [SerializeField] private RectTransform containerTutorial = default;
    [SerializeField] private RectTransform handTransform = default;
    [SerializeField] private RectTransform arrowRect = default;
    [SerializeField] private TMP_Text tutorialText = default;

    [Header("Strings")]
    [SerializeField] private string formatControlProgressString = "You control <color=yellow>{0}%</color> of the territory!";
    [SerializeField] private string stringTextMove = "HOLD & DRAG";
    [SerializeField] private string stringTextMarge = "MOVE TO MERGE";


    [Header("Actions")]
    public static Action ShowReactionAction = default;
    public static Action<GroupingType, int> AddUnitCountAction = default;
    public static Action<bool, Vector3, Vector3, MoveType> EnableTutrialAction = default;
    public static Action<bool> ShowTutorialArrwoAction = default;
    public static Action<Vector2> OnMovePanelAction = default;

    private Sequence sequenceReaction = default;
    private int maxCountHex = 0;
    private int unionCount = 0;
    private int enemyCount = 0;

    private void OnEnable()
    {
        ShowReactionAction += ShowReaction;
        AddUnitCountAction += AddUnitCount;
        EnableTutrialAction += EnableTutrial;
        ShowTutorialArrwoAction += ShowTutorialArrow;

        GameManager.LevelStartAction += OnGameStarted;
        StateGenerator.StateCreatedAction += ConfigProgressBar;
        PedestalController.LoadUnitsAction += ShowPaintContainer;
    }

    private void OnDisable()
    {
        ShowReactionAction -= ShowReaction;
        AddUnitCountAction -= AddUnitCount;
        EnableTutrialAction -= EnableTutrial;
        ShowTutorialArrwoAction -= ShowTutorialArrow;

        GameManager.LevelStartAction -= OnGameStarted;
        StateGenerator.StateCreatedAction -= ConfigProgressBar;
        PedestalController.LoadUnitsAction -= ShowPaintContainer;
    }

    private void Awake()
    {
        dependencyContainerSO.GamePanelController = this;
        EnableTutrial(false);
        ShowTutorialArrow(false);

        Init();
        PrepareButtons();
        HideReaction();

        sliderProgressControlEnemy.value = 0;
        sliderProgressControlUnion.value = 0;
    }

    private void PrepareButtons()
    {
        sliderProgressControlUnion.onValueChanged.AddListener((_value) => {
            UpdateControlText((int)(_value > 100 ? 100 : _value));
        });

        eventTriggerPanelMover.triggers.Clear();
        EventTrigger.Entry downMouse = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downMouse.callback.AddListener((eventData) => {

        });

        EventTrigger.Entry dragMouse = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragMouse.callback.AddListener((eventData) => {
            OnDragPanel();
        });

        eventTriggerPanelMover.triggers.Add(downMouse);
        eventTriggerPanelMover.triggers.Add(dragMouse);
    }

    private void OnDragPanel(){
        float deltaMoveX = 0;
        float deltaMoveY = 0;
        #if UNITY_EDITOR
            deltaMoveX = Input.GetAxis("Mouse X") * Time.deltaTime;
            deltaMoveY = Input.GetAxis("Mouse Y") * Time.deltaTime;
        #else
            if (Input.touchCount > 0){
                deltaMoveX = Input.touches[0].deltaPosition.x * .025f * Time.deltaTime;
                deltaMoveY = Input.touches[0].deltaPosition.y * .025f * Time.deltaTime;
            }
        #endif

        OnMovePanelAction?.Invoke(new Vector2(deltaMoveX, deltaMoveY));
    }

    private void ShowPaintContainer()
    {
        paintContainer.gameObject.SetActive(true);
    }

    private void HidePaintContainer()
    {
        paintContainer.gameObject.SetActive(false);
    }

    private void ReloadProgressBar()
    {
        UpdateControlText(0);
        sliderProgressControlUnion.value = 0;
        sliderProgressControlUnion.minValue = 0;
        sliderProgressControlUnion.maxValue = 100;

        var _enemyGroup = groupingStorageSO.GetGrouping(stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping);
        var _unionGroup = groupingStorageSO.GetGrouping(GroupingType.Union);

        progressBarBackground.color = _enemyGroup.Color;
        progressBarFillImage.color = _unionGroup.Color;
        teamImage01.color = _unionGroup.Color;
        teamImage02.color = _enemyGroup.Color;
    }

    private void UpdateControlText(int _val)
    {
        controlText.text = String.Format(formatControlProgressString, _val);
    }

    public void UpdateProgressBar()
    {
        if (maxCountHex != 0 && dependencyContainerSO.InGame)
        {
            var unionTerritory = 100f / maxCountHex * dependencyContainerSO.CountUnionHex;
            var enemyTerritory = 100f / maxCountHex * dependencyContainerSO.CountEnemyHex;

            sliderProgressControlUnion.value = unionTerritory;
            sliderProgressControlEnemy.value = enemyTerritory;

            /*if (dependencyContainerSO.InGame)
            {
                if (unionTerritory >= 85)
                {
                    UnitsController.KillEnemyUnitsAction?.Invoke(LevelResult.Win);
                    
                    //if (unionTerritory == 100)
                    //{
                        StateGenerator.FinishMapAction?.Invoke(LevelResult.Win);
                        dependencyContainerSO.InGame = false;
                        dependencyContainerSO.EnemyBase.ShowHealthBar(true);
                        UnitsController.FreezeAllUnitsAction?.Invoke(LevelResult.Win);
                        UnitsController.AttackBaseAction?.Invoke(dependencyContainerSO.EnemyBase, GroupingType.Union);
                    //}
                    //else
                    //{
                        dependencyContainerSO.EnemyBase.StopSpawnEnenmy();
                    //}
                    
                    dependencyContainerSO.HexGrid.SetAllHexCellColor(groupingStorageSO.GetGrouping(GroupingType.Union).Color, ControlType.Union);
                }
                else if (enemyTerritory >= 85)
                {
                    GameManager.LevelFinishAction?.Invoke(LevelResult.Lose);
                    UnitsController.KillEnemyUnitsAction?.Invoke(LevelResult.Lose);
                    UnitsController.FreezeAllUnitsAction?.Invoke(LevelResult.Lose);
                    //dependencyContainerSO.BaseControlledUnion = false;
                    //if (enemyTerritory == 100)
                    //{
                    //    StateGenerator.FinishMapAction?.Invoke(LevelResult.Lose);
                    //    dependencyContainerSO.InGame = false;
                    //    dependencyContainerSO.UnionBase.ShowHealthBar(true);
                    //    UnitsController.FreezeAllUnitsAction?.Invoke(LevelResult.Lose);
                    //    UnitsController.AttackBaseAction?.Invoke(dependencyContainerSO.UnionBase, stateStorageSO.Country.GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping);
                    //}
                }
            }*/
        }
    }

    private void ConfigProgressBar()
    {
        ReloadProgressBar();
        maxCountHex = dependencyContainerSO.HexGrid.GetAvialableHex();
    }

    private void OnGameStarted()
    {
        UIController.ShowUIPanelAloneAction(UIPanelType.Game);
    }

    private void ShowReaction()
    {
        sequenceReaction.Complete();

        rectReaction.gameObject.SetActive(true);
        rectReaction.localScale = Vector3.zero;
        reactionImage.sprite = reactions[UnityEngine.Random.Range(0, reactions.Length)];
        sequenceReaction = DOTween.Sequence();
        sequenceReaction.Append(rectReaction.transform.DOScale(1.3f, .4f));
        sequenceReaction.Append(rectReaction.transform.DOScale(1f, .2f));
        sequenceReaction.Join(rectReaction.transform.DOShakeRotation(.3f, new Vector3(0f, 5f, 90f), 10, 50f));
        sequenceReaction.OnComplete(() => {
            rectReaction.transform.DOScale(0f, .2f);
        });
    }

    private void HideReaction()
    {
        rectReaction.gameObject.SetActive(false);
    }

    private void AddUnitCount(GroupingType groupingType, int addCount)
    {
        unionCountText.transform.DOKill();
        enemyCountText.transform.DOKill();
        unionCountText.transform.localScale = Vector3.one;
        enemyCountText.transform.localScale = Vector3.one;

        if (groupingType == GroupingType.Union)
        {
            unionCountText.transform.DOShakeScale(.2f, new Vector3(0f, 1f, 0f), 10);
        }
        else
        {
            enemyCountText.transform.DOShakeScale(.2f, 1f, 6);
        }

        unionCount = 0;
        enemyCount = 0;
        dependencyContainerSO.UnitControllers.ForEach((_unit) => { 
            if (_unit.IsBusy)
            {
                if (_unit.GroupingType == GroupingType.Union)
                {
                    unionCount++;
                }
                else
                {
                    enemyCount++;
                }
            }
        });

        unionCountText.text = unionCount.ToString();
        enemyCountText.text = enemyCount.ToString();
    }

    private void EnableTutrial(bool _enable, Vector3 fromPos = default, Vector3 toPos = default, MoveType movetype = MoveType.Drop)
    {
        switch(movetype){
            case MoveType.Drop:
                tutorialText.text = stringTextMove;
                break;
            case MoveType.Marge:
                tutorialText.text = stringTextMarge;
                break;
        }
        ShowTutorialArrow(false);
        containerTutorial.gameObject.SetActive(_enable);
        if (_enable)
        {
            Vector2 viewportUnitPosition = Camera.main.WorldToScreenPoint(fromPos);
            Vector2 viewportBasePosition = Camera.main.WorldToScreenPoint(toPos);

            handTransform.DOKill();
            handTransform.position = viewportUnitPosition;
            handTransform.DOMove(viewportBasePosition, 2f).SetLoops(-1);
        }
    }

    private void ShowTutorialArrow(bool _show)
    {
        arrowRect.gameObject.SetActive(_show);
        arrowRect.DOKill();
        if (_show)
        {
            Vector2 viewportBasePosition = Camera.main.WorldToScreenPoint(dependencyContainerSO.UnionBase.transform.position);
            viewportBasePosition += new Vector2(0f, 100f);
            Vector2 startArrwoPosition = new Vector2(viewportBasePosition.x, viewportBasePosition.y + 100f);
            arrowRect.position = startArrwoPosition;
            arrowRect.DOMove(viewportBasePosition, .7f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    #region IInterfacePanel
    public UIPanelType UIPanelType { get => uiPanelType; }

    public void Hide()
    {
        panelContainer.gameObject.SetActive(false);
    }

    public void Show()
    {
        HidePaintContainer();
        panelContainer.gameObject.SetActive(true);
        ReloadProgressBar();

        unionCount = 0;
        enemyCount = 0;
        unionCountText.text = unionCount.ToString();
        enemyCountText.text = enemyCount.ToString();

        controlBarContainer.anchoredPosition = Vector2.up * 200f;
        controlBarContainer.DOAnchorPosY(0f, .25f);
    }

    public void Init()
    {
        UIController.InterfacePanels.Add(this);
    }
    #endregion
}

public enum MoveType{
    Drop,
    Marge
}
