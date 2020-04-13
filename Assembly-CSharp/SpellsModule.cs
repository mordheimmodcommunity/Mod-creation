using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellsModule : UIModule
{
    private readonly SkillsShop skillsShop = new SkillsShop();

    public GameObject skillPrefab;

    public ScrollGroup scrollGroup;

    public Text unspentSkills;

    private List<SkillLineId> spellSkillLine;

    private Action<SkillData> onSpellSelectedCallback;

    private Action<SkillData> onSpellConfirmedCallback;

    private List<SkillData> canLearnSkills;

    private List<SkillData> cannotLearnSkills;

    public void Refresh()
    {
        spellSkillLine = skillsShop.GetUnitSpellsSkillLines(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit);
        if (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UnspentSpell > 0)
        {
            ((Component)(object)unspentSkills).gameObject.SetActive(value: true);
            unspentSkills.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_unspent_point", PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UnspentSpell.ToConstantString()));
        }
        else
        {
            ((Component)(object)unspentSkills).gameObject.SetActive(value: false);
        }
        canLearnSkills = new List<SkillData>();
        cannotLearnSkills = new List<SkillData>();
    }

    public void ShowSpells(Action<SkillData> onSpellSelected, Action<SkillData> onSpellConfirmed, SkillData currentSpell)
    {
        onSpellSelectedCallback = onSpellSelected;
        onSpellConfirmedCallback = onSpellConfirmed;
        scrollGroup.Setup(skillPrefab, hideBarIfEmpty: true);
        scrollGroup.ClearList();
        if (currentSpell == null)
        {
            skillsShop.GetSkills(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit, spellSkillLine, active: true, ref canLearnSkills, ref cannotLearnSkills);
            for (int i = 0; i < canLearnSkills.Count; i++)
            {
                SkillData skillData = canLearnSkills[i];
                AddSkill(skillData, canLearn: true, i == 0);
            }
            for (int j = 0; j < cannotLearnSkills.Count; j++)
            {
                SkillData skillData2 = cannotLearnSkills[j];
                AddSkill(skillData2, canLearn: false, canLearnSkills.Count == 0 && j == 0);
            }
        }
        else if (currentSpell.SkillQualityId == SkillQualityId.NORMAL_QUALITY)
        {
            SkillData skillMastery = SkillHelper.GetSkillMastery(currentSpell);
            if (skillMastery != null)
            {
                AddSkill(skillMastery, skillsShop.CanLearnSkill(skillMastery), select: true);
            }
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
            component.toggle.onAction.AddListener(delegate
            {
                onSpellConfirmedCallback(skillData);
            });
        }
        component.toggle.onSelect.AddListener(delegate
        {
            onSpellSelectedCallback(skillData);
        });
        component.Set(skillData, canLearn);
        if (select)
        {
            gameObject.SetSelected(force: true);
        }
    }

    public bool HasSpells()
    {
        return skillsShop.UnitHasSkillLine(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit, SkillLineId.SPELL);
    }

    public void ClearList()
    {
        scrollGroup.ClearList();
    }
}
