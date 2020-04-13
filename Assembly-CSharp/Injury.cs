using System.Collections.Generic;

public class Injury
{
    private const string LOC_TITLE = "injury_name_{0}";

    private const string LOC_DESC = "injury_desc_{0}";

    public InjuryData Data
    {
        get;
        private set;
    }

    public List<InjuryJoinAttributeData> AttributeModifiers
    {
        get;
        private set;
    }

    public List<Enchantment> Enchantments
    {
        get;
        private set;
    }

    public string LocName
    {
        get;
        private set;
    }

    public string LocDesc
    {
        get;
        private set;
    }

    public string LabelName
    {
        get;
        private set;
    }

    public string LabelDesc
    {
        get;
        private set;
    }

    public Injury(InjuryId id, Unit unit)
    {
        Data = PandoraSingleton<DataFactory>.Instance.InitData<InjuryData>((int)id);
        AttributeModifiers = PandoraSingleton<DataFactory>.Instance.InitData<InjuryJoinAttributeData>("fk_injury_id", ((int)id).ToConstantString());
        List<InjuryJoinEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<InjuryJoinEnchantmentData>("fk_injury_id", ((int)id).ToConstantString());
        Enchantments = new List<Enchantment>();
        for (int i = 0; i < list.Count; i++)
        {
            Enchantments.Add(new Enchantment(list[i].EnchantmentId, unit, unit, orig: true, innate: false));
        }
        LabelName = $"injury_name_{Data.Name}";
        LocName = PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName);
        LabelDesc = $"injury_desc_{Data.Name}";
        LocDesc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelDesc);
    }
}
