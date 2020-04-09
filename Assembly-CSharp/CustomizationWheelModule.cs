using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomizationWheelModule : UIModule
{
	public List<UIWheelBodyPartSlot> bodyPartSlot;

	public UIWheelBodyPartSlot presetSlot;

	public UIWheelBodyPartSlot skinSlot;

	private UnityAction<BodyPartId> onBodyPartSlotConfirmed;

	private UnityAction<BodyPartId, Sprite> onShowSlotDescription;

	private UnityAction<Sprite> onShowPresetsSlotDescription;

	private UnityAction onPresetsSlotConfirmed;

	private UnityAction<Sprite> onShowSkinSlotDescription;

	private UnityAction onSkinSlotConfirmed;

	private Selectable leftNavItem;

	public ToggleGroup toggleGroup;

	private bool focused;

	private Toggle lastSelected;

	public bool IsFocused
	{
		get;
		set;
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < bodyPartSlot.Count; i++)
		{
			int idx = i;
			bodyPartSlot[i].slot.onAction.AddListener(delegate
			{
				ConfirmBodyPartSlot(idx);
			});
			bodyPartSlot[i].slot.onSelect.AddListener(delegate
			{
				ShowBodyPartSlotDescription(idx);
			});
			bodyPartSlot[i].slot.onPointerEnter.AddListener(delegate
			{
				ShowBodyPartSlotDescription(idx);
			});
		}
		presetSlot.slot.onAction.AddListener(delegate
		{
			ConfirmPresetsSlot();
		});
		presetSlot.slot.onSelect.AddListener(delegate
		{
			ShowPresetsSlotDescription();
		});
		presetSlot.slot.onPointerEnter.AddListener(delegate
		{
			ShowPresetsSlotDescription();
		});
		skinSlot.slot.onAction.AddListener(delegate
		{
			ConfirmSkinSlot();
		});
		skinSlot.slot.onSelect.AddListener(delegate
		{
			ShowSkinSlotDescription();
		});
		skinSlot.slot.onPointerEnter.AddListener(delegate
		{
			ShowSkinSlotDescription();
		});
	}

	public void Activate(Selectable leftNavItem, UnityAction<BodyPartId, Sprite> showSlotDescription, UnityAction<BodyPartId> slotConfirmed, UnityAction<Sprite> showPresetsDescription, UnityAction presetsConfirmed, UnityAction<Sprite> showSkinDescription, UnityAction skinConfirmed)
	{
		SetInteractable(interactable: true);
		this.leftNavItem = leftNavItem;
		onShowSlotDescription = showSlotDescription;
		onBodyPartSlotConfirmed = slotConfirmed;
		onShowPresetsSlotDescription = showPresetsDescription;
		onPresetsSlotConfirmed = presetsConfirmed;
		onShowSkinSlotDescription = showSkinDescription;
		onSkinSlotConfirmed = skinConfirmed;
		lastSelected = null;
		SelectLastSelected();
	}

	public void Deactivate()
	{
		SetInteractable(interactable: false);
		onBodyPartSlotConfirmed = null;
	}

	private void OnDisable()
	{
		if ((Object)(object)lastSelected != null)
		{
			lastSelected.set_isOn(false);
		}
	}

	public void ShowBodyPartSlotDescription(int bodyPartIdx, bool force = false)
	{
		if (onShowSlotDescription != null && (IsFocused || force))
		{
			onShowSlotDescription(bodyPartSlot[bodyPartIdx].bodyPart, bodyPartSlot[bodyPartIdx].icon.get_sprite());
		}
	}

	public void ConfirmBodyPartSlot(int bodyPartIdx)
	{
		ShowBodyPartSlotDescription(bodyPartIdx, force: true);
		lastSelected = bodyPartSlot[bodyPartIdx].slot.toggle;
		if (onBodyPartSlotConfirmed != null)
		{
			IsFocused = false;
			onBodyPartSlotConfirmed(bodyPartSlot[bodyPartIdx].bodyPart);
		}
	}

	public void ShowPresetsSlotDescription(bool force = false)
	{
		if (onShowPresetsSlotDescription != null && (IsFocused || force))
		{
			onShowPresetsSlotDescription(presetSlot.icon.get_sprite());
		}
	}

	public void ConfirmPresetsSlot()
	{
		ShowPresetsSlotDescription(force: true);
		lastSelected = presetSlot.slot.toggle;
		if (onPresetsSlotConfirmed != null)
		{
			IsFocused = false;
			onPresetsSlotConfirmed();
		}
	}

	public void ShowSkinSlotDescription(bool force = false)
	{
		if (onShowSkinSlotDescription != null && (IsFocused || force))
		{
			onShowSkinSlotDescription(skinSlot.icon.get_sprite());
		}
	}

	public void ConfirmSkinSlot()
	{
		ShowSkinSlotDescription(force: true);
		lastSelected = skinSlot.slot.toggle;
		if (onSkinSlotConfirmed != null)
		{
			IsFocused = false;
			onSkinSlotConfirmed();
		}
	}

	public void SelectBodyPartSlot(BodyPartId bodyPart)
	{
		int num = 0;
		while (true)
		{
			if (num < bodyPartSlot.Count)
			{
				if (bodyPartSlot[num].bodyPart == bodyPart)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		bodyPartSlot[num].SetSelected(force: true);
	}

	private bool IsBodyPartCustomizable(UnitMenuController unitController, BodyPartId bodyPart)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = unitController.unit.bodyParts.ContainsKey(bodyPart);
		if (flag3)
		{
			BodyPart bodyPart2 = unitController.unit.bodyParts[bodyPart];
			flag = bodyPart2.Customizable;
			flag2 = (bodyPart2.GetAvailableModels().Count > 1 || bodyPart2.GetAvailableMaterials(includeInjuries: false).Count > 1);
		}
		return flag3 && flag && flag2;
	}

	public void RefreshSlots(UnitMenuController unitController, bool hasPresets)
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < bodyPartSlot.Count; i++)
		{
			bodyPartSlot[i].SetLocked(!IsBodyPartCustomizable(unitController, bodyPartSlot[i].bodyPart));
			if (bodyPartSlot[i].bodyPart == BodyPartId.GEAR_HEAD || bodyPartSlot[i].bodyPart == BodyPartId.HELMET)
			{
				if (!flag && bodyPartSlot[i].IsLocked())
				{
					bodyPartSlot[i].gameObject.SetActive(value: false);
				}
				else if (flag && !flag2)
				{
					bodyPartSlot[i].gameObject.SetActive(value: false);
				}
				else
				{
					bodyPartSlot[i].gameObject.SetActive(value: true);
				}
				flag2 = bodyPartSlot[i].IsLocked();
				flag = true;
			}
		}
		bool flag3 = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkinColorData>("fk_unit_id", unitController.unit.Data.Id.ToIntString()).Count > 1;
		skinSlot.SetLocked(!flag3);
		presetSlot.SetLocked(!hasPresets);
		for (int j = 0; j < bodyPartSlot.Count; j++)
		{
			if (bodyPartSlot[j].isActiveAndEnabled)
			{
				AutoLinkNavNode(bodyPartSlot[j]);
			}
		}
		if (flag3)
		{
			AutoLinkNavNode(skinSlot);
		}
		if (hasPresets)
		{
			AutoLinkNavNode(presetSlot);
		}
	}

	private void AutoLinkNavNode(UIWheelSlot node)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)node.slot.toggle).get_navigation();
		List<UIWheelSlot> list = new List<UIWheelSlot>();
		UIWheelSlot uIWheelSlot = node;
		list.Clear();
		((Navigation)(ref navigation)).set_selectOnUp((Selectable)null);
		while (uIWheelSlot != null && !list.Contains(uIWheelSlot))
		{
			list.Add(uIWheelSlot);
			if (uIWheelSlot.defaultUpItem != null && uIWheelSlot.defaultUpItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.defaultUpItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnUp((Selectable)(object)uIWheelSlot.defaultUpItem.toggle);
				break;
			}
			if (uIWheelSlot.altUpItem != null && uIWheelSlot.altUpItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.altUpItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnUp((Selectable)(object)uIWheelSlot.altUpItem.toggle);
				break;
			}
			if (uIWheelSlot.defaultUpItem != null)
			{
				uIWheelSlot = uIWheelSlot.defaultUpItem.GetComponent<UIWheelSlot>();
				continue;
			}
			break;
		}
		uIWheelSlot = node;
		list.Clear();
		((Navigation)(ref navigation)).set_selectOnDown((Selectable)null);
		while (uIWheelSlot != null && !list.Contains(uIWheelSlot))
		{
			list.Add(uIWheelSlot);
			if (uIWheelSlot.defaultDownItem != null && uIWheelSlot.defaultDownItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.defaultDownItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnDown((Selectable)(object)uIWheelSlot.defaultDownItem.toggle);
				break;
			}
			if (uIWheelSlot.altDownItem != null && uIWheelSlot.altDownItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.altDownItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnDown((Selectable)(object)uIWheelSlot.altDownItem.toggle);
				break;
			}
			if (uIWheelSlot.defaultDownItem != null)
			{
				uIWheelSlot = uIWheelSlot.defaultDownItem.GetComponent<UIWheelSlot>();
				continue;
			}
			break;
		}
		uIWheelSlot = node;
		list.Clear();
		((Navigation)(ref navigation)).set_selectOnLeft((Selectable)null);
		while (uIWheelSlot != null && !list.Contains(uIWheelSlot))
		{
			list.Add(uIWheelSlot);
			if (uIWheelSlot.defaultLeftItem != null && uIWheelSlot.defaultLeftItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.defaultLeftItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnLeft((Selectable)(object)uIWheelSlot.defaultLeftItem.toggle);
				break;
			}
			if (uIWheelSlot.altLeftItem != null && uIWheelSlot.altLeftItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.altLeftItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnLeft((Selectable)(object)uIWheelSlot.altLeftItem.toggle);
				break;
			}
			if (uIWheelSlot.defaultLeftItem != null)
			{
				uIWheelSlot = uIWheelSlot.defaultLeftItem.GetComponent<UIWheelSlot>();
				continue;
			}
			break;
		}
		if (uIWheelSlot != null && uIWheelSlot.defaultLeftItem == null)
		{
			((Navigation)(ref navigation)).set_selectOnLeft(leftNavItem);
		}
		uIWheelSlot = node;
		list.Clear();
		((Navigation)(ref navigation)).set_selectOnRight((Selectable)null);
		while (uIWheelSlot != null && !list.Contains(uIWheelSlot))
		{
			list.Add(uIWheelSlot);
			if (uIWheelSlot.defaultRightItem != null && uIWheelSlot.defaultRightItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.defaultRightItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnRight((Selectable)(object)uIWheelSlot.defaultRightItem.toggle);
				break;
			}
			if (uIWheelSlot.altRightItem != null && uIWheelSlot.altRightItem.isActiveAndEnabled && ((Selectable)uIWheelSlot.altRightItem.toggle).get_interactable())
			{
				((Navigation)(ref navigation)).set_selectOnRight((Selectable)(object)uIWheelSlot.altRightItem.toggle);
				break;
			}
			if (uIWheelSlot.defaultRightItem != null)
			{
				uIWheelSlot = uIWheelSlot.defaultRightItem.GetComponent<UIWheelSlot>();
				continue;
			}
			break;
		}
		((Selectable)node.slot.toggle).set_navigation(navigation);
	}

	private void OnEnable()
	{
		if ((Object)(object)toggleGroup != null)
		{
			toggleGroup.SetAllTogglesOff();
		}
	}

	public void SelectLastSelected()
	{
		if ((Object)(object)lastSelected != null && ((Behaviour)(object)lastSelected).isActiveAndEnabled && ((Selectable)lastSelected).IsInteractable())
		{
			((MonoBehaviour)(object)lastSelected).SetSelected(force: true);
			return;
		}
		if (presetSlot.isActiveAndEnabled && ((Selectable)presetSlot.slot.toggle).IsInteractable())
		{
			presetSlot.SetSelected(force: true);
			lastSelected = presetSlot.slot.toggle;
			return;
		}
		if (skinSlot.isActiveAndEnabled && ((Selectable)skinSlot.slot.toggle).IsInteractable())
		{
			skinSlot.SetSelected(force: true);
			lastSelected = skinSlot.slot.toggle;
			return;
		}
		int num = 0;
		while (true)
		{
			if (num < bodyPartSlot.Count)
			{
				if (bodyPartSlot[num].isActiveAndEnabled && ((Selectable)bodyPartSlot[num].slot.toggle).IsInteractable())
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		bodyPartSlot[num].SetSelected(force: true);
		lastSelected = bodyPartSlot[num].slot.toggle;
	}
}
