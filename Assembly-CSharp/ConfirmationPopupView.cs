using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmationPopupView : UIPopupModule
{
	public Text title;

	public Text text;

	protected Action<bool> _callback;

	private Vector3 _startPosition;

	private GameObject previousSelection;

	public ButtonGroup confirmButton;

	public ButtonGroup cancelButton;

	public bool isShow;

	protected bool isSystem;

	public bool IsVisible => base.gameObject.activeSelf;

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void Start()
	{
		if (!isShow)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public override void Init()
	{
		base.Init();
		PandoraDebug.LogDebug("ConfirmationPopupView Initialize.");
		if (confirmButton != null)
		{
			confirmButton.SetAction(null, "menu_confirm", 1);
			confirmButton.OnAction(Confirm, mouseOnly: false);
		}
		if (cancelButton != null)
		{
			cancelButton.SetAction(null, "menu_cancel", 1);
			cancelButton.OnAction(Cancel, mouseOnly: false);
		}
	}

	public virtual void ShowLocalized(string newTitle, string newText, Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
	{
		if (!string.IsNullOrEmpty(newTitle))
		{
			title.set_text(newTitle);
		}
		if (!string.IsNullOrEmpty(newText))
		{
			text.set_text(newText);
		}
		Show(callback, hideButtons, hideCancel);
	}

	public virtual void Show(string titleId, string textId, Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
	{
		if (!string.IsNullOrEmpty(titleId))
		{
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleId));
		}
		if (!string.IsNullOrEmpty(textId))
		{
			text.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(textId));
		}
		Show(callback, hideButtons, hideCancel);
	}

	public virtual void Show(Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
	{
		Hide();
		if (!isShow)
		{
			PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.POP_UP);
		}
		isShow = true;
		_callback = callback;
		if (!base.initialized)
		{
			Init();
		}
		previousSelection = ((!((UnityEngine.Object)(object)EventSystem.get_current() != null)) ? null : EventSystem.get_current().get_currentSelectedGameObject());
		if (cancelButton != null && !hideCancel)
		{
			cancelButton.effects.toggle.set_isOn(true);
			if (confirmButton != null)
			{
				confirmButton.effects.toggle.set_isOn(false);
			}
			cancelButton.SetSelected(force: true);
		}
		else if (confirmButton != null)
		{
			confirmButton.effects.toggle.set_isOn(true);
			if (cancelButton != null)
			{
				cancelButton.effects.toggle.set_isOn(false);
			}
			confirmButton.SetSelected(force: true);
		}
		else
		{
			base.gameObject.SetSelected(force: true);
		}
		base.gameObject.SetActive(value: true);
		if (hideButtons)
		{
			if (confirmButton != null)
			{
				confirmButton.gameObject.SetActive(value: false);
			}
			if (cancelButton != null)
			{
				cancelButton.gameObject.SetActive(value: false);
			}
			return;
		}
		if (cancelButton != null)
		{
			if (hideCancel)
			{
				cancelButton.gameObject.SetActive(value: false);
			}
			else
			{
				cancelButton.SetAction(null, "menu_cancel", 1);
				cancelButton.gameObject.SetActive(value: true);
				cancelButton.RefreshImage();
			}
		}
		if (confirmButton != null)
		{
			confirmButton.gameObject.SetActive(value: true);
			if (cancelButton != null)
			{
				confirmButton.SetAction(null, "menu_confirm", 1);
			}
			else
			{
				confirmButton.SetAction(null, "menu_continue", 1);
				confirmButton.SetSelected(force: true);
			}
			confirmButton.RefreshImage();
		}
	}

	public virtual void Hide()
	{
		if (isShow)
		{
			isShow = false;
			base.gameObject.SetActive(value: false);
			PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.POP_UP);
			if (previousSelection != null)
			{
				previousSelection.SetSelected();
			}
		}
	}

	public void HideCancelButton()
	{
		if (cancelButton != null)
		{
			cancelButton.gameObject.SetActive(value: false);
		}
		confirmButton.SetAction(null, "menu_continue", 1);
		confirmButton.SetSelected(force: true);
	}

	public virtual void Confirm()
	{
		PandoraDebug.LogDebug("ConfirmationPopupView Confirm!");
		Hide();
		if (_callback != null)
		{
			_callback(obj: true);
		}
	}

	public virtual void Cancel()
	{
		Hide();
		if (_callback != null)
		{
			_callback(obj: false);
		}
	}

	public void ResetPreviousSelection()
	{
		previousSelection = null;
	}

	private void Update()
	{
		if (isShow && (isSystem || !PandoraSingleton<GameManager>.Instance.Popup.isShow) && (UnityEngine.Object)(object)EventSystem.get_current() != null && (EventSystem.get_current().get_currentSelectedGameObject() == null || EventSystem.get_current().get_currentSelectedGameObject().transform.root != base.transform.root))
		{
			if (cancelButton != null && cancelButton.isActiveAndEnabled)
			{
				cancelButton.SetSelected(force: true);
			}
			else if (confirmButton != null)
			{
				confirmButton.SetSelected(force: true);
			}
		}
	}
}
