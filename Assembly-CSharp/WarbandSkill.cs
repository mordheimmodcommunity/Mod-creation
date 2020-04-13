using System.Collections.Generic;
using UnityEngine;

public class WarbandSkill
{
    private string locName;

    private string locDesc;

    public WarbandSkillId Id => Data.Id;

    public WarbandSkillTypeId TypeId => Data.WarbandSkillTypeId;

    public bool IsMastery => Data.SkillQualityId == SkillQualityId.MASTER_QUALITY;

    public WarbandSkillData Data
    {
        get;
        private set;
    }

    public List<WarbandEnchantment> Enchantments
    {
        get;
        private set;
    }

    public List<WarbandSkillUnitData> HireableUnits
    {
        get;
        private set;
    }

    public List<WarbandSkillUnitTypeRankData> UnitTypeRankDatas
    {
        get;
        private set;
    }

    public List<WarbandSkillWarbandContactData> ContactsData
    {
        get;
        private set;
    }

    public List<WarbandSkillMarketItemData> MarketItems
    {
        get;
        private set;
    }

    public bool CanBuy => Data.Points <= PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetPlayerSkillsAvailablePoints();

    public string LocalizedName
    {
        get
        {
            if (locName == null)
            {
                locName = PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_skill_title_" + Data.Name);
            }
            return locName;
        }
    }

    public string LocalizedDesc
    {
        get
        {
            if (locDesc == null)
            {
                locDesc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_skill_desc_" + Data.Name);
            }
            return locDesc;
        }
    }

    public WarbandSkill(WarbandSkillId id)
    {
        Data = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillData>((int)id);
        Enchantments = new List<WarbandEnchantment>();
        List<WarbandSkillWarbandEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillWarbandEnchantmentData>("fk_warband_skill_id", ((int)Id).ToString());
        HireableUnits = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillUnitData>("fk_warband_skill_id", ((int)Id).ToString());
        UnitTypeRankDatas = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillUnitTypeRankData>("fk_warband_skill_id", ((int)Id).ToString());
        ContactsData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillWarbandContactData>("fk_warband_skill_id", ((int)Id).ToString());
        MarketItems = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillMarketItemData>("fk_warband_skill_id", ((int)Id).ToString());
        for (int i = 0; i < list.Count; i++)
        {
            Enchantments.Add(new WarbandEnchantment(list[i].WarbandEnchantmentId));
        }
    }

    public WarbandSkillData GetSkillMastery()
    {
        if (Data.SkillQualityId == SkillQualityId.NORMAL_QUALITY)
        {
            List<WarbandSkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillData>("fk_warband_skill_id_prerequisite", Data.Id.ToIntString());
            if (list.Count > 0)
            {
                return list[0];
            }
        }
        return null;
    }

    public Sprite GetIcon()
    {
        if (Data.WarbandSkillIdPrerequisite != 0)
        {
            return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("warband_skill/" + Data.WarbandSkillIdPrerequisite.ToLowerString(), cached: true);
        }
        return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("warband_skill/" + Data.Name, cached: true);
    }

    public static Sprite GetIcon(WarbandSkill skill)
    {
        if (skill != null)
        {
            return skill.GetIcon();
        }
        return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item/add", cached: true);
    }

    public static string LocName(WarbandSkill skill)
    {
        if (skill != null)
        {
            return skill.LocalizedName;
        }
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_skill_slot_available_title");
    }

    public static string LocDesc(WarbandSkill skill)
    {
        if (skill != null)
        {
            return skill.LocalizedDesc;
        }
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_skill_slot_available_desc");
    }
}
