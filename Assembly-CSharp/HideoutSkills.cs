using UnityEngine.UI;

public class HideoutSkills : BaseHideoutUnitState
{
	private readonly SkillsShop skillsShop = new SkillsShop();

	private SkillsModule skillsModule;

	private SkillsWheelModule wheelModule;

	private SkillDescModule skillDescModule;

	private bool isActive;

	private SkillData currentLearnSkill;

	private bool currentSkillActive;

	private int currentSkillIndex;

	private SkillData selectedSkill;

	private bool showSpell;

	private Warband warband;

	public HideoutSkills(HideoutManager mng, HideoutCamAnchor anchor, bool showSpell)
		: base(anchor, (!showSpell) ? HideoutManager.State.SKILLS : HideoutManager.State.SPELLS)
	{
		this.showSpell = showSpell;
	}

	public override void Enter(int iFrom)
	{
		currentSkillIndex = -1;
		warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.SKILLS);
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count > 1)
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WHEEL_SKILLS, ModuleId.UNIT_TABS, ModuleId.TITLE, (!showSpell) ? ModuleId.SKILL_DESC : ModuleId.SPELL_DESC, ModuleId.DESC, ModuleId.NEXT_UNIT, ModuleId.CHARACTER_AREA, ModuleId.NOTIFICATION);
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<NextUnitModule>(ModuleId.NEXT_UNIT).Setup();
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WHEEL_SKILLS, ModuleId.UNIT_TABS, ModuleId.TITLE, (!showSpell) ? ModuleId.SKILL_DESC : ModuleId.SPELL_DESC, ModuleId.DESC, ModuleId.CHARACTER_AREA, ModuleId.NOTIFICATION);
		}
		base.Enter(iFrom);
		descModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<DescriptionModule>(ModuleId.DESC);
		descModule.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
		skillsModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<SkillsModule>(ModuleId.SKILLS);
		skillsModule.Refresh(showSpell);
		wheelModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<SkillsWheelModule>(ModuleId.WHEEL_SKILLS);
		skillDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<SkillDescModule>((!showSpell) ? ModuleId.SKILL_DESC : ModuleId.SPELL_DESC);
		characterCamModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CharacterCameraAreaModule>(ModuleId.CHARACTER_AREA);
		characterCamModule.Init(camAnchor.transform.position);
		SelectUnit(PandoraSingleton<HideoutManager>.Instance.currentUnit);
	}

	public override void Exit(int iTo)
	{
		base.Exit(iTo);
		wheelModule.Deactivate();
		skillsModule.ClearList();
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WheelModule>(ModuleId.WHEEL).Deactivate();
	}

	public override void SelectUnit(UnitMenuController ctrlr)
	{
		base.SelectUnit(ctrlr);
		UpdateWheel();
		skillDescModule.gameObject.SetActive(value: false);
		skillsModule.Refresh(showSpell);
		skillsModule.ClearList();
		currentSkillIndex = -1;
		wheelModule.SelectSlot(0, currentSkillActive: true);
		SetButtonsWithoutLearnSkill(inWheel: true);
	}

	public override Selectable ModuleLeftOnRight()
	{
		return (Selectable)(object)wheelModule.activeSkills[0].toggle.toggle;
	}

	private void UpdateWheel()
	{
		if (showSpell)
		{
			wheelModule.ShowSpells(ModuleCentertOnLeft(), OnWheelSkillSelected, OnActiveSkill);
		}
		else
		{
			wheelModule.ShowSkills(ModuleCentertOnLeft(), OnWheelSkillSelected, OnPassiveSkill, OnActiveSkill);
		}
	}

	private void SetButtonsWithLearnSkill()
	{
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_return_select_slot", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnSelectSlot, mouseOnly: false);
		PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("action", "menu_learn_skill");
		PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(SelectCurrentSkill, mouseOnly: false);
		SetupApplyButton(PandoraSingleton<HideoutTabManager>.Instance.button3);
		PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
	}

	private void SetButtonsWithoutLearnSkill(bool inWheel)
	{
		if (inWheel)
		{
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
			PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
			PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("action", "menu_select_slot");
			PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(null, mouseOnly: false);
			SetupApplyButton(PandoraSingleton<HideoutTabManager>.Instance.button3);
			PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_return_select_slot", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
			PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnSelectSlot, mouseOnly: false);
			SetupApplyButton(PandoraSingleton<HideoutTabManager>.Instance.button2);
			PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
		}
	}

	private void SetButtonsAttributeSelection()
	{
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
		SetupAttributeButtons(PandoraSingleton<HideoutTabManager>.Instance.button2, PandoraSingleton<HideoutTabManager>.Instance.button3, PandoraSingleton<HideoutTabManager>.Instance.button4);
		PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
	}

	private void OnSkillConfirmed(int index, bool active, SkillData skillData)
	{
		currentLearnSkill = skillData;
		if (skillsShop.CanLearnSkill(skillData))
		{
			PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_learn_skill_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_learn_skill_desc", SkillHelper.GetLocalizedName(skillData)), OnLearnSkillPopup);
		}
	}

	private void OnLearnSkillPopup(bool confirm)
	{
		if (confirm)
		{
			PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(warband.GetSkillLearnPrice(currentLearnSkill, PandoraSingleton<HideoutManager>.Instance.currentUnit.unit));
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
			currentUnit.unit.StartLearningSkill(currentLearnSkill, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, !PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider);
			if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
			{
				currentUnit.unit.EndLearnSkill();
			}
			UpdateWheel();
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			RefreshUnitAttributes();
			skillsModule.Refresh(showSpell);
			currentLearnSkill = null;
			ReturnSelectSlot();
		}
	}

	private void OnPassiveSkill(int index, SkillData skillData)
	{
		if (currentSkillIndex != index || currentSkillActive)
		{
			currentSkillIndex = index;
			currentSkillActive = false;
			if (skillData == null || SkillHelper.HasMastery(skillData))
			{
				CheckForChanges(delegate
				{
					OnApplyChanges();
					skillsModule.ShowSkills(OnSkillSelected, OnSkillConfirmed, skillData, active: false);
				});
				return;
			}
			skillsModule.ClearList();
			skillsModule.SetInteractable(interactable: false);
		}
	}

	private void OnActiveSkill(int index, SkillData skillData)
	{
		if (currentSkillIndex != index || !currentSkillActive)
		{
			currentSkillIndex = index;
			currentSkillActive = true;
			if (skillData == null || SkillHelper.HasMastery(skillData))
			{
				CheckForChanges(delegate
				{
					OnApplyChanges();
					skillsModule.ShowSkills(OnSkillSelected, OnSkillConfirmed, skillData, active: true);
				});
				return;
			}
			skillsModule.ClearList();
			skillsModule.SetInteractable(interactable: false);
		}
	}

	private void OnWheelSkillSelected(int idx, SkillData skillData)
	{
		if (skillData != null)
		{
			skillDescModule.gameObject.SetActive(value: true);
			descModule.gameObject.SetActive(value: false);
			skillDescModule.Set(skillData);
		}
		else
		{
			skillDescModule.gameObject.SetActive(value: false);
			descModule.gameObject.SetActive(value: false);
		}
		SetButtonsWithoutLearnSkill(inWheel: true);
	}

	private void OnSkillSelected(SkillData skillData)
	{
		if (skillData != null)
		{
			selectedSkill = skillData;
			skillDescModule.gameObject.SetActive(value: true);
			descModule.gameObject.SetActive(value: false);
			if (skillsShop.CanLearnSkill(skillData, out string reason))
			{
				SetButtonsWithLearnSkill();
			}
			else
			{
				SetButtonsWithoutLearnSkill(inWheel: false);
			}
			skillDescModule.Set(skillData, reason);
		}
		else
		{
			skillDescModule.gameObject.SetActive(value: false);
			descModule.gameObject.SetActive(value: false);
			SetButtonsWithoutLearnSkill(inWheel: false);
		}
	}

	protected override void ShowDescription(string title, string desc)
	{
		base.ShowDescription(title, desc);
		skillDescModule.gameObject.SetActive(value: false);
		SetButtonsAttributeSelection();
	}

	protected void ReturnSelectSlot()
	{
		int num = currentSkillIndex;
		currentSkillIndex = -1;
		wheelModule.SelectSlot(num, currentSkillActive);
		skillsModule.ClearList();
		skillsModule.RefreshUnspentPoints();
		SetButtonsWithoutLearnSkill(inWheel: true);
	}

	protected override void OnAttributeChanged()
	{
		base.OnAttributeChanged();
		currentSkillIndex = -1;
		skillsModule.ClearList();
	}

	public override bool CanIncreaseAttributes()
	{
		return true;
	}

	private void SelectCurrentSkill()
	{
		if (selectedSkill != null)
		{
			OnSkillConfirmed(-1, active: false, selectedSkill);
		}
	}
}
