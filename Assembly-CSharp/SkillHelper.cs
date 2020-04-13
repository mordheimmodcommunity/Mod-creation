using System.Collections.Generic;
using UnityEngine;

public static class SkillHelper
{
    public static bool IsMastery(SkillData skillData)
    {
        return skillData.SkillQualityId == SkillQualityId.MASTER_QUALITY;
    }

    public static SkillData GetSkillMastery(SkillData skillData)
    {
        if (skillData.SkillQualityId == SkillQualityId.NORMAL_QUALITY)
        {
            List<SkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>("fk_skill_id_prerequiste", skillData.Id.ToIntString());
            SkillData result = null;
            if (list.Count > 0)
            {
                result = list[0];
            }
            return result;
        }
        return null;
    }

    public static List<SkillLearnBonusData> GetSkillLearnBonus(SkillId skillId)
    {
        return PandoraSingleton<DataFactory>.Instance.InitData<SkillLearnBonusData>("fk_skill_id", ((int)skillId).ToConstantString());
    }

    public static bool HasMastery(SkillData skillData)
    {
        return GetSkillMastery(skillData) != null;
    }

    public static string GetLocalizedMasteryDescription(SkillId skillId)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_desc_" + skillId.ToLowerString() + "_hideout");
    }

    public static string GetLocalizedRange(SkillData skillData)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_range", skillData.Range.ToConstantString());
    }

    public static string GetLocalizedCasting(SkillData skillData)
    {
        List<SkillAttributeData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillAttributeData>(new string[2]
        {
            "fk_skill_id",
            "fk_attribute_id"
        }, new string[2]
        {
            skillData.Id.ToIntString(),
            AttributeId.SPELLCASTING_ROLL.ToIntString()
        });
        if (list.Count > 0)
        {
            return list[0].Modifier.ToString("+#;-#") + "%";
        }
        return "0%";
    }

    public static string GetLocalizedDuration(SkillData skillData)
    {
        int num = 0;
        if (skillData.ZoneAoeId != 0)
        {
            ZoneAoeData zoneAoeData = PandoraSingleton<DataFactory>.Instance.InitData<ZoneAoeData>((int)skillData.ZoneAoeId);
            if (zoneAoeData != null)
            {
                num = zoneAoeData.Duration;
            }
        }
        else if (skillData.DestructibleId != 0)
        {
            DestructibleData destructibleData = PandoraSingleton<DataFactory>.Instance.InitData<DestructibleData>((int)skillData.DestructibleId);
            ZoneAoeData zoneAoeData2 = PandoraSingleton<DataFactory>.Instance.InitData<ZoneAoeData>((int)destructibleData.ZoneAoeId);
            if (zoneAoeData2 != null)
            {
                num = zoneAoeData2.Duration;
            }
        }
        else
        {
            List<SkillEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillEnchantmentData>("fk_skill_id", skillData.Id.ToIntString());
            for (int i = 0; i < list.Count; i++)
            {
                EnchantmentData enchantmentData = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)list[i].EnchantmentId);
                if (enchantmentData != null)
                {
                    num = Mathf.Max(num, enchantmentData.Duration);
                }
            }
        }
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_duration", num.ToConstantString());
    }

    public static string GetLocalizedCurse(SkillData skillData)
    {
        List<SkillAttributeData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillAttributeData>(new string[2]
        {
            "fk_skill_id",
            "fk_attribute_id"
        }, new string[2]
        {
            skillData.Id.ToIntString(),
            (skillData.SpellTypeId != SpellTypeId.ARCANE) ? AttributeId.DIVINE_WRATH_ROLL.ToIntString() : AttributeId.TZEENTCHS_CURSE_ROLL.ToIntString()
        });
        if (list.Count > 0)
        {
            return list[0].Modifier.ToString("+#;-#") + "%";
        }
        return "0%";
    }

    public static string GetLocalizedDescription(SkillId skillId)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_desc_" + skillId.ToLowerString());
    }

    public static string GetLocalizedName(SkillId skillId)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_" + skillId.ToLowerString());
    }

    public static string GetLocalizedName(SkillData skillData)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_" + skillData.Name);
    }

    public static string GetLocalizedRequirement(SkillData skillData)
    {
        if (skillData.AttributeIdStat == AttributeId.NONE)
        {
            return string.Empty;
        }
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_requirement", skillData.StatValue.ToConstantString(), "#attribute_name_" + skillData.AttributeIdStat.ToLowerString());
    }

    public static string GetLocalizedTrainingTime(SkillData skillData)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_training_time", skillData.Time.ToConstantString());
    }

    public static Sprite GetIcon(SkillData skillData)
    {
        Sprite icon = GetIcon(skillData.Name);
        if (icon == null)
        {
            icon = GetIcon(skillData.UnitActionId.ToLowerString());
        }
        return icon;
    }

    public static SkillData GetSkill(SkillId skillId)
    {
        return PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)skillId);
    }

    private static Sprite GetIcon(string name)
    {
        Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/" + name.ToLowerString(), cached: true);
        if (sprite == null)
        {
            sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/" + name.ToLowerInvariant().Replace("base_", string.Empty).Replace("_mstr", string.Empty), cached: true);
        }
        return sprite;
    }

    public static SkillLineId GetSkillLineId(SkillId skillId, UnitId unitId)
    {
        List<SkillLineJoinSkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineJoinSkillData>("fk_skill_id", skillId.ToIntString());
        if (list.Count == 1)
        {
            return list[0].SkillLineId;
        }
        if (list.Count > 1)
        {
            List<UnitJoinSkillLineData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkillLineData>("fk_unit_id", unitId.ToIntString());
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    if (list[i].SkillLineId == list2[j].SkillLineId)
                    {
                        return list[i].SkillLineId;
                    }
                }
            }
        }
        return SkillLineId.NONE;
    }

    public static SkillLineData GetBaseSkillLine(SkillData skillData, UnitId unitId)
    {
        SkillLineId skillLineId = GetSkillLineId(skillData.Id, unitId);
        SkillLineData skillLineData = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineData>((int)skillLineId);
        return PandoraSingleton<DataFactory>.Instance.InitData<SkillLineData>((int)skillLineData.SkillLineIdDisplayed);
    }

    public static int GetRating(SkillData skillData)
    {
        SkillQualityData skillQualityData = PandoraSingleton<DataFactory>.Instance.InitData<SkillQualityData>((int)skillData.SkillQualityId);
        return skillQualityData.Rating;
    }
}
