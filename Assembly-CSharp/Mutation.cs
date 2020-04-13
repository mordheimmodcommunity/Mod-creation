using System.Collections.Generic;
using UnityEngine;

public class Mutation
{
    private const string LOC_TITLE = "mutation_name_{0}";

    private const string LOC_DESC = "mutation_desc_{0}";

    private const string LOC_DESC_ITEM = "mutation_desc_{0}_{1}";

    private Unit owner;

    public MutationData Data
    {
        get;
        private set;
    }

    public MutationGroupData GroupData
    {
        get;
        private set;
    }

    public List<MutationAttributeData> AttributeModifiers
    {
        get;
        private set;
    }

    public List<Enchantment> Enchantments
    {
        get;
        private set;
    }

    public List<MutationGroupBodyPartData> RelatedBodyParts
    {
        get;
        private set;
    }

    public string LocName
    {
        get;
        private set;
    }

    public string LabelName
    {
        get;
        private set;
    }

    public string LocDesc
    {
        get
        {
            if (GroupData.UnitSlotId == UnitSlotId.SET1_MAINHAND || GroupData.UnitSlotId == UnitSlotId.SET1_OFFHAND)
            {
                ItemQualityId itemQualityId = owner.Items[(int)GroupData.UnitSlotId]?.QualityData.Id ?? ItemQualityId.NORMAL;
                return PandoraSingleton<LocalizationManager>.Instance.GetStringById($"mutation_desc_{Data.Name}_{itemQualityId}");
            }
            return PandoraSingleton<LocalizationManager>.Instance.GetStringById($"mutation_desc_{Data.Name}");
        }
    }

    public Mutation(MutationId id, Unit unit)
    {
        Data = PandoraSingleton<DataFactory>.Instance.InitData<MutationData>((int)id);
        GroupData = PandoraSingleton<DataFactory>.Instance.InitData<MutationGroupData>((int)Data.MutationGroupId);
        AttributeModifiers = PandoraSingleton<DataFactory>.Instance.InitData<MutationAttributeData>("fk_mutation_id", ((int)id).ToConstantString());
        RelatedBodyParts = PandoraSingleton<DataFactory>.Instance.InitData<MutationGroupBodyPartData>("fk_mutation_group_id", ((int)GroupData.Id).ToConstantString());
        List<MutationEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<MutationEnchantmentData>("fk_mutation_id", ((int)id).ToConstantString());
        Enchantments = new List<Enchantment>();
        for (int i = 0; i < list.Count; i++)
        {
            Enchantments.Add(new Enchantment(list[i].EnchantmentId, unit, unit, orig: true, innate: false));
        }
        owner = unit;
        LabelName = $"mutation_name_{Data.Name}";
        LocName = PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName);
    }

    public Sprite GetIcon()
    {
        return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("mutation/" + Data.Name, cached: true);
    }

    public bool HasBodyPart(BodyPartId partId)
    {
        for (int i = 0; i < RelatedBodyParts.Count; i++)
        {
            if (RelatedBodyParts[i].BodyPartId == partId)
            {
                return true;
            }
        }
        return false;
    }
}
