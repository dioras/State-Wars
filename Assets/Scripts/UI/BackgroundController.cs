using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BackgroundController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private Color colorDrak = default;

	[Header("Components")]
	[SerializeField] private RectTransform panelContainer = default;
	[SerializeField] private Image backgroundImage = default;

	[Header("Actions")]
	public static Action<bool, Action> DarkenAction = default;

	private Color defaultColor = new Color(0f, 0f, 0f, 0f);

    private void OnEnable()
    {
		DarkenAction += Darken;
		UIController.ShowUIPanelAloneAction += ReactPanel;
	}

    private void OnDisable()
    {
		DarkenAction -= Darken;
		UIController.ShowUIPanelAloneAction -= ReactPanel;
	}

	private void ReactPanel(UIPanelType _uIPanelType)
	{
		switch (_uIPanelType)
		{
			case UIPanelType.None:
				Show();
				break;
			default:
				Hide();
				break;
		}
	}

	private void Darken(bool _dark, Action _callback)
    {
		backgroundImage.DOColor(_dark ? colorDrak : defaultColor, .7f).OnComplete(() => _callback?.Invoke());
	}

	private void Show()
    {
		panelContainer.gameObject.SetActive(true);
    }

	private void Hide()
    {
		panelContainer.gameObject.SetActive(false);
		backgroundImage.color = defaultColor;
	}
}
