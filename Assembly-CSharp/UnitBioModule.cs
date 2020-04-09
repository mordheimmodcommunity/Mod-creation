using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitBioModule : UIModule
{
	public HightlightAnimate highlight;

	public ToggleGroup toggleGroup;

	public ButtonGroup editNameButton;

	public ButtonGroup editBioButton;

	public InputField editNameField;

	public InputField editBioField;

	private Action<string> onNameChanged;

	private Action<string> onBioChanged;

	private Action onNavigateRight;

	public override void Init()
	{
		base.Init();
		if (PandoraSingleton<Hephaestus>.Instance.IsPrivilegeRestricted(Hephaestus.RestrictionId.UGC))
		{
			editNameButton.gameObject.SetActive(value: false);
			editBioButton.gameObject.SetActive(value: false);
			return;
		}
		editNameButton.gameObject.SetActive(value: true);
		editBioButton.gameObject.SetActive(value: true);
		editNameButton.SetAction("rename_unit", "hideout_custom_edit_name");
		editNameButton.OnAction(OnEditNamePressed, mouseOnly: false);
		((UnityEvent<string>)(object)editNameField.get_onEndEdit()).AddListener((UnityAction<string>)OnNameChanged);
		((Selectable)editNameField).set_interactable(false);
		editBioButton.SetAction("edit_bio", "hideout_custom_edit_bio");
		editBioButton.OnAction(OnEditBioPressed, mouseOnly: false);
		((UnityEvent<string>)(object)editBioField.get_onEndEdit()).AddListener((UnityAction<string>)OnBioChanged);
		((Selectable)editBioField).set_interactable(false);
	}

	public void Setup(ToggleGroup toggleGroup, Action<string> nameChangedCb, Action<string> bioChangedCb, Action navigateRight)
	{
		if ((UnityEngine.Object)(object)toggleGroup != null)
		{
			editNameButton.GetComponent<Toggle>().set_group(toggleGroup);
			editBioButton.GetComponent<Toggle>().set_group(toggleGroup);
		}
		onNameChanged = nameChangedCb;
		onBioChanged = bioChangedCb;
		onNavigateRight = navigateRight;
	}

	private void OnEnable()
	{
		if ((UnityEngine.Object)(object)toggleGroup != null)
		{
			toggleGroup.SetAllTogglesOff();
		}
	}

	private void Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h") && onNavigateRight != null && (editBioButton.GetComponent<Toggle>().get_isOn() || editNameButton.GetComponent<Toggle>().get_isOn()))
		{
			onNavigateRight();
		}
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel", 2))
		{
			if (((Selectable)editNameField).get_interactable())
			{
				editNameField.DeactivateInputField();
				UnfocusField(editNameField);
				editNameButton.SetSelected(force: true);
			}
			else
			{
				editBioField.DeactivateInputField();
				UnfocusField(editBioField);
				editBioButton.SetSelected(force: true);
			}
		}
	}

	public void SetName(string name)
	{
		editNameField.set_text(name);
	}

	private void OnEditNamePressed()
	{
		if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
		{
			if (!PandoraSingleton<Hephaestus>.Instance.ShowVirtualKeyboard(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_name"), editNameField.get_text(), 35u, multiLine: false, OnNameChangedVK))
			{
				FocusField(editNameField);
			}
		}
		else
		{
			FocusField(editNameField);
		}
	}

	private void OnNameChangedVK(bool success, string newName)
	{
		if (success)
		{
			OnNameChanged(newName);
		}
	}

	private void OnNameChanged(string newName)
	{
		UnfocusField(editNameField);
		if (onNameChanged != null)
		{
			onNameChanged(newName);
		}
	}

	public void SetBio(string bio)
	{
		editBioField.set_text(bio);
	}

	private void OnEditBioPressed()
	{
		if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
		{
			uint maxChar = 2500u;
			if (!PandoraSingleton<Hephaestus>.Instance.ShowVirtualKeyboard(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_biography"), editBioField.get_text(), maxChar, multiLine: false, OnBioChangedVK, validateString: false))
			{
				FocusField(editBioField);
			}
		}
		else
		{
			FocusField(editBioField);
		}
	}

	private void OnBioChangedVK(bool success, string newBio)
	{
		if (success)
		{
			OnBioChanged(newBio);
		}
	}

	private void OnBioChanged(string newBio)
	{
		UnfocusField(editBioField);
		if (onBioChanged != null)
		{
			onBioChanged(newBio);
		}
	}

	private void FocusField(InputField field)
	{
		((Selectable)field).set_interactable(true);
		((MonoBehaviour)(object)field).SetSelected(force: true);
		field.set_selectionAnchorPosition(0);
		field.set_selectionFocusPosition(field.get_text().Length);
		PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.CHAT);
	}

	private void UnfocusField(InputField field)
	{
		((Selectable)field).set_interactable(false);
		field.set_selectionAnchorPosition(0);
		field.set_selectionFocusPosition(0);
		PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.CHAT);
	}

	public Selectable GetNavItem()
	{
		return (Selectable)(object)editNameButton.GetComponent<Toggle>();
	}
}
