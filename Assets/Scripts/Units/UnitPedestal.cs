using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Collider))]
public class UnitPedestal : MonoBehaviour
{
    [SerializeField] private int materialChangeIndex = default;
    [SerializeField] private SkinnedMeshRenderer meshRenderer = default;
    [SerializeField] private List<MeshRenderer> defaultMeshRenderer = default;
    [SerializeField] private int idleIndex = 0;
    [SerializeField] private Animator animator = default;
    [SerializeField] private TMP_Text unitLevelText = default;
    [SerializeField] private Transform bodyTransform = default;
    [SerializeField] private UnitSkinController unitSkinController = default;

    public UnitSet CurrencySet { get; set; }

    private float defaultY = default;

    private UnitType unitType = default;
    public UnitType UnitType { 
        get { return unitType; }
        set { 
            unitType = value;
            unitLevelText.text = "LVL. " + GetLevelFromType();
        }
    }
    public Collider Collider { get; set; }
    public bool IsBusy { get; set; }
    //public SkinnedMeshRenderer MeshRenderer => meshRenderer;
    public Color SetStickmanColor
    {
        set
        {
            if (meshRenderer != null)
            {
                meshRenderer.materials[materialChangeIndex].SetColor("_Color", value);
            }
            else
            {
                defaultMeshRenderer.ForEach((_mesh) => _mesh.materials[materialChangeIndex].SetColor("_Color", value));
            }
        }
    }

    private void Awake()
    {
        Collider = GetComponent<Collider>();
        if (animator != null)
        {
            animator.SetInteger("Idle", idleIndex);
        }

        defaultY = bodyTransform.localPosition.y;
    }

    private string GetLevelFromType(){
        switch(unitType){
            case UnitType.Easy:
                return "1";
            case UnitType.Hard:
                return "2";
            case UnitType.Tank:
                return "3";
            default:
                return "<color=red>MAX.</color>";
        }
    }

    public void Reserve(){
        gameObject.SetActive(false);
        IsBusy = false;
    }

    public void Release(){
        CurrencySet = unitSkinController?.SetUnitSet(true);
        Collider.enabled = true;
        gameObject.SetActive(true);
        IsBusy = true;
    }

    public void SelectedToMarge(){
        bodyTransform.DOLocalMoveY(defaultY + 1.5f, .5f);
        transform.DOComplete();
        transform.DOShakeScale(.5f, .3f).OnComplete(() => transform.localScale = Vector3.one * 2f);
    }

    public void DeselectedToMare(){
        bodyTransform.DOLocalMoveY(defaultY, .5f);
    }
}
