using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsWheelModule : UIModule
{
    public Text passiveTitle;

    public GameObject passiveSkillsGo;

    public List<SkillWheelSlot> passiveSkills;

    public Text activeTitle;

    public GameObject activeSkillsGo;

    public List<SkillWheelSlot> activeSkills;

    private Action<int, SkillData> onSkillSelectedCallback;

    public void ShowSkills(ToggleEffects left, Action<int, SkillData> onSkillSelected, Action<int, SkillData> passiveSkillConfirmed, Action<int, SkillData> activeSkillsConfirmed)
    {
        //IL_0299: Unknown result type (might be due to invalid IL or missing references)
        //IL_029e: Unknown result type (might be due to invalid IL or missing references)
        //IL_02c4: Unknown result type (might be due to invalid IL or missing references)
        onSkillSelectedCallback = onSkillSelected;
        passiveTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_category_title_passive_skill"));
        passiveSkillsGo.SetActive(value: true);
        Unit unit = PandoraSingleton<HideoutManager>.Instance.currentUnit.unit;
        SkillData skillData = null;
        if (unit.UnitSave.skillInTrainingId != 0)
        {
            skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)unit.UnitSave.skillInTrainingId);
            if (skillData.SpellTypeId != 0)
            {
                skillData = null;
            }
        }
        for (int i = 0; i < passiveSkills.Count; i++)
        {
            if (unit.PassiveSkills.Count == i && skillData != null && skillData.Passive)
            {
                passiveSkills[i].Set(i, skillData, onSkillSelectedCallback, passiveSkillConfirmed, isInTraining: true);
            }
            else if (i < unit.PassiveSkills.Count && skillData != null && skillData.SkillIdPrerequiste == unit.PassiveSkills[i].Id)
            {
                passiveSkills[i].Set(i, skillData, onSkillSelectedCallback, passiveSkillConfirmed, isInTraining: true);
                skillData = null;
            }
            else
            {
                passiveSkills[i].Set(i, (i >= unit.PassiveSkills.Count) ? null : unit.PassiveSkills[i], onSkillSelectedCallback, passiveSkillConfirmed, isInTraining: false);
            }
        }
        activeTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_category_title_active_skill"));
        activeSkillsGo.SetActive(value: true);
        for (int j = 0; j < activeSkills.Count; j++)
        {
            if (unit.ActiveSkills.Count == j && skillData != null && !skillData.Passive)
            {
                activeSkills[j].Set(j, skillData, onSkillSelectedCallback, activeSkillsConfirmed, isInTraining: true);
            }
            else if (j < unit.ActiveSkills.Count && skillData != null && skillData.SkillIdPrerequiste == unit.ActiveSkills[j].Id)
            {
                activeSkills[j].Set(j, skillData, onSkillSelectedCallback, activeSkillsConfirmed, isInTraining: true);
                skillData = null;
            }
            else
            {
                activeSkills[j].Set(j, (j >= unit.ActiveSkills.Count) ? null : unit.ActiveSkills[j], onSkillSelectedCallback, activeSkillsConfirmed, isInTraining: false);
            }
            Navigation navigation = ((Selectable)activeSkills[j].toggle.toggle).get_navigation();
            ((Navigation)(ref navigation)).set_selectOnLeft((Selectable)(object)left.toggle);
            ((Selectable)activeSkills[j].toggle.toggle).set_navigation(navigation);
        }
    }

    public void ShowSpells(ToggleEffects left, Action<int, SkillData> onSkillSelected, Action<int, SkillData> spellConfirmed)
    {
        //IL_0176: Unknown result type (might be due to invalid IL or missing references)
        //IL_017b: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a0: Unknown result type (might be due to invalid IL or missing references)
        Unit unit = PandoraSingleton<HideoutManager>.Instance.currentUnit.unit;
        onSkillSelectedCallback = onSkillSelected;
        passiveSkillsGo.SetActive(value: false);
        activeTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_category_title_spells"));
        activeSkillsGo.SetActive(value: true);
        SkillData skillData = null;
        if (unit.UnitSave.skillInTrainingId != 0)
        {
            skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)unit.UnitSave.skillInTrainingId);
            if (skillData.SpellTypeId == SpellTypeId.NONE)
            {
                skillData = null;
            }
        }
        for (int i = 0; i < activeSkills.Count; i++)
        {
            if (unit.Spells.Count == i && skillData != null && !skillData.Passive)
            {
                activeSkills[i].Set(i, skillData, onSkillSelectedCallback, spellConfirmed, isInTraining: true);
            }
            else if (i < unit.Spells.Count && skillData != null && skillData.SkillIdPrerequiste == unit.Spells[i].Id)
            {
                activeSkills[i].Set(i, skillData, onSkillSelectedCallback, spellConfirmed, isInTraining: true);
                skillData = null;
            }
            else
            {
                activeSkills[i].Set(i, (i >= unit.Spells.Count) ? null : unit.Spells[i], onSkillSelectedCallback, spellConfirmed, isInTraining: false);
            }
            Navigation navigation = ((Selectable)activeSkills[i].toggle.toggle).get_navigation();
            ((Navigation)(ref navigation)).set_selectOnLeft((Selectable)(object)left.toggle);
            ((Selectable)activeSkills[i].toggle.toggle).set_navigation(navigation);
        }
    }

    public void Deactivate()
    {
        for (int i = 0; i < passiveSkills.Count; i++)
        {
            passiveSkills[i].ResetListeners();
        }
        for (int j = 0; j < activeSkills.Count; j++)
        {
            activeSkills[j].ResetListeners();
        }
    }

    public void SelectSlot(int currentSkillIndex, bool currentSkillActive)
    {
        if (currentSkillActive)
        {
            activeSkills[currentSkillIndex].toggle.SetSelected();
        }
        else
        {
            passiveSkills[currentSkillIndex].toggle.SetSelected();
        }
    }
}
