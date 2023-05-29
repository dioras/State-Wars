using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupingSliderObject : MonoBehaviour
{
    [SerializeField] private Image fillImage = default;
    [SerializeField] private Image groupIcon = default;
    [SerializeField] private Slider slider = default;
    [SerializeField] private TMP_Text textProgress = default;

    public Image FillImage => fillImage;
    public Image GroupIcon => groupIcon;
    public Slider Slider => slider;
    public TMP_Text TextProgress => textProgress;
}
