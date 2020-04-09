using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIGroupEffects : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	private Toggle _toggle;

	public List<UIBehaviour> selectables;

	public List<ColorList> toColor;

	public List<ScaleList> toScale;

	public bool toggleOnOver = true;

	public bool untoggleOnExit;

	public HightlightAnimate highlight;

	public RectTransform toHighlight;

	public Toggle toggle
	{
		get
		{
			if ((Object)(object)_toggle == null)
			{
				_toggle = GetComponent<Toggle>();
			}
			return _toggle;
		}
	}

	private void Awake()
	{
		foreach (UIBehaviour selectable in selectables)
		{
			((MonoBehaviour)(object)selectable).AddUnityEvent((EventTriggerType)9, OnSelect);
		}
		((UnityEvent<bool>)(object)toggle.onValueChanged).AddListener((UnityAction<bool>)OnValueChanged);
	}

	private void OnEnable()
	{
		OnValueChanged(toggle.get_isOn());
	}

	private void OnValueChanged(bool isOn)
	{
		if (base.isActiveAndEnabled)
		{
			Color(isOn);
			Scale(isOn);
		}
		if (!(toHighlight != null))
		{
			return;
		}
		if (isOn)
		{
			if (highlight.isActiveAndEnabled)
			{
				highlight.Highlight(toHighlight);
			}
		}
		else if (highlight != null)
		{
			highlight.Deactivate();
		}
	}

	private void Color(bool isOn)
	{
		if (Application.isPlaying)
		{
			foreach (ColorList item in toColor)
			{
				foreach (Graphic item2 in item.toScale)
				{
					item2.CrossFadeColor((!isOn) ? ((ColorBlock)(ref item.color)).get_normalColor() : ((ColorBlock)(ref item.color)).get_highlightedColor(), ((ColorBlock)(ref item.color)).get_fadeDuration(), true, true);
				}
			}
		}
	}

	private void Scale(bool isOn)
	{
		if (Application.isPlaying)
		{
			foreach (ScaleList item in toScale)
			{
				Vector2 vector = (!isOn) ? item.scale.normalScale : item.scale.highlightedScale;
				Vector3 vector2 = new Vector3(vector.x, vector.y, 1f);
				foreach (RectTransform item2 in item.toScale)
				{
					ShortcutExtensions.DOScale((Transform)item2, vector2, item.scale.duration);
				}
			}
		}
	}

	private void OnSelect(BaseEventData arg0)
	{
		toggle.set_isOn(true);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (toggleOnOver && !eventData.get_eligibleForClick() && ((Behaviour)(object)toggle).enabled)
		{
			toggle.set_isOn(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (untoggleOnExit && !eventData.get_eligibleForClick() && ((Behaviour)(object)toggle).enabled)
		{
			toggle.set_isOn(false);
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
	}
}
