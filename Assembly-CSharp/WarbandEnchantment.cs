using System.Collections.Generic;

public class WarbandEnchantment
{
	public WarbandEnchantmentId Id => Data.Id;

	public WarbandEnchantmentData Data
	{
		get;
		private set;
	}

	public List<WarbandEnchantmentAttributeData> Attributes
	{
		get;
		private set;
	}

	public List<WarbandEnchantmentWyrdstoneDensityModifierData> WyrdStoneDensityModifiers
	{
		get;
		private set;
	}

	public List<WarbandEnchantmentSearchDensityModifierData> SearchDensityModifiers
	{
		get;
		private set;
	}

	public List<WarbandEnchantmentMarketModifierData> MarketEventModifiers
	{
		get;
		private set;
	}

	public string LocalizedName => PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName);

	public string LabelName
	{
		get;
		private set;
	}

	public string LocalizedDescription => PandoraSingleton<LocalizationManager>.Instance.GetStringById("enchant_desc_" + Data.Name);

	public WarbandEnchantment(WarbandEnchantmentId warbandEnchantmentId)
	{
		Data = PandoraSingleton<DataFactory>.Instance.InitData<WarbandEnchantmentData>((int)warbandEnchantmentId);
		Attributes = PandoraSingleton<DataFactory>.Instance.InitData<WarbandEnchantmentAttributeData>("fk_warband_enchantment_id", warbandEnchantmentId.ToIntString());
		WyrdStoneDensityModifiers = PandoraSingleton<DataFactory>.Instance.InitData<WarbandEnchantmentWyrdstoneDensityModifierData>("fk_warband_enchantment_id", warbandEnchantmentId.ToIntString());
		SearchDensityModifiers = PandoraSingleton<DataFactory>.Instance.InitData<WarbandEnchantmentSearchDensityModifierData>("fk_warband_enchantment_id", warbandEnchantmentId.ToIntString());
		MarketEventModifiers = PandoraSingleton<DataFactory>.Instance.InitData<WarbandEnchantmentMarketModifierData>("fk_warband_enchantment_id", warbandEnchantmentId.ToIntString());
	}
}
