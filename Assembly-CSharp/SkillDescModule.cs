using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDescModule : UIModule
{
    public Image icon;

    public Image splatter;

    public Sprite splatterRed;

    public Image masteryIcon;

    public Text skillName;

    public GameObject activeStatGroup;

    public Text range;

    public Text duration;

    public Text effect;

    public Text requirement;

    public Text masteryEffect;

    public Text masteryRequirement;

    public Text learningTime;

    public Text skillType;

    public Text ratingValue;

    public List<Image> offensePoints;

    public List<Image> strategyPoints;

    public GameObject masteryGroup;

    public virtual void Set(SkillData skillData, string reason = null)
    {
        base.gameObject.SetActive(value: true);
        Unit unit = (!PandoraSingleton<HideoutManager>.Exists()) ? null : PandoraSingleton<HideoutManager>.Instance.currentUnit.unit;
        bool flag = unit?.HasSkillOrSpell(skillData.Id) ?? true;
        skillName.set_text(SkillHelper.GetLocalizedName(skillData));
        icon.set_sprite(SkillHelper.GetIcon(skillData));
        SkillData skillData2 = null;
        if (!flag && SkillHelper.IsMastery(skillData))
        {
            skillData2 = SkillHelper.GetSkill(skillData.SkillIdPrerequiste);
        }
        duration.set_text(SkillHelper.GetLocalizedDuration(skillData2 ?? skillData));
        range.set_text(SkillHelper.GetLocalizedRange(skillData2 ?? skillData));
        if ((Object)(object)ratingValue != null)
        {
            ratingValue.set_text(SkillHelper.GetRating(skillData2 ?? skillData).ToString());
        }
        effect.set_text(SkillHelper.GetLocalizedDescription((skillData2 ?? skillData).Id));
        if ((Object)(object)learningTime != null)
        {
            if (flag)
            {
                learningTime.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_training_learned"));
            }
            else
            {
                learningTime.set_text(SkillHelper.GetLocalizedTrainingTime(skillData));
            }
        }
        if ((Object)(object)requirement != null)
        {
            if (flag || (SkillHelper.IsMastery(skillData) && (unit.HasSkillOrSpell(skillData.SkillIdPrerequiste) || unit.UnitSave.skillInTrainingId == skillData.SkillIdPrerequiste)))
            {
                requirement.set_text(string.Empty);
            }
            else if (reason != null)
            {
                if (reason == "na_skill_attribute")
                {
                    requirement.set_text(SkillHelper.GetLocalizedRequirement(skillData));
                }
                else
                {
                    requirement.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(reason));
                }
            }
            else
            {
                requirement.set_text(string.Empty);
            }
            ((Component)(object)requirement).gameObject.SetActive(!string.IsNullOrEmpty(requirement.get_text()));
        }
        if ((Object)(object)splatter != null)
        {
            if (reason != null)
            {
                splatter.set_overrideSprite(splatterRed);
            }
            else
            {
                splatter.set_overrideSprite((Sprite)null);
            }
        }
        for (int i = 0; i < offensePoints.Count; i++)
        {
            ((Behaviour)(object)offensePoints[i]).enabled = (i < skillData.OffensePoints);
        }
        for (int j = 0; j < strategyPoints.Count; j++)
        {
            ((Behaviour)(object)strategyPoints[j]).enabled = (j < skillData.StrategyPoints);
        }
        ((Behaviour)(object)masteryIcon).enabled = SkillHelper.IsMastery(skillData);
        if (masteryGroup != null)
        {
            if (SkillHelper.IsMastery(skillData))
            {
                if (!flag)
                {
                    masteryGroup.SetActive(value: true);
                    masteryEffect.set_text(SkillHelper.GetLocalizedMasteryDescription(skillData.SkillIdPrerequiste));
                    if (reason != null)
                    {
                        if (reason == "na_skill_attribute")
                        {
                            masteryRequirement.set_text(SkillHelper.GetLocalizedRequirement(skillData));
                        }
                        else
                        {
                            masteryRequirement.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(reason));
                        }
                    }
                    else
                    {
                        masteryRequirement.set_text(string.Empty);
                    }
                }
                else
                {
                    masteryGroup.SetActive(value: false);
                }
            }
            else
            {
                SkillData skillMastery = SkillHelper.GetSkillMastery(skillData);
                if (skillMastery != null)
                {
                    masteryGroup.SetActive(value: true);
                    masteryEffect.set_text(SkillHelper.GetLocalizedMasteryDescription(skillData.Id));
                    masteryRequirement.set_text(SkillHelper.GetLocalizedRequirement(skillMastery));
                }
                else
                {
                    masteryGroup.SetActive(value: false);
                }
            }
            ((Component)(object)masteryRequirement).gameObject.SetActive(!string.IsNullOrEmpty(masteryRequirement.get_text()));
        }
        if (activeStatGroup != null)
        {
            activeStatGroup.SetActive(!skillData.Passive);
        }
        if ((Object)(object)skillType != null)
        {
            skillType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!skillData.Passive) ? "skill_active" : "skill_passive"));
        }
    }
}
