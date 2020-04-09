using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleEffects : MonoBehaviour, ISubmitHandler, IPointerClickHandler, ISelectHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
{
	public List<Graphic> toColor;

	public ColorBlock color = MordheimColorBlock.defaultColorBlock;

	public List<RectTransform> toScale;

	public ScaleBlock scale = ScaleBlock.defaultScaleBlock;

	public GameObject toSelectOnOver;

	public bool toggleOnOver = true;

	public bool selectOnOver;

	public bool highlightOnOver = true;

	public bool toggleOnSelect = true;

	public bool highlightOnSelect;

	public bool unToggleOnExit;

	public bool unToggleOnUnSelect;

	public bool submitOnToggle;

	public bool enableKeySubmit = true;

	public bool keepSelectedOnSubmit = true;

	public bool checkInteractable = true;

	public bool overrideColor;

	public UnityEvent onAction;

	public UnityEvent onPointerEnter;

	public UnityEvent onPointerExit;

	public UnityEvent onSelect;

	public UnityEvent onUnselect;

	public UnityEvent onDoubleClick;

	public bool actionDisabled;

	private Toggle _toggle;

	private bool currentValue;

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
		((UnityEvent<bool>)(object)toggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool isOn)
		{
			OnValueChanged(isOn);
		});
	}

	private void OnEnable()
	{
		if (!actionDisabled)
		{
			Color(((ColorBlock)(ref color)).get_normalColor());
			Scale(scale.normalScale);
			OnValueChanged(toggle.get_isOn(), force: true);
		}
	}

	private void OnDisable()
	{
		Color(((ColorBlock)(ref color)).get_disabledColor());
		Scale(scale.normalScale);
	}

	public void DisableAction()
	{
		actionDisabled = true;
		Color(((ColorBlock)(ref color)).get_disabledColor());
		Scale(scale.normalScale);
	}

	public void EnableAction()
	{
		actionDisabled = false;
		Color(((ColorBlock)(ref color)).get_normalColor());
		Scale(scale.normalScale);
	}

	private void OnValueChanged(bool isOn, bool force = false)
	{
		if (!base.isActiveAndEnabled || (!force && currentValue == isOn))
		{
			return;
		}
		currentValue = isOn;
		if (!actionDisabled)
		{
			Color((!isOn) ? ((ColorBlock)(ref color)).get_normalColor() : ((ColorBlock)(ref color)).get_highlightedColor());
			Scale((!isOn) ? scale.normalScale : scale.highlightedScale);
		}
		if (isOn)
		{
			if (toggleOnSelect && base.isActiveAndEnabled)
			{
				StartCoroutine(SelectOnNextFrame());
			}
			else
			{
				onSelect.Invoke();
			}
		}
		else
		{
			onUnselect.Invoke();
		}
	}

	private void Color(Color newColor, bool forceColor = false)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		for (int i = 0; i < toColor.Count; i++)
		{
			if ((Object)(object)toColor[i] != null)
			{
				if (overrideColor)
				{
					ShortcutExtensions.DOColor(toColor[i], newColor, ((ColorBlock)(ref color)).get_fadeDuration());
				}
				else
				{
					toColor[i].CrossFadeColor(newColor, ((ColorBlock)(ref color)).get_fadeDuration(), true, true);
				}
			}
		}
	}

	private void Scale(Vector2 newScale)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Vector3 vector = new Vector3(newScale.x, newScale.y, 1f);
		for (int i = 0; i < toScale.Count; i++)
		{
			if (toScale[i] != null)
			{
				ShortcutExtensions.DOScale((Transform)toScale[i], vector, scale.duration);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!base.isActiveAndEnabled || !((Behaviour)(object)toggle).isActiveAndEnabled || (checkInteractable && !((Selectable)toggle).IsInteractable()))
		{
			return;
		}
		if (!actionDisabled)
		{
			if (highlightOnOver)
			{
				Color((!currentValue) ? ((ColorBlock)(ref color)).get_pressedColor() : ((ColorBlock)(ref color)).get_highlightedColor());
				Scale((!currentValue) ? scale.pressedScale : scale.highlightedScale);
			}
			if (toggleOnOver || selectOnOver)
			{
				if (toggleOnOver)
				{
					SetOn();
				}
				StartCoroutine(SelectOnNextFrame());
			}
		}
		onPointerEnter.Invoke();
	}

	private IEnumerator SelectOnNextFrame()
	{
		GameObject toSelect2 = null;
		toSelect2 = ((!(toSelectOnOver != null)) ? base.gameObject : toSelectOnOver);
		if ((Object)(object)EventSystem.get_current() != null && EventSystem.get_current().get_currentSelectedGameObject() != toSelect2)
		{
			yield return null;
			toSelect2.SetSelected();
		}
		else
		{
			onSelect.Invoke();
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (((Selectable)toggle).IsInteractable())
		{
			UISound.Instance.OnClick();
			if (keepSelectedOnSubmit && !currentValue)
			{
				SetOn();
			}
			if (!Input.GetMouseButtonUp(0) && enableKeySubmit)
			{
				onAction.Invoke();
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (base.isActiveAndEnabled && ((Behaviour)(object)toggle).isActiveAndEnabled && (!checkInteractable || ((Selectable)toggle).IsInteractable()) && (int)eventData.get_button() == 0)
		{
			UISound.Instance.OnClick();
			if (!actionDisabled && keepSelectedOnSubmit && !currentValue)
			{
				SetOn();
			}
			if (eventData.get_clickCount() == 2)
			{
				onDoubleClick.Invoke();
			}
			else if (!actionDisabled)
			{
				onAction.Invoke();
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		UISound.Instance.OnSelect();
		if (submitOnToggle && !actionDisabled)
		{
			onAction.Invoke();
		}
		if (highlightOnSelect && !actionDisabled)
		{
			Color((!currentValue) ? ((ColorBlock)(ref color)).get_pressedColor() : ((ColorBlock)(ref color)).get_highlightedColor());
			Scale((!currentValue) ? scale.pressedScale : scale.highlightedScale);
		}
		if (toggleOnSelect && !(eventData is PointerEventData))
		{
			SetOn();
		}
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(SelectOnNextFrame());
		}
	}

	public void SetOn()
	{
		if (!currentValue && !toggle.get_isOn())
		{
			toggle.set_isOn(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!base.isActiveAndEnabled || !((Behaviour)(object)toggle).isActiveAndEnabled || (checkInteractable && !((Selectable)toggle).IsInteractable()))
		{
			return;
		}
		if (!actionDisabled)
		{
			if (unToggleOnExit)
			{
				toggle.set_isOn(false);
				Color(((ColorBlock)(ref color)).get_normalColor());
				Scale(scale.normalScale);
				if (EventSystem.get_current().get_currentSelectedGameObject() == base.gameObject)
				{
					EventSystem.get_current().SetSelectedGameObject((GameObject)null);
				}
			}
			else if (highlightOnOver)
			{
				Color((!currentValue) ? ((ColorBlock)(ref color)).get_normalColor() : ((ColorBlock)(ref color)).get_highlightedColor());
				Scale((!currentValue) ? scale.normalScale : scale.highlightedScale);
			}
		}
		onPointerExit.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (!actionDisabled)
		{
			if (highlightOnSelect)
			{
				Color((!currentValue) ? ((ColorBlock)(ref color)).get_normalColor() : ((ColorBlock)(ref color)).get_highlightedColor());
				Scale((!currentValue) ? scale.normalScale : scale.highlightedScale);
			}
			if (unToggleOnUnSelect)
			{
				toggle.set_isOn(false);
			}
		}
		onUnselect.Invoke();
	}
}
