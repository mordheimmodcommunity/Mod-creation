using UnityEngine;
using UnityEngine.UI;

public class HireUnitDescription : MonoBehaviour
{
	public Text unitName;

	public Text unitType;

	public Text cost;

	public Image icon;

	public Text rank;

	public Image unitTypeIcon;

	public ToggleEffects btnBuy;

	public virtual void Set(Unit unit)
	{
		if ((Object)(object)unitName != null)
		{
			unitName.set_text(unit.Name);
		}
		if ((Object)(object)unitType != null)
		{
			unitType.set_text(unit.LocalizedType);
		}
		icon.set_sprite(unit.GetIcon());
		if ((Object)(object)unitTypeIcon != null)
		{
			switch (unit.GetUnitTypeId())
			{
			case UnitTypeId.LEADER:
				((Behaviour)(object)unitTypeIcon).enabled = true;
				unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true));
				break;
			case UnitTypeId.HERO_1:
			case UnitTypeId.HERO_2:
			case UnitTypeId.HERO_3:
				((Behaviour)(object)unitTypeIcon).enabled = true;
				unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
				break;
			case UnitTypeId.IMPRESSIVE:
				((Behaviour)(object)unitTypeIcon).enabled = true;
				unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
				break;
			default:
				((Behaviour)(object)unitTypeIcon).enabled = false;
				break;
			}
		}
		rank.set_text(unit.Rank.ToString());
		cost.set_text(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitHireCost(unit).ToString());
		if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitHireCost(unit))
		{
			((Graphic)cost).set_color(Color.red);
		}
	}

	public void Set(UnitId unitId, int unitRank)
	{
		if ((Object)(object)unitName != null)
		{
			unitName.set_text(string.Empty);
		}
		UnitData unitData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>((int)unitId);
		if ((Object)(object)unitType != null)
		{
			unitType.set_text(string.Format("{0} / {1}", PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + unitData.UnitTypeId.ToLowerString()), PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_" + unitId.ToLowerString())));
		}
		icon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("unit/" + unitId.ToLowerString(), cached: true));
		if ((Object)(object)unitTypeIcon != null)
		{
			switch (unitData.UnitTypeId)
			{
			case UnitTypeId.LEADER:
				((Behaviour)(object)unitTypeIcon).enabled = true;
				unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader"));
				break;
			case UnitTypeId.HERO_1:
			case UnitTypeId.HERO_2:
			case UnitTypeId.HERO_3:
				((Behaviour)(object)unitTypeIcon).enabled = true;
				unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
				break;
			case UnitTypeId.IMPRESSIVE:
				((Behaviour)(object)unitTypeIcon).enabled = true;
				unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
				break;
			default:
				((Behaviour)(object)unitTypeIcon).enabled = false;
				break;
			}
		}
		rank.set_text(unitRank.ToString());
		((Component)(object)cost).transform.parent.gameObject.SetActive(value: false);
	}
}
