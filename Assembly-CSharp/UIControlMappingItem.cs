using Rewired;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIControlMappingItem : MonoBehaviour
{
	public ToggleEffects[] mappingButtons;

	public Text actionLabel;

	public int actionId;

	public bool isPositiveInput = true;

	public string inputCategory;

	private ActionElementMap[] actionElement;

	public Action<UIControlMappingItem, int, ActionElementMap> OnMappingButton;

	public Color disabledColor;

	private void Awake()
	{
		for (int i = 0; i < mappingButtons.Length; i++)
		{
			int btnId = i;
			mappingButtons[i].onAction.AddListener(delegate
			{
				OnMappingButtonClicked(btnId);
			});
		}
		actionElement = (ActionElementMap[])(object)new ActionElementMap[mappingButtons.Length];
	}

	public void SetMapping(ActionElementMap mapping, int index, bool remappable = true)
	{
		if (index >= mappingButtons.Length)
		{
			return;
		}
		actionElement[index] = mapping;
		if (mapping != null)
		{
			string elementIdentifierName = mapping.get_elementIdentifierName();
			string text = elementIdentifierName;
			elementIdentifierName = "key_" + elementIdentifierName.ToLowerInvariant().Replace(" ", "_");
			if (elementIdentifierName.Equals("key_mouse_horizontal") || elementIdentifierName.Equals("key_mouse_vertical"))
			{
				elementIdentifierName = "key_mouse_move";
			}
			else if (index == 1)
			{
				elementIdentifierName = elementIdentifierName.Replace("_+", string.Empty);
				elementIdentifierName = elementIdentifierName.Replace("_-", string.Empty);
			}
			text = PandoraSingleton<LocalizationManager>.Instance.GetStringById(elementIdentifierName);
			mappingButtons[index].GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(text);
		}
		else if (!remappable)
		{
			mappingButtons[index].GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_action_name_not_available"));
		}
		else
		{
			mappingButtons[index].GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(string.Empty);
		}
		if (!remappable)
		{
			if (mappingButtons[index] != null)
			{
				mappingButtons[index].onAction.RemoveAllListeners();
				((ColorBlock)(ref mappingButtons[index].color)).set_highlightedColor(((ColorBlock)(ref mappingButtons[index].color)).get_normalColor());
			}
			((Graphic)mappingButtons[index].GetComponentsInChildren<Text>(includeInactive: true)[0]).set_color(disabledColor);
		}
	}

	public void SetNav(UIControlMappingItem up, UIControlMappingItem down)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < mappingButtons.Length; i++)
		{
			Navigation navigation = ((Selectable)mappingButtons[i].toggle).get_navigation();
			((Navigation)(ref navigation)).set_selectOnUp((Selectable)(object)up.mappingButtons[i].toggle);
			((Navigation)(ref navigation)).set_selectOnDown((Selectable)(object)down.mappingButtons[i].toggle);
			((Selectable)mappingButtons[i].toggle).set_navigation(navigation);
		}
	}

	public void OnMappingButtonClicked(int buttonIndex)
	{
		OnMappingButton(this, buttonIndex, actionElement[buttonIndex]);
	}
}
