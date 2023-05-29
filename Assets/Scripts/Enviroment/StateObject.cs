using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(MeshRenderer))]
public class StateObject : MonoBehaviour
{
    [SerializeField] private StateType stateType = default;
    [SerializeField] private Image imageState = default;
    private MeshRenderer meshRenderer = default;

    public StateType StateType => stateType;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Config(Color color, Sprite icon, Color iconColor)
    {
        meshRenderer.material.SetColor("_Color", color);
        imageState.sprite = icon;
        imageState.color = iconColor;
    }

    public void Pulse(bool pulse)
    {
        imageState.transform.DOKill();
        imageState.transform.DOScale(1f, .2f).OnComplete(() => {
            if (pulse)
            {
                imageState.transform.DOScale(1.65f, .5f).SetLoops(-1, LoopType.Yoyo);
            }
        });
    }
}
