using UnityEngine;
using UnityEngine.UI;

public class UISkillItem : MonoBehaviour
{
	public ToggleEffects toggle;

	public Text skillName;

	public Image icon;

	public Image cannotLearnIcon;

	public Text pointCost;

	public Text goldCost;

	public Text ratingValue;

	public GameObject ratingSection;

	public Image mastery;

	public Image redSplatter;

	public CanvasGroup canvasGroup;

	public void Set(SkillData skillData, bool canLearnSkill)
	{
		skillName.set_text(SkillHelper.GetLocalizedName(skillData));
		if ((Object)(object)icon != null)
		{
			icon.set_sprite(SkillHelper.GetIcon(skillData));
		}
		if ((Object)(object)pointCost != null)
		{
			pointCost.set_text(skillData.Points.ToConstantString());
			((Graphic)pointCost).set_color((!PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.HasEnoughPointsForSkill(skillData)) ? Color.red : Color.white);
		}
		if ((Object)(object)goldCost != null)
		{
			int skillLearnPrice = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetSkillLearnPrice(skillData, PandoraSingleton<HideoutManager>.Instance.currentUnit.unit);
			goldCost.set_text(skillLearnPrice.ToConstantString());
			if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() < skillLearnPrice)
			{
				((Graphic)goldCost).set_color(Color.red);
			}
		}
		if ((Object)(object)ratingValue != null)
		{
			ratingValue.set_text(SkillHelper.GetRating(skillData).ToConstantString());
		}
		if ((Object)(object)cannotLearnIcon != null)
		{
			((Behaviour)(object)cannotLearnIcon).enabled = !canLearnSkill;
		}
		((Behaviour)(object)mastery).enabled = (skillData.SkillQualityId == SkillQualityId.MASTER_QUALITY);
		if ((Object)(object)redSplatter != null)
		{
			((Behaviour)(object)redSplatter).enabled = !canLearnSkill;
		}
	}

	public void Set(WarbandSkill skill)
	{
		skillName.set_text(skill.LocalizedName);
		if ((Object)(object)icon != null)
		{
			icon.set_sprite(skill.GetIcon());
		}
		if ((Object)(object)pointCost != null)
		{
			pointCost.set_text(skill.Data.Points.ToConstantString());
		}
		((Behaviour)(object)mastery).enabled = skill.IsMastery;
		if ((Object)(object)goldCost != null)
		{
			((Component)(object)goldCost).transform.parent.gameObject.SetActive(value: false);
		}
		if ((Object)(object)cannotLearnIcon != null)
		{
			((Behaviour)(object)cannotLearnIcon).enabled = !skill.CanBuy;
		}
		if ((Object)(object)redSplatter != null)
		{
			((Behaviour)(object)redSplatter).enabled = !skill.CanBuy;
		}
		if (ratingSection != null)
		{
			ratingSection.SetActive(value: false);
		}
	}
}
