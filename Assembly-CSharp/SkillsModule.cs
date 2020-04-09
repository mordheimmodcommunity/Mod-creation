using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsModule : UIModule
{
	private readonly SkillsShop skillsShop = new SkillsShop();

	public Text title;

	public GameObject skillPrefab;

	public ScrollGroup scrollGroup;

	public List<SkillLinesTab> skillLinesIcons;

	public GameObject unspentPoints;

	public Text pointsText;

	public Text emptyListMessage;

	public Text filterTitle;

	public Image warbandIcon;

	private int currentSkillLineIndex;

	private Dictionary<SkillLineId, List<SkillLineId>> skillLines;

	private List<SkillData> canLearnSkills;

	private List<SkillData> cannotLearnSkills;

	private Action<SkillData> onSkillSelectedCallback;

	private Action<int, bool, SkillData> onSkillConfirmedCallback;

	private bool isFocus;

	private bool showActiveSkill;

	private bool showMastery;

	private bool showSpell;

	public CanvasGroup skillLinesGroup;

	public ButtonGroup btnPreviousFilter;

	public ButtonGroup btnNextFilter;

	public override void Init()
	{
		base.Init();
		filterTitle.set_text(string.Empty);
		scrollGroup.Setup(skillPrefab, hideBarIfEmpty: true);
		canLearnSkills = new List<SkillData>();
		cannotLearnSkills = new List<SkillData>();
		btnPreviousFilter.SetAction("subfilter", null);
		btnPreviousFilter.OnAction(Next, mouseOnly: false);
	}

	public void Refresh(bool showOnlySpell)
	{
		showSpell = showOnlySpell;
		skillLinesGroup.gameObject.SetActive(!showSpell);
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!showSpell) ? "hideout_menu_unit_skills" : "hideout_menu_unit_spells"));
		skillLines = skillsShop.GetUnitSkillLines(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit);
		RefreshUnspentPoints();
		warbandIcon.set_sprite(Warband.GetIcon(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.WarbandId));
		if (showOnlySpell)
		{
			emptyListMessage.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_select_spell_slot"));
		}
		else
		{
			emptyListMessage.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_select_skill_slot"));
		}
		((Component)(object)emptyListMessage).gameObject.SetActive(value: true);
	}

	public void RefreshUnspentPoints()
	{
		int num = (!showSpell) ? PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UnspentSkill : PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UnspentSpell;
		if (num > 0)
		{
			unspentPoints.gameObject.SetActive(value: true);
			pointsText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!showSpell) ? "hideout_unspent_skill_point" : "hideout_unspent_spell_point", num.ToString()));
		}
		else
		{
			unspentPoints.gameObject.SetActive(value: false);
		}
	}

	public void ShowSkills(Action<SkillData> onSkillSelected, Action<int, bool, SkillData> onSkillConfirmed, SkillData currentSkill, bool active)
	{
		if (!showSpell)
		{
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!active) ? "skill_category_title_passive_skill" : "skill_category_title_active_skill"));
		}
		SetInteractable(interactable: true);
		isFocus = true;
		showActiveSkill = active;
		onSkillSelectedCallback = onSkillSelected;
		onSkillConfirmedCallback = onSkillConfirmed;
		scrollGroup.ClearList();
		if (currentSkill == null)
		{
			CanvasGroup canvasGroup = skillLinesGroup;
			bool flag = true;
			skillLinesGroup.blocksRaycasts = flag;
			canvasGroup.interactable = flag;
			showMastery = false;
			currentSkillLineIndex = 0;
			for (int i = 0; i < skillLinesIcons.Count; i++)
			{
				skillLinesIcons[i].image.enabled = true;
				skillLinesIcons[i].available = skillLines.ContainsKey(skillLinesIcons[i].skillLine);
				skillLinesIcons[i].image.onAction.RemoveAllListeners();
				SkillLineId skillLine = skillLinesIcons[i].skillLine;
				skillLinesIcons[i].image.onAction.AddListener(delegate
				{
					SelectSkillLine(skillLine);
				});
			}
			SelectSkillLine((!showSpell) ? skillLinesIcons[currentSkillLineIndex].skillLine : SkillLineId.SPELL);
		}
		else
		{
			CanvasGroup canvasGroup2 = skillLinesGroup;
			bool flag = false;
			skillLinesGroup.blocksRaycasts = flag;
			canvasGroup2.interactable = flag;
			if (!SkillHelper.IsMastery(currentSkill))
			{
				SkillData skillMastery = SkillHelper.GetSkillMastery(currentSkill);
				if (skillMastery != null)
				{
					if (showSpell)
					{
						AddSkill(skillMastery, skillsShop.CanLearnSkill(skillMastery), select: true);
					}
					else
					{
						SkillLineId skillLineId = SkillHelper.GetSkillLineId(skillMastery.Id, PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.Id);
						for (int j = 0; j < skillLinesIcons.Count; j++)
						{
							if (skillLines[skillLinesIcons[j].skillLine].Contains(skillLineId, SkillLineIdComparer.Instance))
							{
								showMastery = true;
								currentSkillLineIndex = j;
								skillLinesIcons[j].image.toggle.set_isOn(true);
								AddSkill(skillMastery, skillsShop.CanLearnSkill(skillMastery), select: true);
							}
						}
					}
				}
			}
		}
		((Component)(object)emptyListMessage).gameObject.SetActive(value: false);
	}

	public void SelectSkillLine(SkillLineId skillLine)
	{
		if (!showMastery && isFocus)
		{
			filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_skill_line_" + skillLine));
			for (int i = 0; i < skillLinesIcons.Count; i++)
			{
				if (skillLinesIcons[i].skillLine == skillLine)
				{
					currentSkillLineIndex = i;
					skillLinesIcons[i].image.toggle.set_isOn(true);
					break;
				}
			}
			scrollGroup.ClearList();
			skillsShop.GetSkills(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit, skillLines[skillLine], showActiveSkill, ref canLearnSkills, ref cannotLearnSkills);
			for (int j = 0; j < canLearnSkills.Count; j++)
			{
				AddSkill(canLearnSkills[j], canLearn: true, j == 0);
			}
			for (int k = 0; k < cannotLearnSkills.Count; k++)
			{
				AddSkill(cannotLearnSkills[k], canLearn: false, canLearnSkills.Count == 0 && k == 0);
			}
		}
		else
		{
			filterTitle.set_text(string.Empty);
		}
	}

	private void AddSkill(SkillData skillData, bool canLearn, bool select)
	{
		GameObject gameObject = scrollGroup.AddToList(null, null);
		UISkillItem component = gameObject.GetComponent<UISkillItem>();
		component.toggle.onAction.RemoveAllListeners();
		component.toggle.onSelect.RemoveAllListeners();
		if (canLearn)
		{
			int index = scrollGroup.items.Count - 1;
			bool isActive = showActiveSkill;
			component.toggle.onAction.AddListener(delegate
			{
				onSkillConfirmedCallback(index, isActive, skillData);
			});
		}
		component.toggle.onSelect.AddListener(delegate
		{
			onSkillSelectedCallback(skillData);
		});
		component.Set(skillData, canLearn);
		if (select)
		{
			gameObject.SetSelected(force: true);
		}
	}

	private void Next()
	{
		currentSkillLineIndex = ((currentSkillLineIndex + 1 < skillLinesIcons.Count) ? (currentSkillLineIndex + 1) : 0);
		if (skillLines.ContainsKey(skillLinesIcons[currentSkillLineIndex].skillLine))
		{
			SelectSkillLine(skillLinesIcons[currentSkillLineIndex].skillLine);
		}
		else
		{
			Next();
		}
	}

	private void Prev()
	{
		currentSkillLineIndex = ((currentSkillLineIndex - 1 < 0) ? (skillLinesIcons.Count - 1) : (currentSkillLineIndex - 1));
		if (skillLines.ContainsKey(skillLinesIcons[currentSkillLineIndex].skillLine))
		{
			SelectSkillLine(skillLinesIcons[currentSkillLineIndex].skillLine);
		}
		else
		{
			Prev();
		}
	}

	public void Update()
	{
		if (isFocus && !showMastery && !showSpell)
		{
			if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
			{
				Next();
			}
			else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
			{
				Prev();
			}
		}
	}

	public void ClearList()
	{
		isFocus = false;
		scrollGroup.ClearList();
		filterTitle.set_text(string.Empty);
		((Component)(object)emptyListMessage).gameObject.SetActive(value: true);
	}
}
