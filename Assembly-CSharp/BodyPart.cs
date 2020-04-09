using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BodyPart
{
	public const int PRESET_OFFSET = 8;

	private const string ASSET_BUNDLE_NAME = "{0}_{1}";

	private const string INJURY_NAME = "{0}_{1}_{2}_injury_{3}";

	private const string MUTATION_NAME = "{0}_{1}_{2}_{3}_{4}";

	private const string BODY_PART_NAME = "{0}_{1}_{2}";

	private const string BODY_PART_ITEM_NAME = "{0}_{1}_{2}_{3}";

	private const string MAT_SKIN_COLOR = "{0}_{1}_{2}";

	private const string MAT_COLOR = "{0}_{1}";

	private const string SEPARATOR = " , ";

	private static StringBuilder LogBldr = new StringBuilder();

	private static string[] BODY_PART_ITEM_PRESET_COLOR_IDS = new string[4]
	{
		"fk_unit_id",
		"fk_color_preset_id",
		"fk_body_part_id",
		"fk_item_type_id"
	};

	private List<string> models;

	private List<string> materials;

	public List<GameObject> relatedGO;

	private BodyPartData data;

	private UnitId relatedUnitId;

	private Item relatedItem;

	private int colorIdx;

	private string warband = string.Empty;

	private string unit = string.Empty;

	private string altUnit = string.Empty;

	private int variation;

	private string skinColor = string.Empty;

	private string altSkinColor = string.Empty;

	private bool needAssetRefresh;

	private string assetName;

	private string materialName;

	private string[] bodyPartsValues;

	private string[] bodyPartsValuesNoPreset;

	private bool locked;

	public BodyPartId Id => data.Id;

	public string Name => data.Name;

	public bool Empty
	{
		get;
		private set;
	}

	public bool Customizable => !locked && MutationId == MutationId.NONE && InjuryId == InjuryId.NONE;

	public bool AssetNeedReload
	{
		get;
		set;
	}

	public string AssetBundle
	{
		get;
		private set;
	}

	public MutationId MutationId
	{
		get;
		private set;
	}

	public InjuryId InjuryId
	{
		get;
		private set;
	}

	public string Color
	{
		get;
		private set;
	}

	public BodyPart(BodyPartData partData, UnitId unitId, string warbandAsset, string unitAsset, string altUnitAsset, string unitSkinColor, int colorIndex, int var)
	{
		data = partData;
		relatedUnitId = unitId;
		warband = warbandAsset;
		unit = unitAsset;
		altUnit = altUnitAsset;
		variation = var;
		colorIdx = colorIndex;
		Empty = data.Empty;
		needAssetRefresh = true;
		AssetNeedReload = true;
		relatedGO = new List<GameObject>();
		SetSkinColor(unitSkinColor);
		BodyPartId enumValue = (data.Id != BodyPartId.LLEGR) ? data.Id : BodyPartId.LEGR;
		bodyPartsValues = new string[4]
		{
			unitId.ToIntString(),
			((ColorPresetId)(colorIndex >> 8)).ToIntString(),
			enumValue.ToIntString(),
			ItemTypeId.NONE.ToIntString()
		};
		bodyPartsValuesNoPreset = new string[3]
		{
			unitId.ToIntString(),
			enumValue.ToIntString(),
			ItemTypeId.NONE.ToIntString()
		};
		locked = false;
		string key = warbandAsset + unitAsset;
		models = BodyPartModelIdData.Data[key];
		materials = BodyPartMaterialIdData.Data[key];
	}

	public void DestroyRelatedGO()
	{
		if (relatedGO.Count > 0)
		{
			for (int i = 0; i < relatedGO.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(relatedGO[i]);
			}
			relatedGO.Clear();
		}
	}

	public void SetLocked(bool locked)
	{
		this.locked = locked;
		SetEmpty(locked);
	}

	public void SetEmpty(bool empty)
	{
		Empty = empty;
		AssetNeedReload = true;
	}

	public void SetInjury(InjuryId injId)
	{
		InjuryId = injId;
		Empty = false;
		needAssetRefresh = true;
		AssetNeedReload = true;
	}

	public void SetMutation(MutationId mutId)
	{
		MutationId = mutId;
		Empty = false;
		needAssetRefresh = true;
		AssetNeedReload = true;
	}

	public void SetVariation(int index)
	{
		if (index != variation)
		{
			variation = index;
			Empty = false;
			AssetNeedReload = true;
			needAssetRefresh = true;
		}
	}

	public int GetVariation()
	{
		return variation;
	}

	public void SetRelatedItem(Item item)
	{
		if (relatedItem == null || item.Id != relatedItem.Id)
		{
			relatedItem = item;
			Empty = (MutationId == MutationId.NONE && InjuryId == InjuryId.NONE && data.UnitSlotId != UnitSlotId.NONE && item.Id == ItemId.NONE);
			locked = Empty;
			bodyPartsValues[3] = relatedItem.TypeData.Id.ToIntString();
			bodyPartsValuesNoPreset[2] = relatedItem.TypeData.Id.ToIntString();
			needAssetRefresh = true;
			AssetNeedReload = true;
		}
	}

	public Item GetRelatedItem()
	{
		return relatedItem;
	}

	public string GetAsset(ItemTypeId preferredItemType)
	{
		if (Empty)
		{
			return string.Empty;
		}
		if (!needAssetRefresh)
		{
			return assetName;
		}
		LogBldr.Length = 0;
		Empty = false;
		assetName = string.Empty;
		AssetBundle = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}", warband, unit).ToString();
		if (InjuryId != 0)
		{
			string text = "01";
			string asset = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}_injury_{3}", warband, unit, data.Name, text).ToString();
			assetName = FindBodyPart(asset);
			if (string.IsNullOrEmpty(assetName))
			{
				asset = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}_injury_{3}", warband, altUnit, data.Name, text).ToString();
				assetName = FindBodyPart(asset);
			}
			if (string.IsNullOrEmpty(assetName))
			{
				Empty = true;
				return string.Empty;
			}
			if (assetName.Contains("00"))
			{
				Empty = true;
				return string.Empty;
			}
		}
		if (MutationId != 0)
		{
			string text2 = "01";
			string text3 = MutationId.ToLowerString();
			string asset2 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}_{3}_{4}", warband, unit, data.Name, text3, text2).ToString();
			assetName = FindBodyPart(asset2);
			if (string.IsNullOrEmpty(assetName))
			{
				text3 = text3.Substring(0, text3.LastIndexOf("_"));
				asset2 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}_{3}_{4}", warband, unit, data.Name, text3, text2).ToString();
				assetName = FindBodyPart(asset2);
			}
			if (string.IsNullOrEmpty(assetName))
			{
				Empty = true;
				return string.Empty;
			}
			if (assetName.Contains("00"))
			{
				Empty = true;
				return string.Empty;
			}
		}
		if (string.IsNullOrEmpty(assetName))
		{
			List<string> availableModels = GetAvailableModels();
			if (availableModels.Count > 0)
			{
				if (variation == -1 || variation >= availableModels.Count)
				{
					List<string> list = new List<string>();
					for (int i = 0; i < availableModels.Count; i++)
					{
						if (availableModels[i].Contains("01") && !availableModels[i].Contains("injury") && !availableModels[i].Contains("mutation"))
						{
							list.Add(availableModels[i]);
						}
					}
					if (list.Count > 0)
					{
						assetName = list[0];
						string value = string.Empty;
						switch (preferredItemType)
						{
						case ItemTypeId.HEAVY_ARMOR:
							value = "armorh";
							break;
						case ItemTypeId.LIGHT_ARMOR:
							value = "armorl";
							break;
						case ItemTypeId.CLOTH_ARMOR:
							value = "cloth";
							break;
						}
						if (!string.IsNullOrEmpty(value))
						{
							for (int j = 0; j < list.Count; j++)
							{
								if (list[j].Contains(value))
								{
									assetName = list[j];
									break;
								}
							}
						}
					}
					if (string.IsNullOrEmpty(assetName))
					{
						assetName = availableModels[0];
					}
				}
				else
				{
					assetName = availableModels[variation];
				}
			}
		}
		if (assetName.Contains(altUnit))
		{
			AssetBundle = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}", warband, altUnit).ToString();
		}
		if (assetName.Contains("armorh"))
		{
			bodyPartsValues[3] = ItemTypeId.HEAVY_ARMOR.ToIntString();
		}
		else if (assetName.Contains("armorl"))
		{
			bodyPartsValues[3] = ItemTypeId.LIGHT_ARMOR.ToIntString();
		}
		else if (assetName.Contains("cloth"))
		{
			bodyPartsValues[3] = ItemTypeId.CLOTH_ARMOR.ToIntString();
		}
		if (!string.IsNullOrEmpty(assetName))
		{
			if (assetName.Contains("00"))
			{
				Empty = true;
				return string.Empty;
			}
			Empty = false;
			return GetMaterial();
		}
		Empty = true;
		return string.Empty;
	}

	public List<string> GetAvailableModels()
	{
		List<string> foundModels = new List<string>();
		string text = null;
		if (relatedItem != null && relatedItem.Id != 0 && (Id == BodyPartId.BODY || Id == BodyPartId.GEAR_BODY || (!relatedItem.Asset.Contains("armorh") && !relatedItem.Asset.Contains("armorl") && !relatedItem.Asset.Contains("cloth"))))
		{
			text = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}_{3}", warband, unit, data.Name, relatedItem.Asset).ToString();
			InitDataLike(ref foundModels, text, models, clean: true, includeAltArmor: false);
			if (unit != altUnit)
			{
				text = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}_{3}", warband, altUnit, data.Name, relatedItem.Asset).ToString();
				InitDataLike(ref foundModels, text, models, clean: true, includeAltArmor: false);
			}
		}
		bool includeAltArmor = Id != BodyPartId.BODY && Id != BodyPartId.GEAR_BODY;
		text = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}", warband, unit, data.Name).ToString();
		InitDataLike(ref foundModels, text, models, clean: true, includeAltArmor);
		if (unit != altUnit)
		{
			text = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}", warband, altUnit, data.Name).ToString();
			InitDataLike(ref foundModels, text, models, clean: true, includeAltArmor);
		}
		return foundModels;
	}

	private List<string> InitDataLike(ref List<string> foundModels, string name, List<string> list, bool clean, bool includeAltArmor)
	{
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			if (!text.StartsWith(name, StringComparison.Ordinal))
			{
				continue;
			}
			if (clean && text[name.Length + 1] != '0')
			{
				string text2 = text.Substring(name.Length);
				if (includeAltArmor && (text2.Contains("armorh") || text2.Contains("armorl") || text2.Contains("cloth")))
				{
					foundModels.Add(text);
				}
			}
			else
			{
				foundModels.Add(text);
			}
		}
		return foundModels;
	}

	private string FindBodyPart(string asset)
	{
		if (asset.EndsWith("00"))
		{
			return null;
		}
		for (int i = 0; i < models.Count; i++)
		{
			if (string.Compare(models[i], asset, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return asset;
			}
		}
		return null;
	}

	public void SetSkinColor(string skinColor)
	{
		if (skinColor != this.skinColor)
		{
			this.skinColor = skinColor;
			int num = skinColor.IndexOf('_');
			if (num != -1)
			{
				altSkinColor = skinColor.Substring(0, num);
			}
			else
			{
				altSkinColor = string.Empty;
			}
			needAssetRefresh = true;
			AssetNeedReload = true;
		}
	}

	public void SetColorPreset(int offsetPreset)
	{
		if (offsetPreset != colorIdx)
		{
			colorIdx = offsetPreset;
			bodyPartsValues[1] = ((ColorPresetId)(colorIdx >> 8)).ToIntString();
			needAssetRefresh = true;
			AssetNeedReload = true;
		}
	}

	public void SetColorOverride(int colorIndex)
	{
		if (colorIdx != colorIndex)
		{
			colorIdx = colorIndex;
			needAssetRefresh = true;
			AssetNeedReload = true;
		}
	}

	public int GetColorIndex()
	{
		return colorIdx;
	}

	private string GetMaterial()
	{
		if (Empty)
		{
			return string.Empty;
		}
		LogBldr.Length = 0;
		materialName = string.Empty;
		Color = string.Empty;
		if (colorIdx >= 256)
		{
			List<BodyPartColorData> list = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartColorData>(BODY_PART_ITEM_PRESET_COLOR_IDS, bodyPartsValues);
			if (list.Count > 0)
			{
				Color = list[0].Color;
			}
		}
		else
		{
			List<string> availableMaterials = GetAvailableMaterials(includeInjuries: true);
			if (colorIdx >= availableMaterials.Count)
			{
				colorIdx = 0;
			}
			if (availableMaterials.Count > 0)
			{
				Color = availableMaterials[colorIdx];
				materialName = FindMaterial(Color);
			}
		}
		string text = assetName;
		if (InjuryId == InjuryId.NONE && MutationId == MutationId.NONE && data.Id == BodyPartId.LLEGR)
		{
			text = assetName.Replace("lleg", "leg");
		}
		if (string.IsNullOrEmpty(materialName) && data.Skinnable && !string.IsNullOrEmpty(skinColor) && !string.IsNullOrEmpty(Color))
		{
			string text2 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}", text, skinColor, Color).ToString();
			materialName = FindMaterial(text2);
			LogBldr.Append(text2);
		}
		if (string.IsNullOrEmpty(materialName) && data.Skinnable && !string.IsNullOrEmpty(altSkinColor) && !string.IsNullOrEmpty(Color))
		{
			string text3 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}_{2}", text, altSkinColor, Color).ToString();
			materialName = FindMaterial(text3);
			LogBldr.Append(" , ");
			LogBldr.Append(text3);
		}
		if (string.IsNullOrEmpty(materialName) && data.Skinnable && !string.IsNullOrEmpty(skinColor))
		{
			string text4 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}", text, skinColor).ToString();
			materialName = FindMaterial(text4);
			LogBldr.Append(" , ");
			LogBldr.Append(text4);
		}
		if (string.IsNullOrEmpty(materialName) && data.Skinnable && !string.IsNullOrEmpty(altSkinColor))
		{
			string text5 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}", text, altSkinColor).ToString();
			materialName = FindMaterial(text5);
			LogBldr.Append(" , ");
			LogBldr.Append(text5);
		}
		if (string.IsNullOrEmpty(materialName) && !string.IsNullOrEmpty(Color))
		{
			string text6 = PandoraUtils.StringBuilder.AppendFormat("{0}_{1}", text, Color).ToString();
			materialName = FindMaterial(text6);
			LogBldr.Append(" , ");
			LogBldr.Append(text6);
		}
		if (string.IsNullOrEmpty(materialName))
		{
			string text7 = text;
			materialName = FindMaterial(text7);
			LogBldr.Append(" , ");
			LogBldr.Append(text7);
		}
		if (!string.IsNullOrEmpty(materialName))
		{
			needAssetRefresh = false;
			if (InjuryId == InjuryId.NONE && MutationId == MutationId.NONE && data.Id == BodyPartId.LLEGR)
			{
				materialName = materialName.Replace("leg", "lleg");
			}
			return materialName;
		}
		if (colorIdx >= 256)
		{
			colorIdx = 0;
			return GetMaterial();
		}
		Empty = true;
		return string.Empty;
	}

	public List<string> GetAvailableMaterials(bool includeInjuries)
	{
		if (string.IsNullOrEmpty(assetName))
		{
			return new List<string>();
		}
		string name = assetName;
		if (InjuryId == InjuryId.NONE && MutationId == MutationId.NONE && data.Id == BodyPartId.LLEGR)
		{
			name = assetName.Replace("lleg", "leg");
		}
		List<string> foundModels = new List<string>();
		InitDataLike(ref foundModels, name, materials, clean: false, includeAltArmor: false);
		if (!includeInjuries)
		{
			for (int num = foundModels.Count - 1; num >= 0; num--)
			{
				if (foundModels[num].Contains("injury"))
				{
					foundModels.RemoveAt(num);
				}
			}
		}
		if (!string.IsNullOrEmpty(skinColor) && data.Skinnable)
		{
			List<string> list = new List<string>();
			for (int num2 = foundModels.Count - 1; num2 >= 0; num2--)
			{
				if (!foundModels[num2].Contains("skin") || (foundModels[num2].Contains(skinColor) && skinColor.Contains("pimple") == foundModels[num2].Contains("pimple")))
				{
					list.Add(foundModels[num2]);
				}
			}
			if (list.Count == 0 && !string.IsNullOrEmpty(altSkinColor))
			{
				for (int num3 = foundModels.Count - 1; num3 >= 0; num3--)
				{
					if (foundModels[num3].Contains(altSkinColor) && altSkinColor.Contains("pimple") == foundModels[num3].Contains("pimple"))
					{
						list.Add(foundModels[num3]);
					}
				}
			}
			foundModels = list;
		}
		return foundModels;
	}

	private string FindMaterial(string material)
	{
		for (int i = 0; i < materials.Count; i++)
		{
			if (string.Compare(materials[i], material, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return material;
			}
		}
		return null;
	}
}
