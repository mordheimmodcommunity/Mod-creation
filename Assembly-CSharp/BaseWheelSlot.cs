using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseWheelSlot<T> : MonoBehaviour
{
	public ToggleEffects toggle;

	public Image icon;

	public Image mastery;

	public Image inTraining;

	protected Action<int, T> onSkillSelectedCallback;

	protected Action<int, T> activeSkillsConfirmedCallback;

	protected T skillData;

	protected int skillIndex;

	public void Set(int index, T skill, Action<int, T> onSkillSelected, Action<int, T> activeSkillsConfirmed, bool isInTraining)
	{
		skillIndex = index;
		skillData = skill;
		activeSkillsConfirmedCallback = activeSkillsConfirmed;
		onSkillSelectedCallback = onSkillSelected;
		ResetListeners();
		toggle.onSelect.RemoveAllListeners();
		toggle.onSelect.AddListener(SkillSelected);
		toggle.onPointerEnter.RemoveAllListeners();
		toggle.onPointerEnter.AddListener(SkillSelected);
		toggle.onAction.RemoveAllListeners();
		toggle.onAction.AddListener(SkillConfirmed);
		icon.set_overrideSprite(GetIcon());
		((Behaviour)(object)mastery).enabled = IsMastery();
		if ((UnityEngine.Object)(object)inTraining != null)
		{
			((Behaviour)(object)icon).enabled = !isInTraining;
			((Behaviour)(object)inTraining).enabled = isInTraining;
		}
	}

	public void SkillSelected()
	{
		onSkillSelectedCallback(skillIndex, skillData);
	}

	public void SkillConfirmed()
	{
		activeSkillsConfirmedCallback(skillIndex, skillData);
	}

	public void ResetListeners()
	{
		toggle.onSelect.RemoveAllListeners();
	}

	protected abstract Sprite GetIcon();

	protected abstract bool IsMastery();
}
