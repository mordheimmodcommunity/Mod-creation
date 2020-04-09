using System.Collections.Generic;
using UnityEngine;

public class RuneMark
{
	private const string RUNE_NAME_PARAM = "item_enchant_type_rune_param";

	private const string MARK_NAME_PARAM = "item_enchant_type_mark_param";

	private const string RUNE_NAME = "item_enchant_type_rune";

	private const string MARK_NAME = "item_enchant_type_mark";

	private string fullLocName;

	private string locDesc;

	private string locShort;

	private string locQuality;

	private string suffixLocName;

	private string color;

	public string Name => Data.Name;

	public string FullLabel
	{
		get;
		set;
	}

	public string FullLocName
	{
		get
		{
			if (string.IsNullOrEmpty(fullLocName))
			{
				fullLocName = Color + PandoraSingleton<LocalizationManager>.Instance.GetStringById((AllegianceId != AllegianceId.ORDER) ? "item_enchant_type_mark_param" : "item_enchant_type_rune_param", SuffixLocName) + "</color>";
			}
			return fullLocName;
		}
	}

	public string LocDesc
	{
		get
		{
			if (string.IsNullOrEmpty(locDesc))
			{
				locDesc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_enchant_desc_" + Data.Name + ((QualityData.Id != RuneMarkQualityId.MASTER) ? string.Empty : "_mstr"));
			}
			return locDesc;
		}
	}

	public string LocShort
	{
		get
		{
			if (string.IsNullOrEmpty(locShort))
			{
				locShort = PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_enchant_short_" + Data.Name + ((QualityData.Id != RuneMarkQualityId.MASTER) ? string.Empty : "_mstr"));
			}
			return locShort;
		}
	}

	public string LocQuality
	{
		get
		{
			if (string.IsNullOrEmpty(locQuality))
			{
				locQuality = PandoraSingleton<LocalizationManager>.Instance.GetStringById("rune_quality_" + QualityData.Name);
			}
			return locQuality;
		}
	}

	public string SuffixLocName
	{
		get
		{
			if (string.IsNullOrEmpty(suffixLocName))
			{
				suffixLocName = PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName);
			}
			return suffixLocName;
		}
	}

	public string Reason
	{
		get;
		set;
	}

	public string Color
	{
		get
		{
			if (string.IsNullOrEmpty(color))
			{
				color = PandoraSingleton<LocalizationManager>.Instance.GetStringById((QualityData.Id != RuneMarkQualityId.REGULAR) ? "color_item_purple" : "color_item_blue");
			}
			return color;
		}
	}

	public string LabelName
	{
		get;
		set;
	}

	public RuneMarkData Data
	{
		get;
		private set;
	}

	public RuneMarkQualityData QualityData
	{
		get;
		private set;
	}

	public RuneMarkQualityJoinItemTypeData QualityItemTypeData
	{
		get;
		private set;
	}

	public List<RuneMarkAttributeData> AttributeModifiers
	{
		get;
		private set;
	}

	public List<Enchantment> Enchantments
	{
		get;
		private set;
	}

	public AllegianceId AllegianceId
	{
		get;
		private set;
	}

	public int Cost => Data.Cost * QualityItemTypeData.CostModifier;

	public RuneMark(RuneMarkId id, RuneMarkQualityId qualityId, AllegianceId allegianceId, ItemTypeId itemTypeId, string reason = null)
	{
		Reason = reason;
		AllegianceId = allegianceId;
		Data = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkData>((int)id);
		QualityData = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkQualityData>((int)qualityId);
		if (itemTypeId == ItemTypeId.NONE)
		{
			itemTypeId = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityJoinItemTypeData>("fk_item_type_id", QualityData.Id.ToIntString())[0].ItemTypeId;
		}
		List<RuneMarkQualityJoinItemTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkQualityJoinItemTypeData>(new string[2]
		{
			"fk_rune_mark_quality_id",
			"fk_item_type_id"
		}, new string[2]
		{
			((int)qualityId).ToConstantString(),
			((int)itemTypeId).ToConstantString()
		});
		QualityItemTypeData = list[0];
		AttributeModifiers = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkAttributeData>(new string[3]
		{
			"fk_rune_mark_id",
			"fk_rune_mark_quality_id",
			"fk_item_type_id"
		}, new string[3]
		{
			((int)Data.Id).ToConstantString(),
			((int)QualityData.Id).ToConstantString(),
			((int)itemTypeId).ToConstantString()
		});
		List<RuneMarkEnchantmentData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkEnchantmentData>(new string[3]
		{
			"fk_rune_mark_id",
			"fk_rune_mark_quality_id",
			"fk_item_type_id"
		}, new string[3]
		{
			((int)Data.Id).ToConstantString(),
			((int)QualityData.Id).ToConstantString(),
			((int)itemTypeId).ToConstantString()
		});
		Enchantments = new List<Enchantment>();
		for (int i = 0; i < list2.Count; i++)
		{
			Enchantments.Add(new Enchantment(list2[i].EnchantmentId, null, null, orig: true, innate: true, AllegianceId));
		}
		LabelName = "item_enchant_name_" + Data.Name + ((QualityData.Id != RuneMarkQualityId.MASTER) ? string.Empty : "_mstr");
		FullLabel = "#" + ((AllegianceId != AllegianceId.ORDER) ? "item_enchant_type_mark" : "item_enchant_type_rune") + " #" + LabelName;
	}

	public Sprite GetIcon()
	{
		return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("runemark/" + QualityData.Name, cached: true);
	}
}
