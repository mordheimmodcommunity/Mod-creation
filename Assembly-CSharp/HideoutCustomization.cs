using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideoutCustomization : BaseHideoutUnitState
{
	private BodyPartId selectedBodyPart;

	private bool isActive;

	private UnitCustomizationModule unitCustomizationModule;

	private CustomizationWheelModule wheelModule;

	private CustomizationDescModule custoDescModule;

	private UnitBioModule bioModule;

	private List<ColorPresetData> presetsData;

	private List<string> availableSkins;

	private List<string> availableColours;

	private List<string> availableModels;

	public HideoutCustomization(HideoutManager mng, HideoutCamAnchor anchor)
		: base(anchor, HideoutManager.State.CUSTOMIZATION)
	{
		availableModels = new List<string>();
	}

	public override void Enter(int iFrom)
	{
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.UNIT_SHEET, ModuleId.UNIT_CUSTOMIZATION);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.UNIT_CUSTOMIZATION, ModuleId.TREASURY);
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count > 1)
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.NEXT_UNIT, ModuleId.UNIT_TABS, ModuleId.TITLE, ModuleId.CHARACTER_AREA, ModuleId.UNIT_CUSTOMIZATION, ModuleId.UNIT_CUSTOMIZATION_DESC, ModuleId.NOTIFICATION);
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<NextUnitModule>(ModuleId.NEXT_UNIT).Setup();
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.UNIT_TABS, ModuleId.TITLE, ModuleId.CHARACTER_AREA, ModuleId.UNIT_CUSTOMIZATION, ModuleId.UNIT_CUSTOMIZATION_DESC, ModuleId.NOTIFICATION);
		}
		Init();
		sheetModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitSheetModule>(ModuleId.UNIT_SHEET);
		bioModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitBioModule>(ModuleId.UNIT_CUSTOMIZATION);
		custoDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CustomizationDescModule>(ModuleId.UNIT_CUSTOMIZATION_DESC);
		characterCamModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CharacterCameraAreaModule>(ModuleId.CHARACTER_AREA);
		characterCamModule.Init(camAnchor.transform.position);
		wheelModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CustomizationWheelModule>(ModuleId.UNIT_CUSTOMIZATION);
		wheelModule.IsFocused = true;
		wheelModule.Activate(null, OnBodyPartHighlighted, OnBodyPartSelected, OnPresetHighlighted, OnPresetSelected, OnSkinHighlighted, OnSkinSelected);
		unitCustomizationModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<UnitCustomizationModule>(ModuleId.UNIT_CUSTOMIZATION);
		unitCustomizationModule.onTabSelected = OnTabSelected;
		SelectUnit(PandoraSingleton<HideoutManager>.Instance.currentUnit);
		bioModule.Setup(wheelModule.GetComponent<ToggleGroup>(), OnNameChanged, OnBioChanged, delegate
		{
			wheelModule.SelectLastSelected();
		});
		SetupWheelSlotSelectionButtons();
	}

	public override void Exit(int iTo)
	{
		base.Exit(iTo);
		unitCustomizationModule.Clear();
		unitCustomizationModule.SetFocused(focused: false);
	}

	public override Selectable ModuleLeftOnRight()
	{
		return null;
	}

	public override void SelectUnit(UnitMenuController ctrlr)
	{
		base.SelectUnit(ctrlr);
		bioModule.SetName(ctrlr.unit.UnitSave.stats.Name);
		bioModule.SetBio(ctrlr.unit.UnitSave.bio);
		presetsData = PandoraSingleton<DataFactory>.Instance.InitData<ColorPresetData>("fk_warband_id", ((int)ctrlr.unit.WarbandId).ToString()).ToDynList();
		for (int num = presetsData.Count - 1; num >= 0; num--)
		{
			List<BodyPartColorData> list = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartColorData>(new string[2]
			{
				"fk_unit_id",
				"fk_color_preset_id"
			}, new string[2]
			{
				((int)ctrlr.unit.Id).ToString(),
				presetsData[num].Id.ToIntString()
			});
			if (list.Count == 0)
			{
				presetsData.RemoveAt(num);
			}
		}
		availableSkins = new List<string>();
		List<UnitJoinSkinColorData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkinColorData>("fk_unit_id", ctrlr.unit.Data.Id.ToIntString());
		for (int i = 0; i < list2.Count; i++)
		{
			availableSkins.Add(list2[i].SkinColorId.ToLowerString());
		}
		wheelModule.RefreshSlots(ctrlr, presetsData.Count > 0);
		ReturnToWheelSlotSelection();
	}

	public override bool CanIncreaseAttributes()
	{
		return false;
	}

	private void OnPresetChanged(int index)
	{
		currentUnit.SetColorPreset(presetsData[index].Id);
		PandoraSingleton<HideoutManager>.Instance.SaveChanges();
	}

	private void OnSkinChanged(int index)
	{
		currentUnit.SetSkinColor(availableSkins[index]);
		PandoraSingleton<HideoutManager>.Instance.SaveChanges();
		wheelModule.RefreshSlots(currentUnit, presetsData.Count > 0);
	}

	private void OnModelChanged(int index)
	{
		if (selectedBodyPart == BodyPartId.LEGL)
		{
			if (currentUnit.unit.bodyParts.ContainsKey(BodyPartId.LEGR))
			{
				currentUnit.SetModelVariation(BodyPartId.LEGR, index);
			}
			if (currentUnit.unit.bodyParts.ContainsKey(BodyPartId.LLEGR))
			{
				currentUnit.SetModelVariation(BodyPartId.LLEGR, index);
			}
		}
		else if (selectedBodyPart == BodyPartId.FOOTL && currentUnit.unit.bodyParts.ContainsKey(BodyPartId.FOOTR) && !currentUnit.unit.HasInjury(InjuryId.SEVERED_LEG))
		{
			currentUnit.SetModelVariation(BodyPartId.FOOTR, index);
		}
		currentUnit.SetModelVariation(selectedBodyPart, index);
		availableColours = currentUnit.unit.bodyParts[selectedBodyPart].GetAvailableMaterials(includeInjuries: false);
		unitCustomizationModule.SetTabsVisible();
		PandoraSingleton<HideoutManager>.Instance.SaveChanges();
	}

	private void OnColorChanged(int index)
	{
		if (selectedBodyPart == BodyPartId.LEGL)
		{
			if (currentUnit.unit.bodyParts.ContainsKey(BodyPartId.LEGR))
			{
				currentUnit.SetBodyPartColor(BodyPartId.LEGR, index);
			}
			if (currentUnit.unit.bodyParts.ContainsKey(BodyPartId.LLEGR))
			{
				currentUnit.SetBodyPartColor(BodyPartId.LLEGR, index);
			}
		}
		else if (selectedBodyPart == BodyPartId.FOOTL && currentUnit.unit.bodyParts.ContainsKey(BodyPartId.FOOTR))
		{
			currentUnit.SetBodyPartColor(BodyPartId.FOOTR, index);
		}
		currentUnit.SetBodyPartColor(selectedBodyPart, index);
		PandoraSingleton<HideoutManager>.Instance.SaveChanges();
	}

	private void OnTabSelected(int idx)
	{
		if (idx != -1)
		{
			unitCustomizationModule.SetSelected();
		}
		switch (idx)
		{
		case 0:
		{
			List<string> list2 = new List<string>(availableColours.Count);
			for (int j = 1; j <= availableColours.Count; j++)
			{
				list2.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_custom_style_param", j.ToString()));
			}
			unitCustomizationModule.Refresh(list2, OnColorChanged, string.Empty);
			int colorIndex = currentUnit.unit.bodyParts[selectedBodyPart].GetColorIndex();
			if (colorIndex < 256 && colorIndex >= 0 && colorIndex < list2.Count)
			{
				unitCustomizationModule.SetSelectedStyle(colorIndex);
			}
			else if (list2.Count > 0)
			{
				unitCustomizationModule.SetSelectedStyle(0);
			}
			break;
		}
		case 1:
		{
			List<string> list = new List<string>(availableModels.Count);
			for (int i = 1; i <= availableModels.Count; i++)
			{
				list.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_custom_style_param", i.ToString()));
			}
			unitCustomizationModule.Refresh(list, OnModelChanged, string.Empty);
			int variation = currentUnit.unit.bodyParts[selectedBodyPart].GetVariation();
			if (variation >= 0 && variation < list.Count)
			{
				unitCustomizationModule.SetSelectedStyle(variation);
			}
			else if (list.Count > 0)
			{
				unitCustomizationModule.SetSelectedStyle(0);
			}
			break;
		}
		}
	}

	private void OnPresetHighlighted(Sprite icon)
	{
		custoDescModule.Set(icon, "hideout_custom_preset", "hideout_custom_preset_desc");
	}

	private void OnPresetSelected()
	{
		SetupCustomizationButtons();
		unitCustomizationModule.SetFocused(focused: true);
		unitCustomizationModule.SetTabsVisible(visible: false);
		List<string> list = presetsData.ConvertAll((ColorPresetData x) => PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_custom_color_preset_" + x.Name.ToLowerInvariant()));
		unitCustomizationModule.Refresh(list, OnPresetChanged, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_custom_preset"));
		if (list.Count > 0)
		{
			unitCustomizationModule.SetSelectedStyle(0);
		}
	}

	private void OnSkinHighlighted(Sprite icon)
	{
		custoDescModule.Set(icon, "hideout_custom_skin_title", "hideout_custom_skin_desc");
	}

	private void OnSkinSelected()
	{
		SetupCustomizationButtons();
		unitCustomizationModule.SetFocused(focused: true);
		unitCustomizationModule.SetTabsVisible(visible: false);
		List<string> styles = availableSkins.ConvertAll((string x) => PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_custom_color_skin_" + x.ToLowerInvariant()));
		unitCustomizationModule.Refresh(styles, OnSkinChanged, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_custom_skin_title"));
		string text = currentUnit.unit.UnitSave.skinColor;
		if (string.IsNullOrEmpty(text))
		{
			text = availableSkins[0];
		}
		int num = 0;
		while (true)
		{
			if (num < availableSkins.Count)
			{
				if (availableSkins[num] == text)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		unitCustomizationModule.SetSelectedStyle(num);
	}

	private void OnBodyPartHighlighted(BodyPartId bodyPart, Sprite icon)
	{
		if (currentUnit.unit.bodyParts.ContainsKey(bodyPart))
		{
			string str = currentUnit.unit.bodyParts[bodyPart].Name.ToLowerInvariant();
			custoDescModule.Set(icon, "hideout_custom_title_" + str, "hideout_custom_desc_" + str);
		}
	}

	private void OnBodyPartSelected(BodyPartId bodyPart)
	{
		selectedBodyPart = bodyPart;
		Transform bodyPartLookAtTarget = GetBodyPartLookAtTarget(selectedBodyPart);
		if (bodyPartLookAtTarget == null)
		{
			characterCamModule.SetCameraLookAtDefault(instantTransition: false);
		}
		else
		{
			characterCamModule.SetCameraLookAt(bodyPartLookAtTarget, instantTransition: false);
		}
		availableModels = currentUnit.unit.bodyParts[selectedBodyPart].GetAvailableModels();
		availableColours = currentUnit.unit.bodyParts[selectedBodyPart].GetAvailableMaterials(includeInjuries: false);
		SetupCustomizationButtons();
		unitCustomizationModule.SetTabsVisible();
		OnTabSelected(unitCustomizationModule.GetSelectedTabIndex());
		unitCustomizationModule.SetFocused(focused: true);
	}

	private void ReturnToWheelSlotSelection()
	{
		SetupWheelSlotSelectionButtons();
		unitCustomizationModule.Clear();
		unitCustomizationModule.SetFocused(focused: false);
		wheelModule.SelectLastSelected();
		wheelModule.IsFocused = true;
	}

	private void SetupCustomizationButtons()
	{
		if (Input.mousePresent)
		{
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
			PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: true);
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: false);
		}
		PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("cancel", "menu_return_select_slot");
		PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(ReturnToWheelSlotSelection, mouseOnly: false);
	}

	private void SetupWheelSlotSelectionButtons()
	{
		PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
		if (Input.mousePresent)
		{
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband");
		}
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
	}

	private void OnNameChanged(string newName)
	{
		if (!string.IsNullOrEmpty(newName))
		{
			currentUnit.unit.UnitSave.stats.overrideName = newName;
			sheetModule.unitName.set_text(newName);
			bioModule.SetName(newName);
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
		}
		ReturnToWheelSlotSelection();
	}

	private void OnBioChanged(string newBio)
	{
		if (!string.IsNullOrEmpty(newBio))
		{
			currentUnit.unit.UnitSave.bio = newBio;
			bioModule.SetBio(newBio);
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
		}
		ReturnToWheelSlotSelection();
	}

	private Transform GetBodyPartLookAtTarget(BodyPartId bodyPart)
	{
		switch (bodyPart)
		{
		case BodyPartId.ARML:
		case BodyPartId.HANDL:
		case BodyPartId.GEAR_ARML:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_LARMPALM))
			{
				return currentUnit.BonesTr[BoneId.RIG_LARMPALM];
			}
			break;
		case BodyPartId.ARMR:
		case BodyPartId.HANDR:
		case BodyPartId.GEAR_ARMR:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_RARMPALM))
			{
				return currentUnit.BonesTr[BoneId.RIG_RARMPALM];
			}
			break;
		case BodyPartId.BODY:
		case BodyPartId.GEAR_BACK:
		case BodyPartId.GEAR_BODY:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_SPINE3))
			{
				return currentUnit.BonesTr[BoneId.RIG_SPINE3];
			}
			break;
		case BodyPartId.FOOTL:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_LLEGANKLE))
			{
				return currentUnit.BonesTr[BoneId.RIG_LLEGANKLE];
			}
			break;
		case BodyPartId.LEGL:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_LLEG21))
			{
				return currentUnit.BonesTr[BoneId.RIG_LLEG21];
			}
			break;
		case BodyPartId.FOOTR:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_RLEGANKLE))
			{
				return currentUnit.BonesTr[BoneId.RIG_RLEGANKLE];
			}
			break;
		case BodyPartId.LEGR:
		case BodyPartId.LLEGR:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_RLEG21))
			{
				return currentUnit.BonesTr[BoneId.RIG_RLEG21];
			}
			break;
		case BodyPartId.GEAR_BELT:
		case BodyPartId.GEAR_LEGS:
		case BodyPartId.GEAR_LOIN:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_PELVIS))
			{
				return currentUnit.BonesTr[BoneId.RIG_PELVIS];
			}
			break;
		case BodyPartId.HELMET:
		case BodyPartId.HEAD:
		case BodyPartId.GEAR_HELMET:
		case BodyPartId.GEAR_HEAD:
		case BodyPartId.GEAR_FACE:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_HEAD))
			{
				return currentUnit.BonesTr[BoneId.RIG_HEAD];
			}
			break;
		case BodyPartId.GEAR_NECK:
		case BodyPartId.SHOULDERR:
			if (currentUnit.BonesTr.ContainsKey(BoneId.RIG_NECK1))
			{
				return currentUnit.BonesTr[BoneId.RIG_NECK1];
			}
			break;
		default:
			return null;
		}
		return null;
	}
}
