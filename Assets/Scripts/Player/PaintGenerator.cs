using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class PaintGenerator : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private GameStorage gameStorageSO = default;
    [SerializeField] private DependencyContainer dependencyContainerSO = default;

    [Header("Settings")]
    [SerializeField] private Color colorAdd = default;
    [SerializeField] private Color colorRemove = default;

    [Header("Componets")]
    [SerializeField] private Slider paintSlider = default;
    [SerializeField] private Slider paintSliderSecond = default;
    [SerializeField] private TMP_Text paintSliderText = default;
    [SerializeField] private Image backgroundSliderImage = default;

    [Header("Action")]
    public static Action UpdatePaintValueAction = default;
    public static Action<float> StartReloadUnitAction = default;

    [Header("Strings")]
    [SerializeField] private string formatTextPaintSlider = "{0}<color=yellow>/</color>{1}";


    private void OnEnable()
    {
        //UpdatePaintValueAction += RecanculateSlider;
        //StateGenerator.StateCreatedAction += ReloadPaint;
        //StateGenerator.CreateStateAction += Prepare;

        StartReloadUnitAction += StartReloadUnit;

        paintSliderText.text = string.Format(formatTextPaintSlider, 0, paintSlider.maxValue);
        paintSliderSecond.value = 0;
        paintSlider.value = 0;

    }

    private void OnDisable()
    {
        //UpdatePaintValueAction -= RecanculateSlider;
        //StateGenerator.StateCreatedAction -= ReloadPaint;
        //StateGenerator.CreateStateAction -= Prepare;

         StartReloadUnitAction -= StartReloadUnit;
    }

    private void Prepare(StateType type)
    {
        dependencyContainerSO.PaintAvialableEnemy = 0;
        paintSlider.maxValue = 20;
        paintSlider.value = 0;
        paintSliderSecond.maxValue = 20;
        paintSliderSecond.value = 0;
        paintSliderText.text = string.Format(formatTextPaintSlider, 0, paintSlider.maxValue);
        paintSlider.onValueChanged.AddListener((_val) => {
            paintSliderText.text = string.Format(formatTextPaintSlider, dependencyContainerSO.PaintAvialable, paintSlider.maxValue);
        });
    }

    private void ReloadPaint()
    {
        Prepare(default);
        StopAllCoroutines();
        StartCoroutine(PaintGeneation());
    }

    private IEnumerator PaintGeneation()
    {
        while (dependencyContainerSO.InGame)
        {
            dependencyContainerSO.PaintAvialable++;
            dependencyContainerSO.PaintAvialableEnemy += 3;
            yield return new WaitForSecondsRealtime(gameStorageSO.GameBaseParameters.FillDuration);
        }
    }

    private void RecanculateSlider()
    {
        paintSliderSecond.DOKill();
        paintSlider.DOKill();
        if (dependencyContainerSO.PaintAvialable > paintSlider.value)
        {
            backgroundSliderImage.color = colorAdd;
            paintSliderSecond.value = dependencyContainerSO.PaintAvialable;
            paintSlider.DOValue(dependencyContainerSO.PaintAvialable, gameStorageSO.GameBaseParameters.SliderReactionDuration);
        }
        else
        {
            backgroundSliderImage.color = colorRemove;
            paintSlider.value = dependencyContainerSO.PaintAvialable;
            paintSliderSecond.DOValue(dependencyContainerSO.PaintAvialable, gameStorageSO.GameBaseParameters.SliderReactionDuration);
        }
    }

    private void StartReloadUnit(float duration){
        paintSlider.value = 0;
        paintSlider.DOValue(paintSlider.maxValue, duration)
            .SetEase(Ease.Flash)
            .OnComplete(() => paintSlider.value = 0);
    }
}
