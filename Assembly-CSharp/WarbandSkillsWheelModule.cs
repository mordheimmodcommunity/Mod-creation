using System.Collections.Generic;
using UnityEngine.Events;

public class WarbandSkillsWheelModule : UIModule
{
	public List<WarbandSkillWheelSlot> slots;

	private int rank;

	private List<WarbandSkill> skills;

	private UnityAction<int, WarbandSkill> onSkillSelected;

	private UnityAction onSkillConfirmed;

	public void Set(UnityAction<int, WarbandSkill> skillSelected, UnityAction skillConfirmed)
	{
		onSkillSelected = skillSelected;
		onSkillConfirmed = skillConfirmed;
		base.gameObject.SetActive(value: true);
		rank = PandoraSingleton<GameManager>.Instance.Profile.Rank;
		skills = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetPlayerSkills();
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].Set(i, (i >= skills.Count) ? null : skills[i], OnSkillSelected, OnSkillConfirmed, isInTraining: false);
		}
	}

	private void OnSkillSelected(int idx, WarbandSkill skill)
	{
		if (onSkillSelected != null)
		{
			onSkillSelected(idx, skill);
		}
	}

	private void OnSkillConfirmed(int idx, WarbandSkill skill)
	{
		if (onSkillConfirmed != null)
		{
			onSkillConfirmed();
		}
	}
}
