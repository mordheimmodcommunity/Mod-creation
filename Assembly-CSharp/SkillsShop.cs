using System.Collections.Generic;

public class SkillsShop
{
    private readonly BuySkillsComparer skillsComparer = new BuySkillsComparer();

    public void GetSkills(Unit unit, List<SkillLineId> skillLines, bool active, ref List<SkillData> canLearnSkills, ref List<SkillData> cannotLearnSkills)
    {
        canLearnSkills.Clear();
        cannotLearnSkills.Clear();
        for (int i = 0; i < skillLines.Count; i++)
        {
            List<SkillLineJoinSkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineJoinSkillData>("fk_skill_line_id", skillLines[i].ToIntString());
            for (int j = 0; j < list.Count; j++)
            {
                SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)list[j].SkillId);
                if (skillData.Passive != active && skillData.Released && skillData.SkillIdPrerequiste == SkillId.NONE && CanShowSkill(unit, skillData))
                {
                    if (CanLearnSkill(skillData))
                    {
                        canLearnSkills.Add(skillData);
                    }
                    else
                    {
                        cannotLearnSkills.Add(skillData);
                    }
                }
            }
        }
        canLearnSkills.Sort(skillsComparer);
        cannotLearnSkills.Sort(skillsComparer);
    }

    public bool UnitHasSkillLine(Unit unit, SkillLineId skillLineId)
    {
        List<UnitJoinSkillLineData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkillLineData>("fk_unit_id", unit.Id.ToIntString());
        foreach (UnitJoinSkillLineData item in list)
        {
            SkillLineData skillLineData = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineData>((int)item.SkillLineId);
            if (skillLineData.SkillLineIdDisplayed == skillLineId)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanLearnSkill(SkillData skillData)
    {
        string reason;
        return CanLearnSkill(skillData, out reason);
    }

    public bool IsSkillChangeType(SkillId skillId)
    {
        List<SkillLearnBonusData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillLearnBonusData>("fk_skill_id", skillId.ToIntString());
        if (list.Count == 1 && list[0].UnitTypeId != 0)
        {
            return true;
        }
        return false;
    }

    public bool CanLearnSkill(SkillData skillData, out string reason)
    {
        if (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.CanLearnSkill(skillData, out reason))
        {
            if (IsSkillChangeType(skillData.Id) && PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UnitSave.warbandSlotIndex < 12)
            {
                reason = "na_skill_warband_slot";
                return false;
            }
            if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetSkillLearnPrice(skillData, PandoraSingleton<HideoutManager>.Instance.currentUnit.unit) > PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
            {
                reason = "na_skill_not_enough_money";
                return false;
            }
        }
        return reason == null;
    }

    public bool CanShowSkill(Unit unit, SkillData skillData)
    {
        if (unit.HasSkillOrSpell(skillData.Id))
        {
            return false;
        }
        if (!SkillHelper.IsMastery(skillData))
        {
            SkillData skillMastery = SkillHelper.GetSkillMastery(skillData);
            if (skillMastery != null && unit.HasSkillOrSpell(skillMastery.Id))
            {
                return false;
            }
        }
        if (skillData.SkillIdPrerequiste != 0 && !unit.HasSkillOrSpell(skillData.SkillIdPrerequiste))
        {
            return false;
        }
        return true;
    }

    public Dictionary<SkillLineId, List<SkillLineId>> GetUnitSkillLines(Unit unit)
    {
        Dictionary<SkillLineId, List<SkillLineId>> dictionary = new Dictionary<SkillLineId, List<SkillLineId>>();
        List<UnitJoinSkillLineData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkillLineData>("fk_unit_id", unit.Id.ToIntString());
        foreach (UnitJoinSkillLineData item in list)
        {
            AddSkillLineToDictionary(item.SkillLineId, dictionary);
        }
        List<UnitTypeJoinSkillLineData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeJoinSkillLineData>("fk_unit_type_id", unit.GetUnitTypeId().ToIntString());
        foreach (UnitTypeJoinSkillLineData item2 in list2)
        {
            AddSkillLineToDictionary(item2.SkillLineId, dictionary);
        }
        return dictionary;
    }

    private static void AddSkillLineToDictionary(SkillLineId skillLineId, Dictionary<SkillLineId, List<SkillLineId>> dict)
    {
        SkillLineData skillLineData = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineData>((int)skillLineId);
        if (!dict.TryGetValue(skillLineData.SkillLineIdDisplayed, out List<SkillLineId> value))
        {
            value = new List<SkillLineId>();
            dict.Add(skillLineData.SkillLineIdDisplayed, value);
        }
        value.Add(skillLineData.Id);
    }

    public List<SkillLineId> GetUnitSpellsSkillLines(Unit unit)
    {
        List<SkillLineId> list = new List<SkillLineId>();
        List<UnitJoinSkillLineData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkillLineData>("fk_unit_id", unit.Id.ToIntString());
        foreach (UnitJoinSkillLineData item in list2)
        {
            SkillLineData skillLineData = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineData>((int)item.SkillLineId);
            if (skillLineData.SkillLineIdDisplayed == SkillLineId.SPELL)
            {
                list.Add(skillLineData.Id);
            }
        }
        return list;
    }
}
