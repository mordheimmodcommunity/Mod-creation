using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitDescriptionModule : UIModule
{
	public Selectable selectable;

	public Text description;

	public Text injuriesTabTitle;

	public GameObject unitDescriptionGroup;

	public GameObject listTitleGroup;

	public GameObject perksSection;

	public GameObject perkItem;

	public GameObject mutationItem;

	public GameObject historyTitleItem;

	public GameObject historyItem;

	public ScrollGroup listGroup;

	public ScrollGroup listGroupInjuryHistory;

	public List<ToggleEffects> tabs;

	private Unit currentUnit;

	private UnitDescriptionModuleTab currentTab = UnitDescriptionModuleTab.MAX_VALUE;

	private bool tabChangedThisFrame;

	public override void Init()
	{
		base.Init();
		listGroup.Setup(perkItem, hideBarIfEmpty: true);
		selectable = GetComponent<Selectable>();
		for (int i = 0; i < tabs.Count; i++)
		{
			UnitDescriptionModuleTab tab = (UnitDescriptionModuleTab)i;
			tabs[i].onAction.AddListener(delegate
			{
				SetTab(tab);
			});
			tabs[i].onSelect.AddListener(delegate
			{
				SetTab(tab);
			});
		}
		tabChangedThisFrame = false;
	}

	public void SetTab(UnitDescriptionModuleTab tab)
	{
		if (currentTab == tab)
		{
			return;
		}
		currentTab = tab;
		switch (tab)
		{
		case UnitDescriptionModuleTab.DESCRIPTION:
			unitDescriptionGroup.SetActive(value: true);
			listTitleGroup.SetActive(value: true);
			perksSection.SetActive(value: true);
			listGroupInjuryHistory.gameObject.SetActive(value: false);
			description.set_text(currentUnit.LocalizedDescription);
			break;
		case UnitDescriptionModuleTab.INJURIES_AND_MUTATIONS:
		{
			unitDescriptionGroup.SetActive(value: false);
			perksSection.SetActive(value: false);
			listTitleGroup.SetActive(value: false);
			listGroupInjuryHistory.gameObject.SetActive(value: true);
			listGroupInjuryHistory.Setup(mutationItem, hideBarIfEmpty: true);
			listGroupInjuryHistory.ClearList();
			for (int i = 0; i < currentUnit.Mutations.Count; i++)
			{
				GameObject gameObject2 = listGroupInjuryHistory.AddToList(null, null);
				InjuryMutationItem component2 = gameObject2.GetComponent<InjuryMutationItem>();
				component2.Set(currentUnit.Mutations[i]);
			}
			for (int j = 0; j < currentUnit.Injuries.Count; j++)
			{
				if (currentUnit.Injuries[j].Data.Id != InjuryId.LIGHT_WOUND)
				{
					GameObject gameObject3 = listGroupInjuryHistory.AddToList(null, null);
					InjuryMutationItem component3 = gameObject3.GetComponent<InjuryMutationItem>();
					component3.Set(currentUnit.Injuries[j]);
				}
			}
			if (listGroupInjuryHistory.items.Count == 0)
			{
				GameObject gameObject4 = listGroupInjuryHistory.AddToList(null, null);
				InjuryMutationItem component4 = gameObject4.GetComponent<InjuryMutationItem>();
				component4.Set(PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_no_injury"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_desc_no_injury"));
			}
			break;
		}
		case UnitDescriptionModuleTab.HISTORY:
		{
			unitDescriptionGroup.SetActive(value: false);
			listTitleGroup.SetActive(value: false);
			perksSection.SetActive(value: false);
			listGroupInjuryHistory.gameObject.SetActive(value: true);
			listGroupInjuryHistory.ClearList();
			MonthId monthId = MonthId.MAX_VALUE;
			for (int num = currentUnit.UnitSave.stats.history.Count - 1; num >= 0; num--)
			{
				if (currentUnit.UnitSave.stats.history[num].Item1 <= PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
				{
					Date date = new Date(currentUnit.UnitSave.stats.history[num].Item1);
					if (date.Month != monthId)
					{
						listGroupInjuryHistory.Setup(historyTitleItem, hideBarIfEmpty: true);
						monthId = date.Month;
						GameObject gameObject = listGroupInjuryHistory.AddToList(null, null);
						if (monthId == MonthId.NONE)
						{
							gameObject.GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_holiday_" + date.Holiday.ToLowerString()));
						}
						else
						{
							gameObject.GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_month_" + monthId.ToLowerString()));
						}
						listGroupInjuryHistory.Setup(historyItem, hideBarIfEmpty: true);
					}
					string text = string.Empty;
					switch (currentUnit.UnitSave.stats.history[num].Item2)
					{
					case EventLogger.LogEvent.HIRE:
						text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unit_history_hire");
						break;
					case EventLogger.LogEvent.INJURY:
					{
						InjuryId item = (InjuryId)currentUnit.UnitSave.stats.history[num].Item3;
						if (item != InjuryId.LIGHT_WOUND && item != InjuryId.FULL_RECOVERY && item != InjuryId.NEAR_DEATH && item != InjuryId.AMNESIA)
						{
							text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unit_history_injury", PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_" + item));
						}
						break;
					}
					case EventLogger.LogEvent.MUTATION:
						text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unit_history_mutation", PandoraSingleton<LocalizationManager>.Instance.GetStringById("mutation_name_" + (MutationId)currentUnit.UnitSave.stats.history[num].Item3));
						break;
					case EventLogger.LogEvent.RANK_ACHIEVED:
						text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unit_history_rank_up", currentUnit.UnitSave.stats.history[num].Item3.ToString());
						break;
					case EventLogger.LogEvent.SKILL:
					case EventLogger.LogEvent.SPELL:
						text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unit_history_training", SkillHelper.GetLocalizedName((SkillId)currentUnit.UnitSave.stats.history[num].Item3));
						break;
					case EventLogger.LogEvent.MEMORABLE_KILL:
						text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unit_history_kills", currentUnit.UnitSave.stats.history[num].Item3.ToString());
						break;
					}
					if (!string.IsNullOrEmpty(text))
					{
						WarbandHistoryItem component = listGroupInjuryHistory.AddToList(null, null).GetComponent<WarbandHistoryItem>();
						if (date.Day == 0)
						{
							component.date.set_text(string.Empty);
						}
						else
						{
							component.date.set_text(date.Day.ToString());
						}
						component.eventDesc.set_text(text);
					}
				}
			}
			break;
		}
		}
		listGroupInjuryHistory.RealignList(isOn: true, 0, force: true);
		tabChangedThisFrame = true;
	}

	public void Refresh(Unit unit, bool showCost)
	{
		currentUnit = unit;
		currentTab = UnitDescriptionModuleTab.MAX_VALUE;
		SetTab(UnitDescriptionModuleTab.DESCRIPTION);
		SetPerks();
		if (currentUnit.Mutations.Count > 0)
		{
			injuriesTabTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_injuries_and_mutations"));
		}
		else
		{
			injuriesTabTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_injuries"));
		}
	}

	private void SetPerks()
	{
		listGroup.ClearList();
		listGroup.Setup(perkItem, hideBarIfEmpty: true);
		List<UnitJoinPerkData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinPerkData>("fk_unit_id", currentUnit.Data.Id.ToIntString());
		for (int i = 0; i < list.Count; i++)
		{
			string str = list[i].PerkId.ToLowerString();
			GameObject gameObject = listGroup.AddToList(null, null);
			UIDescription component = gameObject.GetComponent<UIDescription>();
			component.Set("perk_name_" + str, "perk_desc_" + str);
		}
	}

	private void Update()
	{
		switch (currentTab)
		{
		case UnitDescriptionModuleTab.DESCRIPTION:
			CheckScroll(tabs[0], listGroup);
			break;
		case UnitDescriptionModuleTab.INJURIES_AND_MUTATIONS:
		case UnitDescriptionModuleTab.HISTORY:
			CheckScroll(tabs[(int)currentTab], listGroupInjuryHistory);
			break;
		}
		tabChangedThisFrame = false;
	}

	private void CheckScroll(ToggleEffects tab, ScrollGroup scrollGroup)
	{
		if (EventSystem.get_current().get_currentSelectedGameObject() == tab.gameObject && !tabChangedThisFrame && PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
		{
			float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("v");
			if (!Mathf.Approximately(axis, 0f) && Mathf.Abs(axis) > 0.8f)
			{
				scrollGroup.ForceScroll(axis < 0f, setSelected: false);
			}
		}
	}

	public void SetNav(Selectable left)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)tabs[0].toggle).get_navigation();
		((Navigation)(ref navigation)).set_selectOnLeft(left);
		((Selectable)tabs[0].toggle).set_navigation(navigation);
	}
}
